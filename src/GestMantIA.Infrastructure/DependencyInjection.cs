using System.Text;
using GestMantIA.Core.Identity;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Interfaces;
using GestMantIA.Infrastructure.Data;
using GestMantIA.Infrastructure.Services;
using GestMantIA.Infrastructure.Services.Auth;
using GestMantIA.Infrastructure.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using GestMantIA.Core.Configuration;
using GestMantIA.Infrastructure.Identity.Factories;

namespace GestMantIA.Infrastructure
{
    /// <summary>
    /// Clase de extensión para configurar la inyección de dependencias de la capa de Infraestructura.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Agrega los servicios de infraestructura al contenedor de dependencias.
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>La colección de servicios para encadenamiento</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityServices(configuration);
            services.AddApplicationUtilities(configuration);

            return services;
        }

        private static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar Identity con Guid como tipo de clave
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Configuración de opciones de usuario
                options.User.RequireUniqueEmail = true;
                
                // Configuración de opciones de contraseña
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                
                // Configuración de bloqueo de cuenta
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                
                // Configuración de inicio de sesión
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>() // Registrar la factoría personalizada
            .AddDefaultTokenProviders()
            .AddUserManager<UserManager<ApplicationUser>>()
            .AddRoleManager<RoleManager<ApplicationRole>>()
            .AddSignInManager<SignInManager<ApplicationUser>>();
            
            // Configurar el almacenamiento de tokens
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(2);
            });

            // La configuración de autenticación JWT se ha movido a Program.cs

            // Configurar servicios de autenticación
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<Core.Identity.Interfaces.IUserService, UserService>();
            services.AddScoped<Core.Identity.Interfaces.IRoleService, RoleService>();

            return services;
        }

        private static IServiceCollection AddApplicationUtilities(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar el servicio de correo electrónico
            // En desarrollo, usamos DevelopmentEmailService que solo registra los correos
            // En producción, se debe configurar un servicio real de envío de correos
            services.AddTransient<IEmailService, DevelopmentEmailService>();
            
            // TODO: Registrar aquí otros servicios de utilidad general de infraestructura si es necesario

            return services;
        }

    }
}
