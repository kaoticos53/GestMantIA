using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GestMantIA.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Obtener la ruta al directorio del proyecto API, asumiendo una estructura de carpetas estándar
            // donde Infrastructure y API son hermanos bajo una carpeta 'src'
            string apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "GestMantIA.API");
            if (!Directory.Exists(apiProjectPath))
            {
                // Intento alternativo si la estructura es diferente o el comando se ejecuta desde otra ubicación
                apiProjectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "GestMantIA.API"));
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath) // Usar la ruta del proyecto API
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("No se pudo encontrar la cadena de conexión 'DefaultConnection' en appsettings.Development.json. Asegúrate de que el archivo existe en el proyecto API y la clave está presente.");
            }

            builder.UseNpgsql(connectionString, 
                options => options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

            return new ApplicationDbContext(builder.Options);
        }
    }
}
