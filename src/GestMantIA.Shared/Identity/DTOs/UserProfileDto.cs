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
        public string UserId { get; init; } = string.Empty;

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
    }
}
