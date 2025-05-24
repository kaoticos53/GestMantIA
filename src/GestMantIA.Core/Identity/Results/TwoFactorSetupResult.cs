using System;
using GestMantIA.Core.Identity.Results;

namespace GestMantIA.Core.Identity.Results
{
    /// <summary>
    /// Resultado de la configuración de autenticación de dos factores.
    /// </summary>
    public class TwoFactorSetupResult : OperationResult
    {
        /// <summary>
        /// Obtiene o establece la clave secreta para la autenticación de dos factores.
        /// </summary>
        public string? SharedKey { get; set; }

        /// <summary>
        /// Obtiene o establece la URL del código QR en formato base64.
        /// </summary>
        public string? QrCodeImageUrl { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TwoFactorSetupResult"/>.
        /// </summary>
        public TwoFactorSetupResult() { }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TwoFactorSetupResult"/> con un mensaje de error.
        /// </summary>
        /// <param name="message">Mensaje de error.</param>
        public TwoFactorSetupResult(string message) : base()
        {
            Succeeded = false;
            Message = message;
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TwoFactorSetupResult"/> con una clave secreta y una URL de código QR.
        /// </summary>
        /// <param name="sharedKey">Clave secreta.</param>
        /// <param name="qrCodeImageUrl">URL del código QR en formato base64.</param>
        public TwoFactorSetupResult(string sharedKey, string qrCodeImageUrl)
        {
            Succeeded = true;
            SharedKey = sharedKey;
            QrCodeImageUrl = qrCodeImageUrl;
        }
    }
}
