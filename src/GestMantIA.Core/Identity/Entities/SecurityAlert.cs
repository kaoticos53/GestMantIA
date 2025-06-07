using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestMantIA.Core.Identity.Interfaces;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Representa una alerta de seguridad que requiere atención del equipo de seguridad.
    /// </summary>
    [Table("SecurityAlerts")]
    public class SecurityAlert : BaseEntity
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="SecurityAlert"/>
        /// </summary>
        public SecurityAlert()
        {
            Title = string.Empty;
            Message = string.Empty;
            ResolutionNotes = null;
        }

        /// <summary>
        /// Título de la alerta.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Mensaje detallado de la alerta.
        /// </summary>
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// Nivel de gravedad de la alerta.
        /// </summary>
        [Required]
        public SecurityAlertSeverity Severity { get; set; }

        /// <summary>
        /// ID del evento de seguridad relacionado (opcional).
        /// </summary>
        public Guid? RelatedEventId { get; set; }

        /// <summary>
        /// Indica si la alerta ha sido resuelta.
        /// </summary>
        public bool IsResolved { get; set; }
        /// <summary>
        /// Fecha y hora en que se resolvió la alerta (si aplica).
        /// </summary>
        public DateTimeOffset? ResolvedAt { get; set; }

        /// <summary>
        /// Notas adicionales sobre la resolución de la alerta.
        /// </summary>
        public string? ResolutionNotes { get; set; }

        /// <summary>
        /// ID del usuario que resolvió la alerta (si aplica).
        /// </summary>
        public Guid? ResolvedById { get; set; }

        /// <summary>
        /// Usuario que resolvió la alerta (si aplica).
        /// </summary>
        [ForeignKey(nameof(ResolvedById))]
        public virtual ApplicationUser? ResolvedBy { get; set; }

        /// <summary>
        /// Marca la alerta como resuelta.
        /// </summary>
        /// <param name="resolvedById">ID del usuario que resuelve la alerta.</param>
        /// <param name="notes">Notas adicionales sobre la resolución.</param>
        public void MarkAsResolved(Guid resolvedById, string? notes = null)
        {
            if (!IsResolved)
            {
                IsResolved = true;
                ResolvedAt = DateTimeOffset.UtcNow;
                ResolvedById = resolvedById;
                ResolutionNotes = notes;
            }
        }

        /// <summary>
        /// Vuelve a abrir una alerta resuelta.
        /// </summary>
        public void Reopen()
        {
            if (IsResolved)
            {
                IsResolved = false;
                ResolvedAt = null;
                ResolvedById = null;
                ResolutionNotes = string.Empty; // Cambiado de null a string.Empty
            }
        }
    }
}
