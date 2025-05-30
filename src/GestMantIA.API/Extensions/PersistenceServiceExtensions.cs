namespace GestMantIA.API.Extensions
{
    public static class PersistenceServiceExtensions
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ESTA CLASE Y MÉTODO SON OBSOLETOS.
            // La configuración de persistencia se ha movido a GestMantIA.Infrastructure.DependencyInjection.AddPersistence().
            // Program.cs ahora llama a services.AddInfrastructure(configuration) que incluye la configuración de persistencia.
            // Esta clase (PersistenceServiceExtensions.cs) puede ser eliminada del proyecto.

            // No se realiza ninguna operación aquí.
            return services;
        }
    }
}
