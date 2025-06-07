using GestMantIA.Shared.Identity.DTOs;
using System.Threading.Tasks;
using GestMantIA.Application; // Para la clase Result y Result<T>

namespace GestMantIA.Application.Interfaces
{
    public interface IAccountService
    {
        /// <summary>
        /// Inicia el proceso de restablecimiento de contraseña para un usuario.
        /// Genera un token y envía un correo electrónico al usuario.
        /// </summary>
        /// <param name="request">DTO con el correo electrónico del usuario.</param>
        /// <param name="origin">URL base para construir el enlace de restablecimiento (ej. https://localhost:5001).</param>
        /// <returns>Un resultado indicando si la operación fue exitosa.</returns>
        Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);

        /// <summary>
        /// Restablece la contraseña de un usuario utilizando un token válido.
        /// </summary>
        /// <param name="request">DTO con el token, correo y la nueva contraseña.</param>
        /// <returns>Un resultado indicando si la operación fue exitosa.</returns>
        Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
