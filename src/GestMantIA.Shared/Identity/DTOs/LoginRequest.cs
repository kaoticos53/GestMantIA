using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs
{
    /// <summary>
    /// DTO para la solicitud de inicio de sesión de un usuario.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="LoginRequest"/>
        /// </summary>
        public LoginRequest()
        {
            UsernameOrEmail = string.Empty;
            Password = string.Empty;
        }

        /// <summary>
        /// Nombre de usuario o correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El nombre de usuario o correo electrónico es obligatorio")]
        public string UsernameOrEmail { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Indica si se debe recordar la sesión del usuario.
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }
}
