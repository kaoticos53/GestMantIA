using System;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase que representa la relación muchos a muchos entre usuarios y roles.
    /// </summary>
    public class ApplicationUserRole : BaseEntity
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ApplicationUserRole"/>
        /// </summary>
        public ApplicationUserRole()
        {
            User = null!; // Inicialización forzada, se debe establecer después
            Role = null!; // Inicialización forzada, se debe establecer después
        }

        /// <summary>
        /// Identificador del usuario.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Usuario asociado.
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// Identificador del rol.
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Rol asociado.
        /// </summary>
        public virtual ApplicationRole Role { get; set; }
    }
}
