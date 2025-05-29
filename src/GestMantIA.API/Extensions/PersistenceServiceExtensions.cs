using GestMantIA.Core.Configuration; // For SeedDataSettings
using GestMantIA.Core.Interfaces;
using GestMantIA.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // For LogLevel and ILogger
using System; // For TimeSpan, InvalidOperationException, Console

namespace GestMantIA.API.Extensions
{
    public static class PersistenceServiceExtensions
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar la configuración de datos iniciales
            services.Configure<SeedDataSettings>(configuration.GetSection("SeedData"));
            
            // Registrar el inicializador de base de datos
            services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
            
            // Configurar el contexto de base de datos principal (ApplicationDbContext)
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var env = sp.GetRequiredService<IWebHostEnvironment>();
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
                    
                    if (env.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging(true);
                        options.LogTo(message => loggerFactory.CreateLogger("EFCore").LogInformation(message), LogLevel.Information);
                    }
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger("GestMantIA.API.Extensions.PersistenceServiceExtensions");
                    logger.LogError(ex, "[ERROR] Error al configurar ApplicationDbContext.");
                    throw;
                }
            });

            // Registrar Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Registrar el contexto de base de datos genérico para que pueda ser inyectado donde se requiera DbContext
            // Esto es útil si alguna vez se necesita DbContext directamente en lugar de ApplicationDbContext.
            // Sin embargo, IUnitOfWork y los repositorios específicos son la forma preferida de interactuar con los datos.
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // Registrar repositorios genéricos y específicos si es necesario (ejemplo)
            // services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            // services.AddScoped<IUserRepository, UserRepository>(); // Ejemplo de repositorio específico

            return services;
        }
    }
}
