using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase que representa un permiso en el sistema.
    /// </summary>
    public class ApplicationPermission
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ApplicationPermission"/>
        /// </summary>
        public ApplicationPermission()
        {
            RolePermissions = new HashSet<ApplicationRolePermission>();
        }

        /// <summary>
        /// Identificador único del permiso.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del permiso (ejemplo: "Users.Read").
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del permiso.
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Categoría del permiso para agruparlos en la interfaz de usuario.
        /// </summary>
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de claim asociado al permiso.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ClaimType { get; set; } = string.Empty;

        /// <summary>
        /// Valor de claim asociado al permiso.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ClaimValue { get; set; } = string.Empty;

        /// <summary>
        /// Relación con los roles que tienen este permiso.
        /// </summary>
        public virtual ICollection<ApplicationRolePermission> RolePermissions { get; set; }
    }
}
