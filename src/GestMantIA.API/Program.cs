using GestMantIA.API.Extensions;
using GestMantIA.Core.Interfaces;
using GestMantIA.Infrastructure;
using GestMantIA.Infrastructure.Data;
using GestMantIA.Infrastructure.Services.Email;
using GestMantIA.Infrastructure.Services.Security;
using GestMantIA.Core.Identity.Interfaces;
// using GestMantIA.Core.Entities.Identity; // Comentado temporalmente, corregido abajo
using GestMantIA.Core.Identity.Entities; // Para ApplicationUser y ApplicationRole
using Microsoft.AspNetCore.Authentication; // Para AuthenticationOptions
// using GestMantIA.Infrastructure.Identity; // Comentado temporalmente ya que SpanishIdentityErrorDescriber no se encuentra
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using System.Text;
using System.Reflection;
using GestMantIA.API.Controllers;
using GestMantIA.API.Filters.Swagger;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;

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

        // Configuración de logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        
        // Configurar el nivel de log para Entity Framework Core
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

        // Configuración de servicios de persistencia
        builder.Services.AddPersistenceServices(builder.Configuration);

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

        // Configuración de autorización global
        builder.Services.AddAuthorization(options =>
        {
            // Política por defecto: requiere autenticación
            var defaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.DefaultPolicy = defaultPolicy;

            // Políticas personalizadas
            options.AddPolicy("RequireAdminRole", policy => 
                policy.RequireRole("Admin"));
        });

        // Esta configuración duplicada de AddControllers() también se elimina ya que la principal se movió.
        
        // Asegurarse de que los controladores se registren correctamente
        builder.Services.AddMvcCore()
            .AddApiExplorer()
            .AddApplicationPart(Assembly.GetExecutingAssembly())
            .AddControllersAsServices();

        // Configuración de AutoMapper
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        var app = builder.Build();

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
            logger.LogInformation("Aplicando migraciones para ApplicationDbContext...");
            await appDbContext.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas correctamente para ApplicationDbContext");
            
            if (app.Environment.EnvironmentName == "Development" && app.Configuration.GetSection("Database").GetValue<bool>("SeedData"))
            {
                var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
                await initializer.SeedDataAsync(); // Llamar al método correcto para sembrar datos
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
