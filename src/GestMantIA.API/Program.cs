using System.Reflection;
using System.IO;
using GestMantIA.API.Configuration;
using GestMantIA.API.Extensions;
using GestMantIA.Application;
using GestMantIA.Application.Interfaces; // Para IDatabaseInitializer
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Infrastructure;
using GestMantIA.Infrastructure.Data;
using GestMantIA.Infrastructure.Services.Security;
// using GestMantIA.Core.Entities.Identity; // Comentado temporalmente, corregido abajo
// using GestMantIA.Infrastructure.Identity; // Comentado temporalmente ya que SpanishIdentityErrorDescriber no se encuentra
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;
using Serilog.Sinks.SystemConsole.Themes;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;

// Punto de entrada de la aplicación
public class Program
{
    public static async Task Main(string[] args)
    {
        // Crear el builder de la aplicación
        var builder = WebApplication.CreateBuilder(args);

        // Configurar Kestrel
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            // Configurar límites
            serverOptions.Limits.MaxConcurrentConnections = 100;
            serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
            serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
            serverOptions.Limits.MinRequestBodyDataRate = null;
            serverOptions.Limits.MinResponseDataRate = null;
            serverOptions.AddServerHeader = false;

            // Configurar endpoints desde configuración
            var kestrelSection = builder.Configuration.GetSection("Kestrel");
            if (kestrelSection.Exists())
            {
                serverOptions.Configure(kestrelSection);
            }
        });

        // Configurar el host para usar el proveedor de configuración de variables de entorno
        builder.Configuration.AddEnvironmentVariables();

        // Configurar métricas
        builder.Services.AddMetricsConfiguration(builder.Configuration);

        // Configurar el host para usar métricas
        builder.Host.ConfigureMetricsWithDefaults(metricsBuilder =>
        {
            // metricsBuilder.OutputMetrics.AsPrometheusPlainText(); // Configurado globalmente al añadir IMetricsRoot
        });

        // Configurar el host para usar métricas de aplicación
        builder.Host.UseMetrics();
        builder.Host.UseMetricsWebTracking();

        // Add services to the container.
        // La configuración de controladores se mueve a AddPresentationServices
        // builder.Services.AddControllers()
        //     .ConfigureApiBehaviorOptions(options =>
        //     {
        //         // Personalizar la respuesta de validación para mantener consistencia
        //         options.InvalidModelStateResponseFactory = context =>
        //         {
        //             var result = new BadRequestObjectResult(context.ModelState);
        //             result.ContentTypes.Add("application/problem+json");
        //             return result;
        //         };
        //     })
        //     .AddJsonOptions(options =>
        //     {
        //         options.JsonSerializerOptions.PropertyNamingPolicy = null; // Mantener el mismo formato de propiedades
        //         options.JsonSerializerOptions.WriteIndented = true;
        //     })
        //     .AddApplicationPart(Assembly.GetExecutingAssembly()) // Asegurar que todos los controladores del ensamblado actual se descubran
        //     .AddControllersAsServices(); // Registrar controladores como servicios para DI
        // Fin de la configuración de controladores movida

        // Configuración de validación personalizada
        builder.Services.AddCustomValidation();

        // Configuración de Serilog
        var logPath = Path.Combine("Logs", "log-.txt");
        Directory.CreateDirectory("Logs"); // Asegurarse de que el directorio existe

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("Iniciando la aplicación...");
            
            // Configuración de logging estándar (se mantiene para compatibilidad)
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Error al configurar el sistema de logging");
            throw;
        }

        // Configuración de servicios de infraestructura (incluye persistencia, identidad, utilidades)
        builder.Services.AddInfrastructure(builder.Configuration);

        // Registro de repositorios vertical slice (User y Role)
        builder.Services.AddScoped<GestMantIA.Core.Identity.Interfaces.IUserRepository, GestMantIA.Infrastructure.Features.UserManagement.Repositories.UserRepository>();
        builder.Services.AddScoped<GestMantIA.Core.Identity.Interfaces.IRoleRepository, GestMantIA.Infrastructure.Features.UserManagement.Repositories.RoleRepository>();

        // Configuración de servicios de aplicación (AutoMapper, MediatR, etc.)
        builder.Services.AddApplicationServices(builder.Configuration);

        // Configuración de servicios de seguridad (Identity, JWT)
        builder.Services.AddSecurityServices(builder.Configuration);

        // Configuración de servicios de presentación (Controllers, CORS, Swagger, HealthChecks)
        builder.Services.AddPresentationServices(builder.Configuration);

        // Configuración de notificaciones de seguridad
        builder.Services.Configure<SecurityNotificationOptions>(
            builder.Configuration.GetSection("SecurityNotifications"));

        // Registrar servicios de seguridad
        builder.Services.AddScoped<ISecurityLogger, SecurityLogger>();
        builder.Services.AddScoped<ISecurityNotificationService, SecurityNotificationService>();

        // Configuración de políticas de autorización para notificaciones de seguridad
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ViewSecurityNotifications", policy =>
                policy.RequireClaim("permission", "security.notifications.view"));

            options.AddPolicy("ManageSecurityAlerts", policy =>
                policy.RequireClaim("permission", "security.alerts.manage"));
        });

        // La configuración de controladores se ha consolidado en la llamada anterior a AddControllers().

        // La configuración de CORS se mueve a AddPresentationServices
        // La lógica de eventos JWT (OnChallenge, OnForbidden) se ha movido a SecurityServiceExtensions.cs
        // }); // Esta llave de cierre pertenecía al AddJwtBearer que ya no está aquí.

        // Asegurarse de que los controladores se registren correctamente
        builder.Services.AddMvcCore()
            .AddApiExplorer()
            .AddApplicationPart(Assembly.GetExecutingAssembly())
            .AddControllersAsServices();

        // Configuración de AutoMapper
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        var app = builder.Build();

        // Habilitar Swagger en entorno Development
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Configurar el pipeline de la API usando el método de extensión
        app.ConfigureApiPipeline(app.Environment);

        // Inicializar la base de datos (si es necesario)
        await SeedDatabaseAsync(app);

        // Iniciar la aplicación
        await app.RunAsync();
    }

    private static async Task SeedDatabaseAsync(WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            // Aplicar migraciones para ApplicationDbContext
            var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            logger.LogInformation("Verificando y aplicando migraciones para ApplicationDbContext (esto puede crear la base de datos si no existe)...");
            await appDbContext.Database.MigrateAsync();
            logger.LogInformation("Base de datos ApplicationDbContext actualizada correctamente mediante migraciones.");

            if (app.Environment.IsDevelopment())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
                await initializer.SeedDataAsync(); // Sembrar datos siempre en desarrollo
            }
        } // Cierre del bloque try
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error al aplicar migraciones");
            throw;
        }


    }
}
