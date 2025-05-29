using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase que representa un token de actualización para la autenticación JWT.
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="RefreshToken"/>
        /// </summary>
        public RefreshToken()
        {
            Token = string.Empty;
            CreatedByIp = string.Empty;
            RevokedByIp = string.Empty;
            ReplacedByToken = string.Empty;
            UserId = Guid.Empty; // Usar Guid.Empty para inicializar el UserId
            User = null!; // Inicialización forzada, se debe establecer después
        }

        /// <summary>
        /// Token de actualización.
        /// </summary>
        [Required]
        public string Token { get; set; }

        /// <summary>
        /// Fecha de expiración del token.
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// Indica si el token ha sido revocado.
        /// </summary>
        public bool IsRevoked { get; set; }


        /// <summary>
        /// Dirección IP desde la que se creó el token.
        /// </summary>
        public string CreatedByIp { get; set; }

        /// <summary>
        /// Fecha de revocación del token, si ha sido revocado.
        /// </summary>
        public DateTime? Revoked { get; set; }


        /// <summary>
        /// Dirección IP desde la que se revocó el token.
        /// </summary>
        public string RevokedByIp { get; set; }

        /// <summary>
        /// Razón por la que el token fue revocado.
        /// </summary>
        public string? ReasonRevoked { get; set; }

        /// <summary>
        /// Token que reemplazó a este token, si fue revocado.
        /// </summary>
        public string ReplacedByToken { get; set; }

        /// <summary>
        /// Identificador del usuario propietario del token.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Usuario propietario del token.
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// Indica si el token ha expirado.
        /// </summary>
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= Expires;

        /// <summary>
        /// Indica si el token está activo (no revocado y no expirado).
        /// </summary>
        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
