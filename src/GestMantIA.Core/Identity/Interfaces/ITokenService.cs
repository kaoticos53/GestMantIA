using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Core.Identity.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de generación y validación de tokens JWT.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Genera un token de acceso JWT para el usuario especificado.
        /// </summary>
        /// <param name="user">Usuario para el que se generará el token.</param>
        /// <returns>Token de acceso JWT como cadena.</returns>
        string GenerateAccessToken(ApplicationUser user);

        /// <summary>
        /// Genera un token de actualización para el usuario especificado.
        /// </summary>
        /// <param name="user">Usuario para el que se generará el token de actualización.</param>
        /// <param name="ipAddress">Dirección IP desde la que se solicita el token.</param>
        /// <returns>Token de actualización.</returns>
        Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, string ipAddress);

        /// <summary>
        /// Obtiene los claims del usuario a partir de un token JWT.
        /// </summary>
        /// <param name="token">Token JWT.</param>
        /// <returns>Lista de claims del token.</returns>
        IEnumerable<Claim> GetClaimsFromToken(string token);

        /// <summary>
        /// Valida un token JWT.
        /// </summary>
        /// <param name="token">Token JWT a validar.</param>
        /// <returns>True si el token es válido; de lo contrario, false.</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// Obtiene el identificador de usuario a partir de un token JWT.
        /// </summary>
        /// <param name="token">Token JWT.</param>
        /// <returns>Identificador del usuario o null si no se puede obtener.</returns>
        string GetUserIdFromToken(string token);
    }
}
