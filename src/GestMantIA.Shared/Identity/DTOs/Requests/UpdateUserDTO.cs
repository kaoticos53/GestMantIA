using System;
using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs.Requests
{
    /// <summary>
    /// DTO para actualizar un usuario existente
    /// </summary>
    public class UpdateUserDTO
    {
        public UpdateUserDTO()
        {
            Id = string.Empty;
            UserName = string.Empty;
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
        }
        /// <summary>
        /// Identificador único del usuario
        /// </summary>
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario (debe ser único)
        /// </summary>
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Nombre de usuario")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario (debe ser único)
        /// </summary>
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de {1} caracteres")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Apellidos del usuario
        /// </summary>
        [StringLength(100, ErrorMessage = "Los apellidos no pueden tener más de {1} caracteres")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Número de teléfono del usuario
        /// </summary>
        [Phone(ErrorMessage = "El formato del número de teléfono no es válido")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el usuario está activo
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Indica si el correo electrónico está confirmado
        /// </summary>
        public bool? EmailConfirmed { get; set; }

        /// <summary>
        /// Indica si el número de teléfono está confirmado
        /// </summary>
        public bool? PhoneNumberConfirmed { get; set; }
        /// <summary>
        /// Indica si la autenticación en dos factores está habilitada
        /// </summary>
        public bool? TwoFactorEnabled { get; set; }
    }
}
