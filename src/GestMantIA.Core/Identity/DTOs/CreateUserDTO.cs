using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Core.Identity.DTOs
{
    /// <summary>
    /// DTO para la creación de un nuevo usuario.
    /// </summary>
    public class CreateUserDTO
    {
        /// <summary>
        /// Nombre de usuario para el inicio de sesión.
        /// </summary>
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public string Username { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Confirmación de la contraseña.
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
        public string FirstName { get; set; }

        /// <summary>
        /// Apellidos del usuario.
        /// </summary>
        [StringLength(100, ErrorMessage = "Los apellidos no pueden tener más de 100 caracteres")]
        public string LastName { get; set; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        [Phone(ErrorMessage = "El formato del número de teléfono no es válido")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Indica si el usuario debe confirmar su correo electrónico.
        /// </summary>
        public bool RequireEmailConfirmation { get; set; } = true;
    }
}
