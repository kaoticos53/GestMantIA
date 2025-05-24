using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase que representa un permiso en el sistema.
    /// </summary>
    public class ApplicationPermission : BaseEntity
    {
        public ApplicationPermission()
        {
            RolePermissions = new HashSet<ApplicationRolePermission>();
        }

        /// <summary>
        /// Nombre del permiso (ejemplo: "Users.Read").
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Descripción del permiso.
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// Categoría del permiso para agruparlos en la interfaz de usuario.
        /// </summary>
        [StringLength(50)]
        public string Category { get; set; }

        /// <summary>
        /// Relación con los roles que tienen este permiso.
        /// </summary>
        public virtual ICollection<ApplicationRolePermission> RolePermissions { get; set; }
    }
}
