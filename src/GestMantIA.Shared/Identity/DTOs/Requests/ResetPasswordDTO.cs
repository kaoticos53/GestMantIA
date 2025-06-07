using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs.Requests
{
    /// <summary>
    /// DTO para restablecer la contraseña de un usuario.
    /// </summary>
    public class ResetPasswordDTO
    {
        /// <summary>
        /// Correo electrónico o ID del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico o ID de usuario es obligatorio.")]
        // Consider removing [EmailAddress] if User ID (Guid string) is also accepted.
        // For now, keeping it as per original 'Email' property.
        // [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido si se proporciona un email.")] 
        public string EmailOrUserId { get; set; } = string.Empty;

        /// <summary>
        /// Token de restablecimiento de contraseña.
        /// </summary>
        [Required(ErrorMessage = "El token es obligatorio.")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Nueva contraseña para el usuario.
        /// </summary>
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres de longitud.", MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la nueva contraseña.
        /// </summary>
        [Required(ErrorMessage = "La confirmación de la nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;

    }
}
