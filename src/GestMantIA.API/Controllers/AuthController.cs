using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Identity.Results;
using GestMantIA.Shared.Identity.DTOs;
using GestMantIA.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestMantIA.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar la autenticación de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly ICookieService _cookieService;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AuthController"/>
        /// </summary>
        public AuthController(
            IAuthenticationService authService,
            ILogger<AuthController> logger,
            ICookieService cookieService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
        }

        /// <summary>
        /// Inicia sesión de un usuario.
        /// </summary>
        /// <param name="request">Datos de inicio de sesión.</param>
        /// <returns>Token de autenticación y datos del usuario.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de inicio de sesión con modelo inválido");
                return BadRequest(new { message = "Datos de inicio de sesión inválidos", errors = ModelState });
            }

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.AuthenticateAsync(request, ipAddress);

                if (result.Succeeded == false)
                {
                    _logger.LogWarning("Error en el inicio de sesión: {Message}", result.Message);
                    return Unauthorized(result);
                }

                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    _cookieService.SetRefreshTokenCookie(result.RefreshToken);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud de inicio de sesión");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud");
            }
        }

        /// <summary>
        /// Registra un nuevo usuario.
        /// </summary>
        /// <param name="request">Datos de registro.</param>
        /// <returns>Resultado del registro.</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterResult>> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de registro con modelo inválido");
                return BadRequest(ModelState);
            }

            try
            {
                var origin = GetOrigin();
                var result = await _authService.RegisterAsync(request, origin);

                if (result.Succeeded == false)
                {
                    _logger.LogWarning("Error en el registro: {Message}", result.Message);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud de registro");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud");
            }
        }

        /// <summary>
        /// Renueva el token de acceso utilizando un token de actualización.
        /// </summary>
        /// <returns>Nuevo token de acceso y de actualización.</returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthenticationResult>> RefreshToken()
        {
            try
            {
                var refreshToken = _cookieService.GetRefreshTokenFromCookie();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Intento de renovación de token sin token de actualización");
                    return BadRequest("Token de actualización no proporcionado");
                }

                var ipAddress = GetIpAddress();
                var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

                if (result.Succeeded == false)
                {
                    _logger.LogWarning("Error al renovar el token: {Message}", result.Message);
                    return Unauthorized(result);
                }

                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    _cookieService.SetRefreshTokenCookie(result.RefreshToken);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renovar el token");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al renovar el token");
            }
        }

        /// <summary>
        /// Revoca el token de actualización actual del usuario.
        /// </summary>
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeToken()
        {
            try
            {
                var refreshToken = _cookieService.GetRefreshTokenFromCookie();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Intento de revocación de token sin token de actualización");
                    return BadRequest("Token de actualización no proporcionado");
                }

                var ipAddress = GetIpAddress();
                var result = await _authService.RevokeTokenAsync(refreshToken, ipAddress);

                if (!result)
                {
                    _logger.LogWarning("No se pudo revocar el token");
                    return BadRequest("No se pudo revocar el token");
                }

                _cookieService.DeleteRefreshTokenCookie();
                return Ok(new { message = "Token revocado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar el token");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al revocar el token");
            }
        }

        // Resto de los métodos del controlador...

        #region Métodos auxiliares

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"]!;
            }
            
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "IP-desconocida";
        }

        private string? GetOrigin()
        {
            return Request.Headers["Origin"].FirstOrDefault() ?? "https://localhost";
        }

        #endregion
    }
}
