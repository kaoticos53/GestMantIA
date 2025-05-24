using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Core.Identity.DTOs.Requests
{
    /// <summary>
    /// DTO para la creación de un nuevo usuario
    /// </summary>
    public class CreateUserDTO
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="CreateUserDTO"/>
        /// </summary>
        public CreateUserDTO()
        {
            Username = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
        }

        /// <summary>
        /// Nombre de usuario (debe ser único)
        /// </summary>
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre {2} y {1} caracteres")]
        public string Username { get; set; }

        /// <summary>
        /// Correo electrónico del usuario (debe ser único)
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos {2} caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Confirmación de la contraseña
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de {1} caracteres")]
        public string FirstName { get; set; }

        /// <summary>
        /// Apellidos del usuario
        /// </summary>
        [StringLength(100, ErrorMessage = "Los apellidos no pueden tener más de {1} caracteres")]
        public string LastName { get; set; }

        /// <summary>
        /// Número de teléfono del usuario
        /// </summary>
        [Phone(ErrorMessage = "El formato del número de teléfono no es válido")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Indica si se requiere confirmación de correo electrónico
        /// </summary>
        public bool RequireEmailConfirmation { get; set; } = true;
    }
}
