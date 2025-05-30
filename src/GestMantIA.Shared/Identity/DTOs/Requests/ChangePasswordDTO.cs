using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs.Requests
{
    /// <summary>
    /// DTO para cambiar la contraseña de un usuario autenticado.
    /// </summary>
    public class ChangePasswordDTO
    {
        /// <summary>
        /// Contraseña actual del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// Nueva contraseña.
        /// </summary>
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [StringLength(100, ErrorMessage = "La nueva contraseña debe tener al menos {2} caracteres y un máximo de {1}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la nueva contraseña.
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
