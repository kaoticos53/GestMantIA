using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs.Requests
{
    /// <summary>
    /// DTO para restablecer la contraseña de un usuario.
    /// </summary>
    public class ResetPasswordDTO
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Token de restablecimiento de contraseña.
        /// </summary>
        [Required(ErrorMessage = "El token es obligatorio.")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Nueva contraseña.
        /// </summary>
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres y un máximo de {1}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la nueva contraseña.
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La nueva contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
