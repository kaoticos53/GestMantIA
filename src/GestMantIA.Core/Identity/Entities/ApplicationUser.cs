using System.ComponentModel.DataAnnotations;
using GestMantIA.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase que representa a un usuario en el sistema.
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ApplicationUser"/>
        /// </summary>
        public ApplicationUser()
        {
            LockoutReason = string.Empty;
            FullName = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            UserRoles = new HashSet<ApplicationUserRole>();
            RefreshTokens = new HashSet<RefreshToken>();
            CreatedAt = DateTime.UtcNow;
        }

        // Propiedades de IdentityUser<Guid> que ya están definidas en la clase base:
        // - UserName
        // - Email
        // - PasswordHash
        // - PhoneNumber
        // - EmailConfirmed
        // - PhoneNumberConfirmed
        // - TwoFactorEnabled
        // - LockoutEnd
        // - LockoutEnabled
        // - AccessFailedCount
        // - SecurityStamp
        // - ConcurrencyStamp
        // - NormalizedUserName
        // - NormalizedEmail

        /// <summary>
        /// Indica si el usuario está activo en el sistema.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Razón del bloqueo del usuario (si está bloqueado).
        /// </summary>
        public string LockoutReason { get; set; }

        /// <summary>
        /// Fecha en que se produjo el bloqueo del usuario.
        /// </summary>
        public DateTime? LockoutDate { get; set; }

        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        [StringLength(100)]
        public string FullName { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de creación del usuario.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha y hora de la última modificación del usuario.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Indica si el usuario ha sido eliminado (borrado lógico).
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Fecha y hora del último inicio de sesión del usuario.
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Fecha en que se marcó como eliminado.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Identificador del usuario que marcó como eliminado.
        /// </summary>
        public Guid? DeletedBy { get; set; }

        /// <summary>
        /// Relación con los roles del usuario.
        /// </summary>
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        /// <summary>
        /// Tokens de actualización asociados al usuario.
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
