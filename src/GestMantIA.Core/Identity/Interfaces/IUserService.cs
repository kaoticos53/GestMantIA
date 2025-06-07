using GestMantIA.Core.Shared;
using GestMantIA.Shared.Identity.DTOs;
using GestMantIA.Shared.Identity.DTOs.Requests;
using GestMantIA.Shared.Identity.DTOs.Responses;

namespace GestMantIA.Core.Identity.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de usuarios.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Obtiene el perfil de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>El perfil del usuario o null si no se encuentra.</returns>
        Task<UserResponseDTO?> GetUserProfileAsync(Guid userId);

        /// <summary>
        /// Actualiza el perfil de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario a actualizar.</param>
        /// <param name="profile">Datos actualizados del perfil.</param>
        /// <returns>True si la actualización fue exitosa, false en caso contrario.</returns>
        Task<bool> UpdateUserProfileAsync(Guid userId, GestMantIA.Shared.Identity.DTOs.Requests.UpdateProfileDTO profile);

        /// <summary>
        /// Obtiene una lista paginada de usuarios.
        /// </summary>
        /// <param name="pageNumber">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Tamaño de página (por defecto 10).</param>
        /// <param name="searchTerm">Término de búsqueda (opcional).</param>
        /// <param name="activeOnly">Indica si solo se deben devolver usuarios activos (opcional, por defecto true).</param>
        /// <returns>Lista paginada de perfiles de usuario.</returns>
        Task<PagedResult<UserResponseDTO>> GetAllUsersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool activeOnly = true);

        /// <summary>
        /// Bloquea un usuario por un período de tiempo específico.
        /// </summary>
        /// <param name="userId">ID del usuario a bloquear.</param>
        /// <param name="duration">Duración del bloqueo (opcional, si no se especifica, se usa el tiempo por defecto).</param>
        /// <param name="reason">Razón del bloqueo (opcional).</param>
        /// <returns>True si el bloqueo fue exitoso, false en caso contrario.</returns>
        Task<bool> LockUserAsync(Guid userId, TimeSpan? duration = null, string? reason = null);

        /// <summary>
        /// Desbloquea un usuario previamente bloqueado.
        /// </summary>
        /// <param name="userId">ID del usuario a desbloquear.</param>
        /// <returns>True si el desbloqueo fue exitoso, false en caso contrario.</returns>
        Task<bool> UnlockUserAsync(Guid userId);

        /// <summary>
        /// Verifica si un usuario está actualmente bloqueado.
        /// </summary>
        /// <param name="userId">ID del usuario a verificar.</param>
        /// <returns>True si el usuario está bloqueado, false en caso contrario.</returns>
        Task<bool> IsUserLockedOutAsync(Guid userId);

        /// <summary>
        /// Obtiene la información de bloqueo de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Información de bloqueo o null si el usuario no existe o no está bloqueado.</returns>
        Task<UserLockoutInfo?> GetUserLockoutInfoAsync(Guid userId);

        #region Gestión de Usuarios

        /// <summary>
        /// Crea un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="createUserDto">Datos del usuario a crear.</param>
        /// <returns>El usuario creado o null si no se pudo crear.</returns>
        Task<UserResponseDTO> CreateUserAsync(CreateUserDTO createUserDto);

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        /// <param name="userId">ID del usuario a actualizar.</param>
        /// <param name="userDto">Datos actualizados del usuario.</param>
        /// <returns>El usuario actualizado o null si no se encontró.</returns>
        Task<UserResponseDTO> UpdateUserAsync(Guid userId, UpdateUserDTO userDto);

        /// <summary>
        /// Elimina un usuario del sistema.
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar.</param>
        /// <param name="hardDelete">Si es true, elimina el usuario permanentemente.</param>
        /// <returns>True si se eliminó correctamente, false en caso contrario.</returns>
        Task<bool> DeleteUserAsync(Guid userId, bool hardDelete = false);

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>El usuario o null si no se encuentra.</returns>
        Task<UserResponseDTO?> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Obtiene los roles de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Lista de nombres de roles.</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);

        #endregion

        #region Gestión de Contraseñas y Confirmación de Email

        /// <summary>
        /// Genera un token para restablecer la contraseña de un usuario.
        /// </summary>
        /// <param name="userIdOrEmail">ID o email del usuario.</param>
        /// <returns>El token de restablecimiento o null si el usuario no se encuentra o no es válido.</returns>
        Task<string?> GetPasswordResetTokenAsync(String userIdOrEmail);

        /// <summary>
        /// Restablece la contraseña de un usuario utilizando un token.
        /// </summary>
        /// <param name="resetPasswordDto">DTO con la información para el restablecimiento.</param>
        /// <returns>True si la contraseña se restableció correctamente, false en caso contrario.</returns>
        Task<bool> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto);

        /// <summary>
        /// Cambia la contraseña de un usuario autenticado.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="changePasswordDto">DTO con la contraseña actual y la nueva contraseña.</param>
        /// <returns>True si la contraseña se cambió correctamente, false en caso contrario.</returns>
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDTO changePasswordDto);

        /// <summary>
        /// Confirma la dirección de correo electrónico de un usuario utilizando un token.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="token">Token de confirmación de email.</param>
        /// <returns>True si el email se confirmó correctamente, false en caso contrario.</returns>
        Task<bool> ConfirmEmailAsync(Guid userId, string token);

        /// <summary>
        /// Genera y reenvía un nuevo token de confirmación de email para un usuario.
        /// </summary>
        /// <param name="userIdOrEmail">ID o email del usuario.</param>
        /// <returns>El nuevo token de confirmación o null si el usuario no se encuentra o el email ya está confirmado.</returns>
        Task<string?> ResendConfirmationEmailAsync(String userIdOrEmail);

        #endregion
    }
}
