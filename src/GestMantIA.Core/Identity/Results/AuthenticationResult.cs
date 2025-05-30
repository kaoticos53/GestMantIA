namespace GestMantIA.Core.Identity.Results
{
    /// <summary>
    /// Resultado de una operación de autenticación exitosa.
    /// </summary>
    public class AuthenticationResult : AuthResult
    {
        public AuthenticationResult()
        {
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
            UserInfo = new UserInfo();
            Errors = new List<string>();
            Succeeded = false;
            Message = "Operación completada";
        }
        /// <summary>
        /// Token de acceso JWT para autenticar solicitudes posteriores.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Token de actualización para obtener un nuevo token de acceso cuando expire.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Fecha de expiración del token de acceso.
        /// </summary>
        public DateTime AccessTokenExpiration { get; set; }

        /// <summary>
        /// Información básica del usuario autenticado.
        /// </summary>
        public UserInfo? UserInfo { get; set; }

        /// <summary>
        /// Crea un resultado de autenticación exitoso.
        /// </summary>
        /// <param name="accessToken">Token de acceso JWT.</param>
        /// <param name="refreshToken">Token de actualización.</param>
        /// <param name="accessTokenExpiration">Fecha de expiración del token de acceso.</param>
        /// <param name="userInfo">Información del usuario autenticado.</param>
        /// <param name="message">Mensaje descriptivo opcional.</param>
        /// <returns>Instancia de AuthenticationResult con los tokens correspondientes.</returns>
        public static AuthenticationResult Success(
            string accessToken,
            string refreshToken,
            DateTime accessTokenExpiration,
            UserInfo userInfo,
            string message = null)
        {
            return new AuthenticationResult
            {
                Succeeded = true,
                Message = message ?? "Autenticación exitosa",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = accessTokenExpiration,
                UserInfo = userInfo
            };
        }

        /// <summary>
        /// Crea un resultado de autenticación fallido.
        /// </summary>
        /// <param name="errors">Lista de errores.</param>
        /// <param name="message">Mensaje descriptivo opcional.</param>
        /// <returns>Instancia de AuthenticationResult con Succeeded en false.</returns>
        public static AuthenticationResult Failure(IEnumerable<string>? errors, string? message = null)
        {
            return new AuthenticationResult
            {
                Succeeded = false,
                Message = message ?? "Error de autenticación",
                Errors = errors ?? new List<string>()
            };
        }
    }

    // La clase UserInfo ha sido movida a su propio archivo UserInfo.cs
}
