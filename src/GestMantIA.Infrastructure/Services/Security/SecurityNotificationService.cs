using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Interfaces;
using GestMantIA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GestMantIA.Infrastructure.Services.Security
{
    /// <summary>
    /// Implementación del servicio de notificaciones de seguridad.
    /// </summary>
    public class SecurityNotificationService : ISecurityNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<SecurityNotificationService> _logger;
        private readonly SecurityNotificationOptions _options;

        public SecurityNotificationService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<SecurityNotificationService> logger,
            IOptions<SecurityNotificationOptions> options)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<bool> SendSecurityNotificationAsync(
            Guid userId,
            string title,
            string message,
            SecurityNotificationType notificationType,
            Guid? relatedEventId = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentNullException(nameof(userId));

            try
            {
                var notification = new SecurityNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    // No asignamos User aquí ya que no tenemos acceso directo al UserManager
                    // La relación se establecerá automáticamente cuando se guarde en la base de datos
                    Title = title ?? "Notificación de seguridad",
                    Message = message ?? "Sin detalles adicionales",
                    NotificationType = notificationType,
                    RelatedEventId = relatedEventId,
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow.UtcDateTime
                };

                _context.SecurityNotifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Notificación de seguridad enviada al usuario {UserId}: {Title}",
                    userId.ToString(), title);

                // Enviar notificación por correo si está habilitado
                if (_options.EnableEmailNotifications)
                {
                    var user = await _context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        var emailSubject = $"[{_options.ApplicationName}] {title}";
                        var emailContent = $"""
                        <h2>{title}</h2>
                        <p>{message}</p>
                        <p><small>Fecha: {DateTime.UtcNow:g} (UTC)</small></p>
                        """;

                        await _emailService.SendEmailAsync(
                            user.Email,
                            emailSubject,
                            emailContent);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al enviar notificación de seguridad al usuario {UserId}",
                    userId.ToString());
                return false;
            }
        }

        public async Task<bool> SendSecurityAlertAsync(
            string title,
            string message,
            SecurityAlertSeverity severity,
            Guid? relatedEventId = null)
        {
            try
            {
                // Guardar la alerta en la base de datos
                var alert = new SecurityAlert
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Message = message,
                    Severity = severity,
                    RelatedEventId = relatedEventId,
                    IsResolved = false,
                    CreatedAt = DateTimeOffset.UtcNow.UtcDateTime
                };

                _context.SecurityAlerts.Add(alert);
                await _context.SaveChangesAsync();

                _logger.LogWarning(
                    "Alerta de seguridad registrada: {Title} - Severidad: {Severity}",
                    title, severity);

                // Notificar al equipo de seguridad si la severidad es alta o crítica
                if (severity >= SecurityAlertSeverity.High &&
                    !string.IsNullOrEmpty(_options.SecurityTeamEmail))
                {
                    var emailSubject = $"[ALERTA DE SEGURIDAD - {severity}] {title}";
                    var emailContent = $"""
                    <h2>🚨 {title}</h2>
                    <p><strong>Gravedad:</strong> {severity}</p>
                    <p>{message}</p>
                    <p><small>ID del evento: {relatedEventId?.ToString() ?? "N/A"}</small></p>
                    <p><small>Fecha: {DateTime.UtcNow:g} (UTC)</small></p>
                    """;

                    await _emailService.SendEmailAsync(
                        _options.SecurityTeamEmail,
                        emailSubject,
                        emailContent);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al registrar alerta de seguridad: {Title}",
                    title);
                return false;
            }
        }

        public async Task<bool> SendSecurityEmailAsync(
            string email,
            string subject,
            string htmlContent)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            try
            {
                var fullSubject = $"[{_options.ApplicationName}] {subject}";

                // Añadir el pie de página estándar
                var fullHtmlContent = $"""
                <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                    {htmlContent}
                    <hr style="border: 0; border-top: 1px solid #eaeaea; margin: 20px 0;" />
                    <p style="color: #666; font-size: 12px; line-height: 1.5;">
                        Este es un correo electrónico de seguridad de {_options.ApplicationName}. 
                        Si no reconoces esta actividad, por favor contacta a nuestro equipo de soporte 
                        inmediatamente en <a href="mailto:{_options.SupportEmail}">{_options.SupportEmail}</a>.
                    </p>
                </div>
                """;

                var result = await _emailService.SendEmailAsync(email, fullSubject, fullHtmlContent);

                _logger.LogInformation(
                    "Correo de seguridad enviado a {Email}: {Subject}",
                    email, subject);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al enviar correo de seguridad a {Email}",
                    email);
                return false;
            }
        }
    }

    /// <summary>
    /// Opciones de configuración para el servicio de notificaciones de seguridad.
    /// </summary>
    public class SecurityNotificationOptions
    {
        /// <summary>
        /// Nombre de la aplicación.
        /// </summary>
        public required string ApplicationName { get; set; } = "GestMantIA";

        /// <summary>
        /// Dirección de correo electrónico del equipo de seguridad.
        /// </summary>
        public required string SecurityTeamEmail { get; set; } = string.Empty;

        /// <summary>
        /// Dirección de correo electrónico de soporte.
        /// </summary>
        public required string SupportEmail { get; set; } = "soporte@gestmantia.com";

        /// <summary>
        /// Habilita o deshabilita las notificaciones por correo electrónico.
        /// </summary>
        public bool EnableEmailNotifications { get; set; } = true;
    }
}
