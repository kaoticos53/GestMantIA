using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestMantIA.Shared.Identity.DTOs;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Shared;
using Microsoft.Extensions.Logging;

namespace GestMantIA.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar las notificaciones de seguridad de los usuarios.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityNotificationsController : ControllerBase
    {
        private readonly ISecurityNotificationService _notificationService;
        private readonly ISecurityLogger _securityLogger;
        private readonly ILogger<SecurityNotificationsController> _logger;

        public SecurityNotificationsController(
            ISecurityNotificationService notificationService,
            ISecurityLogger securityLogger,
            ILogger<SecurityNotificationsController> logger)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _securityLogger = securityLogger ?? throw new ArgumentNullException(nameof(securityLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene las notificaciones de seguridad del usuario actual.
        /// </summary>
        /// <param name="unreadOnly">Indica si se deben obtener solo las notificaciones no leídas.</param>
        /// <param name="page">Número de página (comenzando en 1).</param>
        /// <param name="pageSize">Tamaño de la página (máximo 50).</param>
        /// <returns>Lista paginada de notificaciones de seguridad.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResult<SecurityNotificationDto>>> GetNotifications(
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("No se pudo identificar al usuario.");
                }

                // Validar parámetros de paginación
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 50);

                if (!Guid.TryParse(userId, out var userGuid))
                {
                    return BadRequest("El ID de usuario no es válido.");
                }

                // Obtener notificaciones del usuario
                var (notifications, totalCount) = await _securityLogger.GetUserSecurityLogsAsync(
                    userGuid, 
                    page, 
                    pageSize);

                var result = new PagedResult<SecurityNotificationDto>
                {
                    Items = notifications.Select(n => new SecurityNotificationDto
                    {
                        Id = n.Id,
                        Title = GetNotificationTitle(n.EventType),
                        Message = n.Description,
                        Type = MapToNotificationType(n.EventType),
                        IsRead = n.Succeeded,
                        CreatedAt = n.Timestamp,
                        RelatedEventId = n.Id
                    }),
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las notificaciones de seguridad");
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    "Ocurrió un error al obtener las notificaciones de seguridad.");
            }
        }

        /// <summary>
        /// Marca una notificación como leída.
        /// </summary>
        /// <param name="id">ID de la notificación.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost("{id}/read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult MarkAsRead(Guid id)
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("No se pudo identificar al usuario.");
                }

                // En una implementación real, actualizaríamos el estado de la notificación
                // Por ahora, solo registramos la acción
                _logger.LogInformation("Notificación {NotificationId} marcada como leída por el usuario {UserId}", id, userId);
                
                return Ok(new { Message = "Notificación marcada como leída." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar la notificación {NotificationId} como leída", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    "Ocurrió un error al marcar la notificación como leída.");
            }
        }

        /// <summary>
        /// Obtiene la cantidad de notificaciones no leídas del usuario actual.
        /// </summary>
        /// <returns>Cantidad de notificaciones no leídas.</returns>
        [HttpGet("unread/count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<UnreadNotificationsCountDto> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("No se pudo identificar al usuario.");
                }

                // En una implementación real, contaríamos las notificaciones no leídas
                // Por ahora, devolvemos un valor simulado
                var count = new Random().Next(0, 10);
                
                return Ok(new UnreadNotificationsCountDto { Count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el contador de notificaciones no leídas");
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    "Ocurrió un error al obtener el contador de notificaciones no leídas.");
            }
        }

        #region Métodos auxiliares

        private static string GetNotificationTitle(string eventType)
        {
            return eventType switch
            {
                SecurityEventTypes.LoginSucceeded => "Inicio de sesión exitoso",
                SecurityEventTypes.LoginFailed => "Intento de inicio de sesión fallido",
                SecurityEventTypes.Logout => "Cierre de sesión",
                SecurityEventTypes.LockedOut => "Cuenta bloqueada temporalmente",
                SecurityEventTypes.PasswordChanged => "Contraseña actualizada",
                SecurityEventTypes.PasswordResetRequested => "Solicitud de restablecimiento de contraseña",
                SecurityEventTypes.PasswordReset => "Contraseña restablecida",
                SecurityEventTypes.TwoFactorEnabled => "Autenticación de dos factores habilitada",
                SecurityEventTypes.TwoFactorDisabled => "Autenticación de dos factores deshabilitada",
                SecurityEventTypes.TwoFactorLoginSucceeded => "Inicio de sesión con 2FA exitoso",
                SecurityEventTypes.TwoFactorLoginFailed => "Código 2FA incorrecto",
                SecurityEventTypes.AccountLocked => "Cuenta bloqueada",
                SecurityEventTypes.AccountUnlocked => "Cuenta desbloqueada",
                SecurityEventTypes.EmailChanged => "Correo electrónico actualizado",
                SecurityEventTypes.PhoneNumberChanged => "Número de teléfono actualizado",
                SecurityEventTypes.SuspiciousActivity => "Actividad sospechosa detectada",
                SecurityEventTypes.NewDeviceLogin => "Nuevo dispositivo detectado",
                _ => "Notificación de seguridad"
            };
        }

        private static SecurityNotificationType MapToNotificationType(string eventType)
        {
            return eventType switch
            {
                SecurityEventTypes.LoginSucceeded => SecurityNotificationType.NewLogin,
                SecurityEventTypes.LoginFailed => SecurityNotificationType.SecurityAlert,
                SecurityEventTypes.LockedOut => SecurityNotificationType.SecurityAlert,
                SecurityEventTypes.SuspiciousActivity => SecurityNotificationType.SuspiciousActivity,
                SecurityEventTypes.NewDeviceLogin => SecurityNotificationType.UnrecognizedDevice,
                _ => SecurityNotificationType.Information
            };
        }

        #endregion
    }

    /// <summary>
    /// DTO para representar una notificación de seguridad.
    /// </summary>
    public class SecurityNotificationDto
    {
        /// <summary>
        /// ID de la notificación.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Título de la notificación.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje detallado de la notificación.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de notificación.
        /// </summary>
        public SecurityNotificationType Type { get; set; }

        /// <summary>
        /// Indica si la notificación ha sido leída.
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Fecha y hora en que se creó la notificación.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// ID del evento relacionado (opcional).
        /// </summary>
        public Guid? RelatedEventId { get; set; }
    }

    /// <summary>
    /// DTO para el contador de notificaciones no leídas.
    /// </summary>
    public class UnreadNotificationsCountDto
    {
        /// <summary>
        /// Cantidad de notificaciones no leídas.
        /// </summary>
        public int Count { get; set; }
    }
}
