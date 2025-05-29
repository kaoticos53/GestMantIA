using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs
{
    /// <summary>
    /// Solicitud para verificar un código de autenticación de dos factores.
    /// </summary>
    public class VerifyTwoFactorRequest
    {
        public VerifyTwoFactorRequest()
        {
            Code = string.Empty;
        }

        /// <summary>
        /// Código de verificación de dos factores.
        /// </summary>
        [Required(ErrorMessage = "El código de verificación es obligatorio.")]
        [StringLength(10, ErrorMessage = "El código de verificación debe tener entre {2} y {1} caracteres de longitud.", MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resultado de la verificación de un código de autenticación de dos factores.
    /// </summary>
    public class VerifyTwoFactorResult
    {
        public VerifyTwoFactorResult()
        {
            Message = string.Empty;
        }

        /// <summary>
        /// Indica si el código de verificación es válido.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado de la verificación.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
