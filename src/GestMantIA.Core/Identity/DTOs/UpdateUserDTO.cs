using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Core.Identity.DTOs
{
    /// <summary>
    /// DTO para la actualización de un usuario existente.
    /// </summary>
    public class UpdateUserDTO
    {
        /// <summary>
        /// ID del usuario a actualizar.
        /// </summary>
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario para el inicio de sesión.
        /// </summary>
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public string? Username { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string? Email { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Apellidos del usuario.
        /// </summary>
        [StringLength(100, ErrorMessage = "Los apellidos no pueden tener más de 100 caracteres")]
        public string? LastName { get; set; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        [Phone(ErrorMessage = "El formato del número de teléfono no es válido")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Indica si el usuario está activo en el sistema.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Indica si el correo electrónico está confirmado.
        /// </summary>
        public bool? EmailConfirmed { get; set; }

        /// <summary>
        /// Indica si el número de teléfono está confirmado.
        /// </summary>
        public bool? PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Indica si está habilitado el bloqueo por dos factores.
        /// </summary>
        public bool? TwoFactorEnabled { get; set; }
    }
}
