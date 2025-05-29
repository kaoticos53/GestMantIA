using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs
{
    /// <summary>
    /// DTO para la solicitud de restablecimiento de contraseña.
    /// </summary>
    public class ForgotPasswordRequest
    {
        /// <summary>
        /// Correo electrónico del usuario que olvidó su contraseña.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public required string Email { get; set; }
    }
}
