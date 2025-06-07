using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Registra eventos de seguridad relevantes en el sistema.
    /// </summary>
    [Table("SecurityLogs")]
    public class SecurityLog : BaseEntity
    {
        /// <summary>
        /// ID del usuario relacionado al evento (opcional, puede ser nulo para eventos del sistema).
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Usuario relacionado al evento.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        /// <summary>
        /// Tipo de evento de seguridad.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public required string EventType { get; set; }

        /// <summary>
        /// Descripción detallada del evento.
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public required string Description { get; set; }

        /// <summary>
        /// Dirección IP desde donde se originó el evento.
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Agente de usuario (navegador, dispositivo) que originó el evento.
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Datos adicionales en formato JSON.
        /// </summary>
        public string? AdditionalData { get; set; }

        /// <summary>
        /// Indica si el evento fue exitoso o no.
        /// </summary>
        public bool Succeeded { get; set; } = true;

    }

    /// <summary>
    /// Tipos de eventos de seguridad comunes.
    /// </summary>
    public static class SecurityEventTypes
    {
        // Autenticación
        public const string LoginSucceeded = "LoginSucceeded";
        public const string LoginFailed = "LoginFailed";
        public const string Logout = "Logout";
        public const string LockedOut = "LockedOut";

        // Contraseñas
        public const string PasswordChanged = "PasswordChanged";
        public const string PasswordResetRequested = "PasswordResetRequested";
        public const string PasswordReset = "PasswordReset";

        // 2FA
        public const string TwoFactorEnabled = "TwoFactorEnabled";
        public const string TwoFactorDisabled = "TwoFactorDisabled";
        public const string TwoFactorLoginSucceeded = "TwoFactorLoginSucceeded";
        public const string TwoFactorLoginFailed = "TwoFactorLoginFailed";

        // Cuentas
        public const string AccountLocked = "AccountLocked";
        public const string AccountUnlocked = "AccountUnlocked";
        public const string EmailChanged = "EmailChanged";
        public const string PhoneNumberChanged = "PhoneNumberChanged";

        // Seguridad
        public const string SuspiciousActivity = "SuspiciousActivity";
        public const string NewDeviceLogin = "NewDeviceLogin";
        public const string SecurityAlert = "SecurityAlert";
    }
}
