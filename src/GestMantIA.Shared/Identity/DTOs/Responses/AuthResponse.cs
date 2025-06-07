namespace GestMantIA.Shared.Identity.DTOs.Responses
{
    /// <summary>
    /// DTO para la respuesta de autenticación exitosa.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AuthResponse"/>
        /// </summary>
        public AuthResponse()
        {
            Token = string.Empty;
            RefreshToken = string.Empty;
            
        }

        /// <summary>
        /// Token de acceso JWT.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Token de actualización para obtener un nuevo token de acceso.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Fecha de expiración del token de acceso.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>
        /// Información del usuario autenticado.
        /// </summary>
        public UserResponseDTO? User { get; set; }
    }
}
