using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs.Requests
{
    /// <summary>
    /// DTO para la creación de un nuevo rol.
    /// </summary>
    public class CreateRoleDto
    {
        /// <summary>
        /// Nombre del rol.
        /// </summary>
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del rol no puede exceder los 100 caracteres.")]
        public required string Name { get; set; }

        /// <summary>
        /// Descripción del rol.
        /// </summary>
        [StringLength(256, ErrorMessage = "La descripción del rol no puede exceder los 256 caracteres.")]
        public string? Description { get; set; }

        // Considerar si se necesitan permisos aquí, aunque el mapping actual los ignora para ApplicationRole
        // public List<string>? Permissions { get; set; } 
    }
}
