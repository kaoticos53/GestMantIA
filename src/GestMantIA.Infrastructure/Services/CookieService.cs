using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GestMantIA.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio para manejar cookies de autenticación
    /// </summary>
    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string RefreshTokenCookieName = "refreshToken";
        private readonly ILogger<CookieService> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="CookieService"/>
        /// </summary>
        public CookieService(IHttpContextAccessor httpContextAccessor, ILogger<CookieService> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void SetRefreshTokenCookie(string token, DateTime? expires = null)
        {
            try
            {
                _logger.LogDebug("Estableciendo cookie de refresh token");
                
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = expires ?? DateTime.UtcNow.AddDays(7),
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                };
                
                _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                    RefreshTokenCookieName, 
                    token, 
                    cookieOptions);
                
                _logger.LogDebug("Cookie de refresh token establecida correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer la cookie de refresh token");
                throw;
            }
        }

        /// <inheritdoc/>
        public string? GetRefreshTokenFromCookie()
        {
            try
            {
                _logger.LogDebug("Obteniendo refresh token de la cookie");
                return _httpContextAccessor.HttpContext?.Request.Cookies[RefreshTokenCookieName];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el refresh token de la cookie");
                return null;
            }
        }

        /// <inheritdoc/>
        public void DeleteRefreshTokenCookie()
        {
            try
            {
                _logger.LogDebug("Eliminando cookie de refresh token");
                
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete(
                    RefreshTokenCookieName,
                    new CookieOptions 
                    { 
                        Path = "/",
                        Secure = true,
                        HttpOnly = true,
                        SameSite = SameSiteMode.None
                    });
                
                _logger.LogDebug("Cookie de refresh token eliminada correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la cookie de refresh token");
                throw;
            }
        }
    }
}
