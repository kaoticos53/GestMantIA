using System;
using System.Threading.Tasks;
using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Core.Identity.Interfaces
{
    /// <summary>
    /// Servicio para enviar notificaciones relacionadas con la seguridad.
    /// </summary>
    public interface ISecurityNotificationService
    {
        /// <summary>
        /// Envía una notificación de seguridad al usuario.
        /// </summary>
        /// <param name="userId">ID del usuario destinatario.</param>
        /// <param name="title">Título de la notificación.</param>
        /// <param name="message">Mensaje detallado.</param>
        /// <param name="notificationType">Tipo de notificación.</param>
        /// <param name="relatedEventId">ID del evento relacionado (opcional).</param>
        /// <returns>True si la notificación se envió correctamente.</returns>
        Task<bool> SendSecurityNotificationAsync(
            string userId,
            string title,
            string message,
            SecurityNotificationType notificationType,
            Guid? relatedEventId = null);

        /// <summary>
        /// Envía una alerta de seguridad al equipo de seguridad.
        /// </summary>
        /// <param name="title">Título de la alerta.</param>
        /// <param name="message">Mensaje detallado.</param>
        /// <param name="severity">Nivel de gravedad.</param>
        /// <param name="relatedEventId">ID del evento relacionado (opcional).</param>
        /// <returns>True si la alerta se envió correctamente.</returns>
        Task<bool> SendSecurityAlertAsync(
            string title,
            string message,
            SecurityAlertSeverity severity,
            Guid? relatedEventId = null);

        /// <summary>
        /// Envía un correo electrónico de notificación de seguridad.
        /// </summary>
        /// <param name="email">Dirección de correo electrónico del destinatario.</param>
        /// <param name="subject">Asunto del correo.</param>
        /// <param name="htmlContent">Contenido HTML del correo.</param>
        /// <returns>True si el correo se envió correctamente.</returns>
        Task<bool> SendSecurityEmailAsync(
            string email,
            string subject,
            string htmlContent);
    }

    /// <summary>
    /// Tipos de notificaciones de seguridad.
    /// </summary>
    public enum SecurityNotificationType
    {
        /// <summary>Notificación informativa.</summary>
        Information,
        
        /// <summary>Alerta de seguridad.</summary>
        SecurityAlert,
        
        /// <summary>Actividad sospechosa detectada.</summary>
        SuspiciousActivity,
        
        /// <summary>Nuevo inicio de sesión.</summary>
        NewLogin,
        
        /// <summary>Cambio en la configuración de seguridad.</summary>
        SecuritySettingsChanged,
        
        /// <summary>Dispositivo no reconocido.</summary>
        UnrecognizedDevice
    }

    /// <summary>
    /// Niveles de gravedad para las alertas de seguridad.
    /// </summary>
    public enum SecurityAlertSeverity
    {
        /// <summary>Baja prioridad.</summary>
        Low,
        
        /// <summary>Prioridad media.</summary>
        Medium,
        
        /// <summary>Alta prioridad.</summary>
        High,
        
        /// <summary>Crítico - requiere atención inmediata.</summary>
        Critical
    }
}
