using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Core.Identity.DTOs
{
    /// <summary>
    /// DTO para la solicitud de verificación de correo electrónico.
    /// </summary>
    public class VerifyEmailRequest
    {
        /// <summary>
        /// ID del usuario.
        /// </summary>
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        public string UserId { get; set; }

        /// <summary>
        /// Token de verificación de correo electrónico.
        /// </summary>
        [Required(ErrorMessage = "El token de verificación es obligatorio")]
        public string Token { get; set; }
    }
}
