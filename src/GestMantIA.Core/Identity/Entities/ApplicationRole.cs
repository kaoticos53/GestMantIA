using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase que representa un rol en el sistema.
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() : base()
        {
            Id = Guid.NewGuid();
            Description = string.Empty;
            UserRoles = new HashSet<ApplicationUserRole>();
            RolePermissions = new HashSet<ApplicationRolePermission>();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor con nombre del rol
        /// </summary>
        /// <param name="roleName">Nombre del rol</param>
        public ApplicationRole(string roleName) : base(roleName)
        {
            Id = Guid.NewGuid();
            Description = string.Empty;
            UserRoles = new HashSet<ApplicationUserRole>();
            RolePermissions = new HashSet<ApplicationRolePermission>();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Descripción del rol.
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// Relación con los usuarios que tienen este rol.
        /// </summary>
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        /// <summary>
        /// Relación con los permisos asignados a este rol.
        /// </summary>
        public virtual ICollection<ApplicationRolePermission> RolePermissions { get; set; }

        /// <summary>
        /// Fecha y hora de creación del rol.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha y hora de la última modificación del rol.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Normaliza el nombre del rol (a mayúsculas).
        /// </summary>
        public void NormalizeName()
        {
            base.NormalizedName = Name?.ToUpperInvariant() ?? string.Empty;
        }
    }
}
