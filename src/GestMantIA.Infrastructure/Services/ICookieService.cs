using System;

namespace GestMantIA.Infrastructure.Services
{
    /// <summary>
    /// Servicio para el manejo de cookies de autenticación
    /// </summary>
    public interface ICookieService
    {
        /// <summary>
        /// Establece la cookie de refresh token
        /// </summary>
        /// <param name="token">Token de refresco</param>
        /// <param name="expires">Fecha de expiración (opcional)</param>
        void SetRefreshTokenCookie(string token, DateTime? expires = null);

        /// <summary>
        /// Obtiene el refresh token de la cookie
        /// </summary>
        /// <returns>Token de refresco o null si no existe</returns>
        string? GetRefreshTokenFromCookie();

        /// <summary>
        /// Elimina la cookie de refresh token
        /// </summary>
        void DeleteRefreshTokenCookie();
    }
}
