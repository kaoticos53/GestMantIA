using Microsoft.AspNetCore.Identity;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase que representa la relación muchos a muchos entre usuarios y roles.
    /// </summary>
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ApplicationUserRole"/>
        /// </summary>
        public ApplicationUserRole()
        {
            // Inicialización de propiedades de navegación
            User = null!;
            Role = null!;
        }

        /// <summary>
        /// Usuario asociado.
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// Rol asociado.
        /// </summary>
        public virtual ApplicationRole Role { get; set; }
    }
}
