// Para GestMantIA.Core.Identity.Interfaces.IUserRepository
using GestMantIA.Application.Interfaces; // Para IDatabaseInitializer
using GestMantIA.Core.Configuration;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Interfaces;
using GestMantIA.Infrastructure.Data;
using GestMantIA.Infrastructure.Features.UserManagement.Repositories;
using GestMantIA.Infrastructure.Identity.Factories;
using GestMantIA.Infrastructure.Services.Auth;
using GestMantIA.Infrastructure.Services.Email;
using GestMantIA.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting; // Para IWebHostEnvironment
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            services.AddPersistence(configuration); // Añadido para centralizar registros de persistencia
            services.AddIdentityServices(configuration);
            services.AddApplicationUtilities(configuration);

            // Registrar GestMantIA.Core.Identity.Interfaces.IUserRepository, IGestMantIA.Infrastructure.Features.UserManagement.Repositories.RoleRepository y IUnitOfWork
            services.AddScoped<GestMantIA.Core.Identity.Interfaces.IUserRepository, UserRepository>();
            services.AddScoped<GestMantIA.Core.Identity.Interfaces.IRoleRepository, GestMantIA.Infrastructure.Features.UserManagement.Repositories.RoleRepository>();
            services.AddScoped<GestMantIA.Core.Identity.Interfaces.IRoleService, GestMantIA.Application.Features.UserManagement.Services.RoleService>();
            services.AddScoped<GestMantIA.Core.Repositories.IUserProfileRepository, GestMantIA.Infrastructure.Features.UserManagement.Repositories.UserProfileRepository>();
            // Registro de AutoMapper para inyección de dependencias
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); // Carga todos los perfiles de mapeo

            services.AddScoped<IUnitOfWork, UnitOfWork>();

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
            .AddSignInManager<CustomSignInManager>();

            // Configurar el almacenamiento de tokens
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(2);
            });

            // La configuración de autenticación JWT se ha movido a Program.cs

            // Configurar servicios de autenticación
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // Servicio para la gestión de cuentas, incluyendo recuperación de contraseña
            services.AddScoped<IAccountService, AccountService>();

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

        private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar la configuración de datos iniciales
            services.Configure<SeedDataSettings>(configuration.GetSection("SeedData"));

            // Registrar el inicializador de base de datos
            services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

            // Configurar el contexto de base de datos principal (ApplicationDbContext)
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var env = sp.GetRequiredService<IWebHostEnvironment>(); // Necesita Microsoft.AspNetCore.Hosting
                try
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection' en la configuración.");
                    }

                    var dbContextLogger = loggerFactory.CreateLogger<ApplicationDbContext>();
                    dbContextLogger.LogInformation("[DB] Configurando ApplicationDbContext con cadena de conexión: {ConnectionString}", connectionString);

                    options.UseNpgsql(
                        connectionString,
                        b =>
                        {
                            b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
                            b.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorCodesToAdd: null);
                        });

                    // En desarrollo, habilitar logging sensible y EF Core logging
                    // Nota: IWebHostEnvironment podría no estar disponible aquí directamente si esta capa no debe conocer el entorno de hosting.
                    // Considerar pasar `env.IsDevelopment()` como parámetro o usar directivas de compilación si es un problema.
                    // Por ahora, se asume que IWebHostEnvironment está disponible o se gestionará.
                    if (sp.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging(true);
                        // options.LogTo(Console.WriteLine, LogLevel.Information); // Ejemplo de log a consola
                        options.LogTo(message => loggerFactory.CreateLogger("EFCore").LogInformation(message), LogLevel.Information);
                    }
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger("GestMantIA.Infrastructure.DependencyInjection"); // Cambiado el nombre del logger
                    logger.LogError(ex, "[ERROR] Error al configurar ApplicationDbContext.");
                    throw;
                }
            });

            // Registrar Unit of Work
            // Nota: UnitOfWork.cs está en GestMantIA.Infrastructure.Data, así que el using GestMantIA.Infrastructure.Data.Repositories no es estrictamente necesario si se usa el namespace completo o si UnitOfWork está directamente en Data.
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Registrar el contexto de base de datos genérico para que pueda ser inyectado donde se requiera DbContext
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // Registrar repositorios genéricos y específicos si es necesario (ejemplo)
            // services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            // services.AddScoped<GestMantIA.Core.Identity.Interfaces.IUserRepository, UserRepository>(); // Ejemplo de repositorio específico
            // services.AddScoped<IUserProfileRepository, UserProfileRepository>();

            return services;
        }
    }
}
