using GestMantIA.API.Extensions;
using GestMantIA.Core.Interfaces;
using GestMantIA.Infrastructure;
using GestMantIA.Infrastructure.Data;
using GestMantIA.Infrastructure.Services.Email;
using GestMantIA.Infrastructure.Services.Security;
using GestMantIA.Core.Identity.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// Punto de entrada de la aplicación
public class Program
{
    public static async Task Main(string[] args)
    {
        // Crear el builder de la aplicación
        var builder = WebApplication.CreateBuilder(args);

        // Configurar el host para usar el proveedor de configuración de variables de entorno
        builder.Configuration.AddEnvironmentVariables();

        // Add services to the container.
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                // Personalizar la respuesta de validación para mantener consistencia
                options.InvalidModelStateResponseFactory = context =>
                {
                    var result = new BadRequestObjectResult(context.ModelState);
                    result.ContentTypes.Add("application/problem+json");
                    return result;
                };
            });

        // Configuración de validación personalizada
        builder.Services.AddCustomValidation();

        // Configuración de logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

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

        // Configuración de CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins(
                        "http://localhost:3000", // Ajusta según tu frontend
                        "http://localhost:5000",
                        "https://localhost:5001")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // Configuración de autenticación JWT
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? 
            throw new InvalidOperationException("JWT Secret Key no configurada. Verifica la configuración de la aplicación.");
        var issuer = jwtSettings["Issuer"] ?? "GestMantIA.API";
        var audience = jwtSettings["Audience"] ?? "GestMantIA.Clients";

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

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

        // Configuración de controladores con autorización por defecto
        builder.Services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });

        // Configuración de AutoMapper
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        // Configuración de Swagger
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "GestMantIA API", Version = "v1" });
            
            // Configuración para JWT en Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                              "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                              "Example: \"Bearer 12345abcdef\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });

        // Registrar el servicio de correo electrónico
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<IEmailService, DevelopmentEmailService>();

        // Configuración de la base de datos y servicios de infraestructura
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        // Configurar el contexto de base de datos para migraciones
        if (args.Contains("--migrate"))
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
            return; // Salir después de aplicar migraciones
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Endpoint de salud para health checks
        app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

        await app.RunAsync();
    }
}
