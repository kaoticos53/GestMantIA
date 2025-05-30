using GestMantIA.Core.Identity.Results;
using GestMantIA.Shared.Identity.DTOs;

namespace GestMantIA.Core.Identity.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de autenticación de usuarios.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Autentica a un usuario con su nombre de usuario y contraseña.
        /// </summary>
        /// <param name="request">Datos de inicio de sesión del usuario.</param>
        /// <param name="ipAddress">Dirección IP del cliente.</param>
        /// <returns>Resultado de la autenticación con el token JWT y el token de actualización.</returns>
        Task<AuthenticationResult> AuthenticateAsync(LoginRequest request, string ipAddress);

        /// <summary>
        /// Refresca un token JWT utilizando un token de actualización.
        /// </summary>
        /// <param name="token">Token de actualización.</param>
        /// <param name="ipAddress">Dirección IP del cliente.</param>
        /// <returns>Nuevo token JWT y token de actualización.</returns>
        Task<AuthenticationResult> RefreshTokenAsync(string token, string ipAddress);

        /// <summary>
        /// Revoca un token de actualización.
        /// </summary>
        /// <param name="token">Token de actualización a revocar.</param>
        /// <param name="ipAddress">Dirección IP del cliente.</param>
        /// <returns>True si se revocó el token; de lo contrario, false.</returns>
        Task<bool> RevokeTokenAsync(string token, string ipAddress);

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="request">Datos del nuevo usuario.</param>
        /// <param name="origin">Origen de la solicitud (para envío de correos electrónicos).</param>
        /// <returns>Resultado del registro.</returns>
        Task<RegisterResult> RegisterAsync(RegisterRequest request, string origin);

        /// <summary>
        /// Verifica la dirección de correo electrónico de un usuario.
        /// </summary>
        /// <param name="token">Token de verificación.</param>
        /// <returns>Resultado de la verificación.</returns>
        Task<VerifyEmailResult> VerifyEmailAsync(string token);

        /// <summary>
        /// Inicia el proceso de restablecimiento de contraseña para un correo electrónico.
        /// </summary>
        /// <param name="request">Solicitud de olvido de contraseña.</param>
        /// <param name="origin">Origen de la solicitud (para envío de correos electrónicos).</param>
        /// <returns>Resultado de la operación.</returns>
        Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);

        /// <summary>
        /// Restablece la contraseña de un usuario.
        /// </summary>
        /// <param name="request">Datos para el restablecimiento de contraseña.</param>
        /// <returns>Resultado de la operación.</returns>
        Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request);

        /// <summary>
        /// Genera la configuración para la autenticación de dos factores.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="email">Correo electrónico del usuario.</param>
        /// <returns>Resultado con la clave secreta y el código QR.</returns>
        Task<TwoFactorSetupResult> GenerateTwoFactorSetupAsync(string userId, string email);

        /// <summary>
        /// Habilita la autenticación de dos factores para un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="code">Código de verificación.</param>
        /// <returns>Resultado de la operación.</returns>
        Task<OperationResult> EnableTwoFactorAsync(string userId, string code);

        /// <summary>
        /// Deshabilita la autenticación de dos factores para un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Resultado de la operación.</returns>
        Task<OperationResult> DisableTwoFactorAsync(string userId);

        /// <summary>
        /// Verifica el código de autenticación de dos factores.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="code">Código de verificación.</param>
        /// <returns>True si el código es válido; de lo contrario, false.</returns>
        Task<bool> VerifyTwoFactorTokenAsync(string userId, string code);
    }
}
