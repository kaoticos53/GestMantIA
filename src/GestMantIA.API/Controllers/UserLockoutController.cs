using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Shared.Identity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestMantIA.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar el bloqueo y desbloqueo de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")] // Solo administradores pueden bloquear/desbloquear usuarios
    public class UserLockoutController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserLockoutController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserLockoutController"/>
        /// </summary>
        public UserLockoutController(
            IUserService userService,
            ILogger<UserLockoutController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Bloquea a un usuario por un período de tiempo específico.
        /// </summary>
        /// <param name="userId">ID del usuario a bloquear.</param>
        /// <param name="durationMinutes">Duración del bloqueo en minutos (opcional, si no se especifica, el bloqueo es indefinido).</param>
        /// <param name="reason">Razón del bloqueo (opcional).</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost("{userId:guid}/lock")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> LockUser(
            [FromRoute] Guid userId,
            [FromQuery] int? durationMinutes = null,
            [FromQuery] string? reason = null)
        {
            try
            {
                TimeSpan? duration = durationMinutes.HasValue
                    ? TimeSpan.FromMinutes(durationMinutes.Value)
                    : (TimeSpan?)null;

                var result = await _userService.LockUserAsync(userId, duration, reason);
                if (!result)
                {
                    return NotFound(new { Message = $"No se pudo bloquear al usuario con ID: {userId}" });
                }

                var message = duration.HasValue
                    ? $"Usuario bloqueado por {duration.Value.TotalMinutes} minutos."
                    : "Usuario bloqueado indefinidamente.";

                if (!string.IsNullOrEmpty(reason))
                {
                    message += $" Razón: {reason}";
                }


                return Ok(new { Message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al bloquear al usuario {UserId}", userId);
                return StatusCode(500, new { Message = "Ocurrió un error al procesar la solicitud." });
            }
        }


        /// <summary>
        /// Desbloquea a un usuario previamente bloqueado.
        /// </summary>
        /// <param name="userId">ID del usuario a desbloquear.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost("{userId:guid}/unlock")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UnlockUser([FromRoute] Guid userId)
        {
            try
            {
                var result = await _userService.UnlockUserAsync(userId);
                if (!result)
                {
                    return NotFound(new { Message = $"No se pudo desbloquear al usuario con ID: {userId} o el usuario no existe." });
                }

                return Ok(new { Message = "Usuario desbloqueado exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desbloquear al usuario {UserId}", userId);
                return StatusCode(500, new { Message = "Ocurrió un error al procesar la solicitud." });
            }
        }

        /// <summary>
        /// Obtiene la información de bloqueo de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Información de bloqueo del usuario.</returns>
        [HttpGet("{userId:guid}/lockout-info")]
        [ProducesResponseType(typeof(UserLockoutInfo), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserLockoutInfo([FromRoute] Guid userId)
        {
            try
            {
                var lockoutInfo = await _userService.GetUserLockoutInfoAsync(userId);
                if (lockoutInfo == null)
                {
                    return NotFound(new { Message = $"No se encontró información de bloqueo para el usuario con ID: {userId}" });
                }

                return Ok(lockoutInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la información de bloqueo del usuario {UserId}", userId);
                return StatusCode(500, new { Message = "Ocurrió un error al procesar la solicitud." });
            }
        }
    }
}
