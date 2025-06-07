namespace GestMantIA.Shared.Identity.DTOs
{
    /// <summary>
    /// Data Transfer Object para el perfil de usuario.
    /// </summary>
    public record UserProfileDto
    {
        /// <summary>
        /// Identificador único del perfil de usuario (de BaseEntity).
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Identificador del usuario (clave foránea a ApplicationUser.Id).
        /// </summary>
        public Guid UserId { get; init; } = Guid.Empty;

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public string? FirstName { get; init; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public string? LastName { get; init; }

        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        public DateTime? DateOfBirth { get; init; }

        /// <summary>
        /// URL del avatar del usuario.
        /// </summary>
        public string? AvatarUrl { get; init; }

        /// <summary>
        /// Biografía corta del usuario.
        /// </summary>
        public string? Bio { get; init; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        public string? PhoneNumber { get; init; }

        /// <summary>
        /// Dirección (calle y número).
        /// </summary>
        public string? StreetAddress { get; init; }

        /// <summary>
        /// Ciudad.
        /// </summary>
        public string? City { get; init; }

        /// <summary>
        /// Estado o provincia.
        /// </summary>
        public string? StateProvince { get; init; }

        /// <summary>
        /// Código postal.
        /// </summary>
        public string? PostalCode { get; init; }

        /// <summary>
        /// País.
        /// </summary>
        public string? Country { get; init; }

        // Propiedades de ApplicationUser
        public string? UserName { get; init; }
        public string? Email { get; init; }
        public bool EmailConfirmed { get; init; }
        public bool PhoneNumberConfirmed { get; init; } // Ya existe PhoneNumber, pero este es el estado de confirmación.
        public bool TwoFactorEnabled { get; init; }
        public DateTimeOffset? LockoutEnd { get; init; }
        public bool LockoutEnabled { get; init; }
        public bool IsActive { get; init; } = true;
        public List<string> Roles { get; init; } = new();

        // Propiedades de IAuditableEntity (pueden venir de UserProfile o ApplicationUser)
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public string? CreatedBy { get; init; }
        public string? UpdatedBy { get; init; }
    }
}
