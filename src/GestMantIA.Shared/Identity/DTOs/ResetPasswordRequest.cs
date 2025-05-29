using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs
{
    /// <summary>
    /// DTO para la solicitud de restablecimiento de contraseña.
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ResetPasswordRequest"/>.
        /// </summary>
        public ResetPasswordRequest()
        {
            Token = string.Empty;
            Email = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        /// <summary>
        /// Token de restablecimiento de contraseña.
        /// </summary>
        [Required(ErrorMessage = "El token de restablecimiento es obligatorio")]
        public string Token { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; }

        /// <summary>
        /// Nueva contraseña.
        /// </summary>
        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        /// <summary>
        /// Confirmación de la nueva contraseña.
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
    }
}
