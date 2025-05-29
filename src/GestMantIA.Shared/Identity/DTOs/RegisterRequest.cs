using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs
{
    /// <summary>
    /// DTO para la solicitud de registro de un nuevo usuario.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="RegisterRequest"/>
        /// </summary>
        public RegisterRequest()
        {
            UserName = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            FullName = string.Empty;
            PhoneNumber = string.Empty;
        }

        /// <summary>
        /// Nombre de usuario único para el nuevo usuario.
        /// </summary>
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de usuario")]
        public string UserName { get; set; }

        /// <summary>
        /// Correo electrónico del nuevo usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; }

        /// <summary>
        /// Contraseña del nuevo usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Confirmación de la contraseña del nuevo usuario.
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Nombre completo del nuevo usuario.
        /// </summary>
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string FullName { get; set; }

        /// <summary>
        /// Número de teléfono del nuevo usuario.
        /// </summary>
        [Phone(ErrorMessage = "El formato del número de teléfono no es válido")]
        public string PhoneNumber { get; set; }
    }
}
