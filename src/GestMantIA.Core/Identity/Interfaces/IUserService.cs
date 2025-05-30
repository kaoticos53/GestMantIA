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
        Task<UserResponseDTO?> GetUserProfileAsync(string userId);

        /// <summary>
        /// Actualiza el perfil de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario a actualizar.</param>
        /// <param name="profile">Datos actualizados del perfil.</param>
        /// <returns>True si la actualización fue exitosa, false en caso contrario.</returns>
        Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDTO profile);

        /// <summary>
        /// Busca usuarios según los criterios especificados.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (opcional).</param>
        /// <param name="pageNumber">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Tamaño de página (por defecto 10).</param>
        /// <returns>Lista paginada de perfiles de usuario.</returns>
        Task<PagedResult<UserResponseDTO>> SearchUsersAsync(
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// Bloquea un usuario por un período de tiempo específico.
        /// </summary>
        /// <param name="userId">ID del usuario a bloquear.</param>
        /// <param name="duration">Duración del bloqueo (opcional, si no se especifica, se usa el tiempo por defecto).</param>
        /// <param name="reason">Razón del bloqueo (opcional).</param>
        /// <returns>True si el bloqueo fue exitoso, false en caso contrario.</returns>
        Task<bool> LockUserAsync(string userId, TimeSpan? duration = null, string? reason = null);

        /// <summary>
        /// Desbloquea un usuario previamente bloqueado.
        /// </summary>
        /// <param name="userId">ID del usuario a desbloquear.</param>
        /// <returns>True si el desbloqueo fue exitoso, false en caso contrario.</returns>
        Task<bool> UnlockUserAsync(string userId);

        /// <summary>
        /// Verifica si un usuario está actualmente bloqueado.
        /// </summary>
        /// <param name="userId">ID del usuario a verificar.</param>
        /// <returns>True si el usuario está bloqueado, false en caso contrario.</returns>
        Task<bool> IsUserLockedOutAsync(string userId);

        /// <summary>
        /// Obtiene la información de bloqueo de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Información de bloqueo o null si el usuario no existe o no está bloqueado.</returns>
        Task<UserLockoutInfo?> GetUserLockoutInfoAsync(string userId);

        #region Gestión de Usuarios

        /// <summary>
        /// Crea un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="createUserDto">Datos del usuario a crear.</param>
        /// <param name="roleNames">Roles a asignar al usuario (opcional).</param>
        /// <returns>El usuario creado o null si no se pudo crear.</returns>
        Task<UserResponseDTO> CreateUserAsync(CreateUserDTO createUserDto, IEnumerable<string>? roleNames = null);

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        /// <param name="userId">ID del usuario a actualizar.</param>
        /// <param name="userDto">Datos actualizados del usuario.</param>
        /// <returns>El usuario actualizado o null si no se encontró.</returns>
        Task<UserResponseDTO> UpdateUserAsync(string userId, UpdateUserDTO userDto);

        /// <summary>
        /// Elimina un usuario del sistema.
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar.</param>
        /// <param name="hardDelete">Si es true, elimina el usuario permanentemente.</param>
        /// <returns>True si se eliminó correctamente, false en caso contrario.</returns>
        Task<bool> DeleteUserAsync(string userId, bool hardDelete = false);

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>El usuario o null si no se encuentra.</returns>
        Task<UserResponseDTO?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Obtiene todos los usuarios con paginación.
        /// </summary>
        /// <param name="pageNumber">Número de página (1-based).</param>
        /// <param name="pageSize">Tamaño de la página.</param>
        /// <param name="searchTerm">Término de búsqueda opcional.</param>
        /// <param name="activeOnly">Si es true, solo devuelve usuarios activos.</param>
        /// <returns>Lista paginada de usuarios.</returns>
        Task<PagedResult<UserResponseDTO>> GetAllUsersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            bool activeOnly = false);

        /// <summary>
        /// Asigna roles a un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="roleNames">Nombres de los roles a asignar.</param>
        /// <returns>True si se asignaron los roles correctamente.</returns>
        Task<bool> AssignRolesToUserAsync(string userId, IEnumerable<string> roleNames);

        /// <summary>
        /// Elimina roles de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="roleNames">Nombres de los roles a eliminar.</param>
        /// <returns>True si se eliminaron los roles correctamente.</returns>
        Task<bool> RemoveRolesFromUserAsync(string userId, IEnumerable<string> roleNames);

        /// <summary>
        /// Obtiene los roles de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Lista de nombres de roles.</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);

        #endregion

        #region Gestión de Contraseñas y Confirmación de Email

        /// <summary>
        /// Genera un token para restablecer la contraseña de un usuario.
        /// </summary>
        /// <param name="userIdOrEmail">ID o email del usuario.</param>
        /// <returns>El token de restablecimiento o null si el usuario no se encuentra o no es válido.</returns>
        Task<string?> GetPasswordResetTokenAsync(string userIdOrEmail);

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
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO changePasswordDto);

        /// <summary>
        /// Confirma la dirección de correo electrónico de un usuario utilizando un token.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="token">Token de confirmación de email.</param>
        /// <returns>True si el email se confirmó correctamente, false en caso contrario.</returns>
        Task<bool> ConfirmEmailAsync(string userId, string token);

        /// <summary>
        /// Genera y reenvía un nuevo token de confirmación de email para un usuario.
        /// </summary>
        /// <param name="userIdOrEmail">ID o email del usuario.</param>
        /// <returns>El nuevo token de confirmación o null si el usuario no se encuentra o el email ya está confirmado.</returns>
        Task<string?> ResendConfirmationEmailAsync(string userIdOrEmail);

        #endregion
    }
}
