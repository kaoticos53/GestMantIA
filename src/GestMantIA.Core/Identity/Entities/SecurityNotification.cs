using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestMantIA.Core.Identity.Interfaces;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Representa una notificación de seguridad enviada a un usuario.
    /// </summary>
    [Table("SecurityNotifications")]
    public class SecurityNotification : BaseEntity
    {
        /// <summary>
        /// ID del usuario destinatario de la notificación.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Usuario destinatario de la notificación.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// Título de la notificación.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Mensaje detallado de la notificación.
        /// </summary>
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// Tipo de notificación.
        /// </summary>
        [Required]
        public SecurityNotificationType NotificationType { get; set; }

        /// <summary>
        /// ID del evento de seguridad relacionado (opcional).
        /// </summary>
        public Guid? RelatedEventId { get; set; }

        /// <summary>
        /// Indica si la notificación ha sido leída por el usuario.
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Fecha y hora en que el usuario leyó la notificación (si aplica).
        /// </summary>
        public DateTimeOffset? ReadAt { get; set; }

        /// <summary>
        /// Marca la notificación como leída.
        /// </summary>
        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadAt = DateTimeOffset.UtcNow;
            }
        }
    }
}
