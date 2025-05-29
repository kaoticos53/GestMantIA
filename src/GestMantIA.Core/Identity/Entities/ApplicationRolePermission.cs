using Microsoft.AspNetCore.Identity;
using System;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase que representa la relación muchos a muchos entre roles y permisos.
    /// </summary>
    public class ApplicationRolePermission 
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ApplicationRolePermission"/>
        /// </summary>
        public ApplicationRolePermission()
        {
            // Inicialización de propiedades de navegación
            Role = null!;
            Permission = null!;
        }

        /// <summary>
        /// Identificador del rol.
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Rol asociado.
        /// </summary>
        public virtual ApplicationRole Role { get; set; }

        /// <summary>
        /// Identificador del permiso.
        /// </summary>
        public Guid PermissionId { get; set; }

        /// <summary>
        /// Permiso asociado.
        /// </summary>
        public virtual ApplicationPermission Permission { get; set; }
    }
}
