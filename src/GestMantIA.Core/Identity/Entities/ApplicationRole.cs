using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase que representa un rol en el sistema.
    /// </summary>
    public class ApplicationRole : IdentityRole<string>
    {
        public ApplicationRole()
        {
            Name = string.Empty;
            NormalizedName = string.Empty;
            Description = string.Empty;
            UserRoles = new HashSet<ApplicationUserRole>();
            RolePermissions = new HashSet<ApplicationRolePermission>();
        }

        /// <summary>
        /// Nombre del rol (ejemplo: "Admin", "User", etc.).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Nombre normalizado del rol (en mayúsculas para búsquedas sin distinción de mayúsculas/minúsculas).
        /// </summary>
        public string NormalizedName { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del rol.
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Relación con los usuarios que tienen este rol.
        /// </summary>
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        /// <summary>
        /// Relación con los permisos asignados a este rol.
        /// </summary>
        public virtual ICollection<ApplicationRolePermission> RolePermissions { get; set; }

        /// <summary>
        /// Normaliza el nombre del rol (a mayúsculas).
        /// </summary>
        public void NormalizeName()
        {
            NormalizedName = Name?.ToUpperInvariant();
        }
    }
}
