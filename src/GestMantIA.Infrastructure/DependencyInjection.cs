using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Interfaces;
using GestMantIA.Infrastructure.Data;
using GestMantIA.Core.Identity;
using GestMantIA.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using GestMantIA.Infrastructure.Services.Email;
using System.Text;

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
            // Configurar el contexto de base de datos de autenticación
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var connectionString = configuration.GetConnectionString("IdentityConnection") ?? 
                                     configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            });

            // Configurar el contexto de base de datos principal
            services.AddDbContext<GestMantIADbContext>((sp, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(GestMantIADbContext).Assembly.FullName));
            });

            // Configurar Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // La configuración de autenticación JWT se ha movido a Program.cs

            // Configurar servicios de autenticación
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            // Configurar AutoMapper con perfiles personalizados
            var mapperConfig = Mappings.MappingProfile.ConfigureAutoMapper();
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            // Registrar Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            // Configurar el servicio de correo electrónico
            // En desarrollo, usamos DevelopmentEmailService que solo registra los correos
            // En producción, se debe configurar un servicio real de envío de correos
            services.AddTransient<IEmailService, DevelopmentEmailService>();

            // Registrar los DbContext como fábricas para inyección en constructores
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<GestMantIADbContext>());
            services.AddScoped<ApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            return services;
        }
    }
}
