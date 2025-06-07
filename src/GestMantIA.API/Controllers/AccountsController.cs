using GestMantIA.Application.Interfaces; // Para IAccountService
using GestMantIA.Shared.Identity.DTOs; // Para ForgotPasswordRequest, ResetPasswordRequest
using Microsoft.AspNetCore.Authorization; // Para AllowAnonymous
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Para ILogger
using Microsoft.AspNetCore.Http; // Para StatusCodes

namespace GestMantIA.API.Controllers // Namespace ajustado
{
    [ApiController]
    [Route("api/[controller]")] // Ruta ajustada
    [AllowAnonymous] // Estas operaciones deben ser accesibles sin autenticación
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// Inicia el proceso de restablecimiento de contraseña.
        /// </summary>
        /// <param name="request">El DTO con el correo electrónico del usuario.</param>
        /// <returns>Un resultado indicando el éxito de la solicitud.</returns>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var origin = $"{Request.Scheme}://{Request.Host}";
            
            _logger.LogInformation("Solicitud de olvido de contraseña recibida para: {Email} con origen {Origin}", request.Email, origin);

            var result = await _accountService.ForgotPasswordAsync(request, origin);

            if (result.Succeeded)
            {
                _logger.LogInformation("Proceso de olvido de contraseña iniciado para: {Email}", request.Email);
                return Ok(new { Message = "Si su correo electrónico está registrado, recibirá un enlace para restablecer su contraseña." });
            }

            _logger.LogError("Error en el proceso de olvido de contraseña para: {Email}. Errores: {Errors}", request.Email, string.Join(", ", result.Errors));
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Ocurrió un error al procesar su solicitud.", Errors = result.Errors });
        }

        /// <summary>
        /// Restablece la contraseña del usuario.
        /// </summary>
        /// <param name="request">El DTO con el token, correo y nueva contraseña.</param>
        /// <returns>Un resultado indicando el éxito del restablecimiento.</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            _logger.LogInformation("Solicitud de restablecimiento de contraseña recibida para: {Email}", request.Email);

            var result = await _accountService.ResetPasswordAsync(request);

            if (result.Succeeded)
            {
                _logger.LogInformation("Contraseña restablecida exitosamente para: {Email}", request.Email);
                return Ok(new { Message = "Su contraseña ha sido restablecida exitosamente." });
            }
            
            _logger.LogWarning("Fallo al restablecer contraseña para: {Email}. Errores: {Errors}", request.Email, string.Join(", ", result.Errors));
            return BadRequest(new { Message = "No se pudo restablecer la contraseña. Verifique los datos o el enlace puede haber expirado.", Errors = result.Errors });
        }
    }
}
