using AutoMapper;

namespace GestMantIA.Infrastructure.Mappings
{
    /// <summary>
    /// Clase para registrar todos los perfiles de mapeo de la aplicación.
    /// </summary>
    public static class MappingProfile
    {
        /// <summary>
        /// Configura los perfiles de mapeo de AutoMapper.
        /// </summary>
        /// <param name="services">Colección de servicios.</param>
        /// <returns>La configuración de AutoMapper.</returns>
        public static MapperConfiguration ConfigureAutoMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                // Registrar todos los perfiles de mapeo
                cfg.AddProfile(new UserProfileMapping());
                cfg.AddProfile(new UserManagementMapping());
                
                // Agregar aquí otros perfiles de mapeo según sea necesario
            });
        }
    }
}
