using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GestMantIA.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Construir la ruta al directorio del proyecto API asumiendo que la ejecución es desde la raíz del repositorio.
            // Directorio Raíz -> src -> GestMantIA.API
            string baseDirectory = Directory.GetCurrentDirectory(); // d:\source\Repos\GestMantIA cuando se ejecuta 'dotnet ef' desde la raíz
            string apiProjectPath = Path.Combine(baseDirectory, "src", "GestMantIA.API");

            if (!Directory.Exists(apiProjectPath))
            {
                // Fallback por si la ejecución es desde otro lugar, aunque menos probable para 'dotnet ef'
                // Esta lógica puede necesitar ajustes si el contexto de ejecución es muy variable.
                // Por ahora, priorizamos la ejecución desde la raíz del repo.
                string infrastructurePath = AppDomain.CurrentDomain.BaseDirectory; // ...\GestMantIA\src\GestMantIA.Infrastructure\bin\Debug\netX.X
                apiProjectPath = Path.GetFullPath(Path.Combine(infrastructurePath, "..", "..", "..", "..", "GestMantIA.API"));
            }

            if (!Directory.Exists(apiProjectPath))
            {
                throw new InvalidOperationException($"No se pudo encontrar el directorio del proyecto API en '{apiProjectPath}'. Asegúrate de que la ruta es correcta y el proyecto API existe en esa ubicación.");
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath) // Usar la ruta del proyecto API
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // Añadir appsettings.json como fallback o base
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
