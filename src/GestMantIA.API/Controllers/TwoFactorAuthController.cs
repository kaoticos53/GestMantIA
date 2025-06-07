using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Identity.Results;
using GestMantIA.Shared.Identity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestMantIA.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar la autenticación de dos factores.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TwoFactorAuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<TwoFactorAuthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TwoFactorAuthController"/>
        /// </summary>
        public TwoFactorAuthController(
            IAuthenticationService authService,
            ILogger<TwoFactorAuthController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Genera la configuración inicial para la autenticación de dos factores.
        /// </summary>
        /// <returns>Datos para configurar la autenticación de dos factores.</returns>
        [HttpGet("setup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TwoFactorSetupResult>> GetTwoFactorSetup()
        {
            try
            {
                var uId = User.FindFirst("uid")?.Value;
                var userEmail = User.FindFirst("email")?.Value;

                if (string.IsNullOrEmpty(uId) || string.IsNullOrEmpty(userEmail) || !Guid.TryParse(uId, out _))
                {
                    return Unauthorized("No se pudo identificar al usuario.");
                }

                var result = await _authService.GenerateTwoFactorSetupAsync(Guid.Parse(uId), userEmail);

                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar la configuración de autenticación de dos factores");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al generar la configuración de autenticación de dos factores.");
            }
        }

        /// <summary>
        /// Habilita la autenticación de dos factores para el usuario actual.
        /// </summary>
        /// <param name="request">Solicitud con el código de verificación.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost("enable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<OperationResult>> EnableTwoFactor([FromBody] VerifyTwoFactorRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var uId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(uId) || !Guid.TryParse(uId, out _))
                {
                    return Unauthorized("No se pudo identificar al usuario.");
                }

                var result = await _authService.EnableTwoFactorAsync(Guid.Parse(uId), request.Code);

                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al habilitar la autenticación de dos factores");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al habilitar la autenticación de dos factores.");
            }
        }

        /// <summary>
        /// Deshabilita la autenticación de dos factores para el usuario actual.
        /// </summary>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost("disable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<OperationResult>> DisableTwoFactor()
        {
            try
            {
                var uId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(uId) || !Guid.TryParse(uId, out _))
                {
                    return Unauthorized("No se pudo identificar al usuario.");
                }

                var result = await _authService.DisableTwoFactorAsync(Guid.Parse(uId));

                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deshabilitar la autenticación de dos factores");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al deshabilitar la autenticación de dos factores.");
            }
        }

        /// <summary>
        /// Verifica un código de autenticación de dos factores.
        /// </summary>
        /// <param name="request">Solicitud con el código de verificación.</param>
        /// <returns>Resultado de la verificación.</returns>
        [HttpPost("verify")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VerifyTwoFactorResult>> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var uId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(uId) || !Guid.TryParse(uId, out _))
                {
                    return Unauthorized("No se pudo identificar al usuario.");
                }


                var isValid = await _authService.VerifyTwoFactorTokenAsync(Guid.Parse(uId), request.Code);

                if (!isValid)
                {
                    return BadRequest(new VerifyTwoFactorResult { IsValid = false, Message = "Código de verificación inválido o expirado." });
                }

                return Ok(new VerifyTwoFactorResult { IsValid = true, Message = "Código de verificación válido." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el código de autenticación de dos factores");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al verificar el código de autenticación de dos factores.");
            }
        }
    }
}
