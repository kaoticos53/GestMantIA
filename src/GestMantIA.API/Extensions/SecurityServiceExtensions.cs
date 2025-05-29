using GestMantIA.Core.Identity.Entities;
using GestMantIA.Infrastructure.Data; // For ApplicationDbContext
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.Threading.Tasks; // For Task.CompletedTask
using Microsoft.AspNetCore.Http; // For StatusCodes and context.Response

namespace GestMantIA.API.Extensions
{
    public static class SecurityServiceExtensions
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de JWT - Lectura de settings
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? 
                throw new InvalidOperationException("JWT Secret Key no configurada. Verifica la configuración de la aplicación.");
            var issuer = jwtSettings["Issuer"] ?? "GestMantIA.API";
            var audience = jwtSettings["Audience"] ?? "GestMantIA.Clients";

            // Configuración de ASP.NET Core Identity usando AddIdentityCore para mayor control
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Configuración de contraseña
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                // Configuración de bloqueo de usuario
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Configuración de usuario
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Configuración de inicio de sesión (SignInOptions)
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true; // Requerir confirmación de correo electrónico
            })
            .AddRoles<ApplicationRole>() // Añadir gestión de roles
            .AddEntityFrameworkStores<ApplicationDbContext>() // Configurar el UserStore y RoleStore
            .AddSignInManager<SignInManager<ApplicationUser>>() // Añadir SignInManager
            .AddDefaultTokenProviders(); // Para tokens de confirmación de email, reseteo de contraseña, etc.

            // Configurar la autenticación para usar JWT como esquema principal
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; // Establecer JWT como el esquema general por defecto
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
            {
                jwtOptions.SaveToken = true;
                jwtOptions.RequireHttpsMetadata = false; // En desarrollo. Cambiar a true en producción.
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
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
                
                jwtOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // Skip the default logic.
                        context.HandleResponse(); 
                        if (context.AuthenticateFailure != null)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            var authFailureResult = System.Text.Json.JsonSerializer.Serialize(new 
                            {
                                error = "No autorizado. El token no es válido o ha expirado.",
                                tokenExpired = context.AuthenticateFailure is SecurityTokenExpiredException
                            });
                            return context.Response.WriteAsync(authFailureResult);
                        }
                        else if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            var defaultChallengeResult = System.Text.Json.JsonSerializer.Serialize(new { error = "No autorizado. Se requiere autenticación." });
                            return context.Response.WriteAsync(defaultChallengeResult);
                        }
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new { error = "No tiene permiso para acceder a este recurso." });
                        return context.Response.WriteAsync(result);
                    }
                };
            });
            
            // Es posible que se necesite una llamada genérica a AddAuthorization si no está ya presente
            // services.AddAuthorization(); // Esta línea se añade si es necesaria y no para las políticas específicas

            return services;
        }
    }
}
