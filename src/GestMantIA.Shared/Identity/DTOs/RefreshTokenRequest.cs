using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs
{
    /// <summary>
    /// DTO para la solicitud de actualización de token.
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Token de acceso caducado.
        /// </summary>
        [Required(ErrorMessage = "El token de acceso es obligatorio")]
        public required string Token { get; set; }

        /// <summary>
        /// Token de actualización.
        /// </summary>
        [Required(ErrorMessage = "El token de actualización es obligatorio")]
        public required string RefreshToken { get; set; }
    }
}
