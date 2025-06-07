using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs.Requests
{
    public class UpdateRoleDto
    {
        [Required]
        public required string Id { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del rol no puede exceder los 100 caracteres.")]
        public required string Name { get; set; }

        [StringLength(256, ErrorMessage = "La descripción del rol no puede exceder los 256 caracteres.")]
        public string? Description { get; set; }
    }
}
