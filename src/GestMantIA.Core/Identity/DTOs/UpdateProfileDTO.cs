using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Core.Identity.DTOs
{
    /// <summary>
    /// DTO para actualizar el perfil de usuario.
    /// </summary>
    public class UpdateProfileDTO
    {
        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        public string FirstName { get; set; }

        /// <summary>
        /// Apellidos del usuario.
        /// </summary>
        [StringLength(100, ErrorMessage = "Los apellidos no pueden tener más de 100 caracteres.")]
        public string LastName { get; set; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        [Phone(ErrorMessage = "El número de teléfono no es válido.")]
        [StringLength(20, ErrorMessage = "El número de teléfono no puede tener más de 20 caracteres.")]
        public string PhoneNumber { get; set; }
    }
}
