using System;

namespace GestMantIA.Core.Identity.DTOs
{
    /// <summary>
    /// Información sobre el bloqueo de un usuario.
    /// </summary>
    public class UserLockoutInfo
    {
        /// <summary>
        /// ID del usuario.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Indica si el usuario está actualmente bloqueado.
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Fecha y hora en que finaliza el bloqueo (si es temporal).
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// Razón del bloqueo (si se especificó).
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Fecha y hora en que se inició el bloqueo.
        /// </summary>
        public DateTime? LockoutStart { get; set; }

        /// <summary>
        /// Indica si el bloqueo es permanente.
        /// </summary>
        public bool IsPermanent { get; set; }

        /// <summary>
        /// Tiempo restante de bloqueo (si es temporal).
        /// </summary>
        public TimeSpan? TimeRemaining => IsLockedOut && LockoutEnd.HasValue 
            ? LockoutEnd.Value - DateTime.UtcNow 
            : null;
    }
}
