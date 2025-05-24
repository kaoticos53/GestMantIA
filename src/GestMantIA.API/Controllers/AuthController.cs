using System;
using System.Threading.Tasks;
using GestMantIA.Core.Identity.DTOs;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Identity.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AuthController"/>
        /// </summary>
        public AuthController(
            IAuthenticationService authService,
            ILogger<AuthController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Inicia sesión de un usuario.
        /// </summary>
        /// <param name="request">Datos de inicio de sesión.</param>
        /// <returns>Token de autenticación y datos del usuario.</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthenticationResult>> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de inicio de sesión con modelo inválido");
                return BadRequest(ModelState);
            }

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.AuthenticateAsync(request, ipAddress);

                if (!result.Success)
                {
                    _logger.LogWarning("Error en el inicio de sesión: {Message}", result.Message);
                    return Unauthorized(result);
                }

                SetTokenCookie(result.RefreshToken);
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

                if (!result.Success)
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
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Intento de renovación de token sin token de actualización");
                    return BadRequest("Token de actualización no proporcionado");
                }

                var ipAddress = GetIpAddress();
                var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

                if (!result.Success)
                {
                    _logger.LogWarning("Error al renovar el token: {Message}", result.Message);
                    return Unauthorized(result);
                }

                SetTokenCookie(result.RefreshToken);
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
                var refreshToken = Request.Cookies["refreshToken"];
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

                Response.Cookies.Delete("refreshToken");
                return Ok(new { message = "Token revocado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar el token");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al revocar el token");
            }
        }

        /// <summary>
        /// Verifica la dirección de correo electrónico de un usuario.
        /// </summary>
        /// <param name="token">Token de verificación.</param>
        [HttpGet("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token de verificación no proporcionado");
            }

            try
            {
                var result = await _authService.VerifyEmailAsync(token);

                if (!result.Success)
                {
                    _logger.LogWarning("Error al verificar el correo: {Message}", result.Message);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el correo electrónico");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al verificar el correo electrónico");
            }
        }

        /// <summary>
        /// Solicita el restablecimiento de contraseña para un correo electrónico.
        /// </summary>
        /// <param name="request">Correo electrónico para restablecer la contraseña.</param>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Solicitud de restablecimiento de contraseña con modelo inválido");
                return BadRequest(ModelState);
            }

            try
            {
                var origin = GetOrigin();
                var result = await _authService.ForgotPasswordAsync(request.Email, origin);

                // Por seguridad, no revelamos si el correo existe o no
                if (!result.Success)
                {
                    _logger.LogWarning("Error al procesar la solicitud de restablecimiento de contraseña para {Email}", request.Email);
                }

                // Siempre devolvemos OK para no revelar si el correo existe o no
                return Ok(new { message = "Si el correo electrónico existe en nuestro sistema, se ha enviado un enlace para restablecer la contraseña." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud de restablecimiento de contraseña");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud de restablecimiento de contraseña");
            }
        }

        /// <summary>
        /// Restablece la contraseña de un usuario.
        /// </summary>
        /// <param name="request">Datos para restablecer la contraseña.</param>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de restablecimiento de contraseña con modelo inválido");
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.ResetPasswordAsync(request);

                if (!result.Success)
                {
                    _logger.LogWarning("Error al restablecer la contraseña: {Message}", result.Message);
                    return BadRequest(result);
                }

                return Ok(new { message = "La contraseña se ha restablecido correctamente. Ahora puede iniciar sesión con su nueva contraseña." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer la contraseña");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al restablecer la contraseña");
            }
        }

        #region Métodos auxiliares

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.None,
                Secure = true
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string GetIpAddress()
        {
            if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
                return _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"];
            
            return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
        }

        private string GetOrigin()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}";
        }

        #endregion
    }
}
