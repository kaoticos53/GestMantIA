using GestMantIA.Core.Identity.Entities;
using GestMantIA.Shared.Identity.DTOs;

namespace GestMantIA.Core.Identity.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de roles y permisos.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Obtiene todos los roles disponibles.
        /// </summary>
        /// <returns>Lista de roles.</returns>
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();

        /// <summary>
        /// Obtiene un rol por su ID.
        /// </summary>
        /// <param name="roleId">ID del rol.</param>
        /// <returns>El rol si existe, de lo contrario null.</returns>
        Task<RoleDto?> GetRoleByIdAsync(string roleId);

        /// <summary>
        /// Crea un nuevo rol.
        /// </summary>
        /// <param name="RoleDto">Datos del rol a crear.</param>
        /// <returns>Resultado de la operación.</returns>
        Task<RoleResult> CreateRoleAsync(RoleDto RoleDto);

        /// <summary>
        /// Actualiza un rol existente.
        /// </summary>
        /// <param name="RoleDto">Datos actualizados del rol.</param>
        /// <returns>Resultado de la operación.</returns>
        Task<RoleResult> UpdateRoleAsync(RoleDto RoleDto);

        /// <summary>
        /// Elimina un rol por su ID.
        /// </summary>
        /// <param name="roleId">ID del rol a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        Task<RoleResult> DeleteRoleAsync(string roleId);

        /// <summary>
        /// Asigna un rol a un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="roleName">Nombre del rol.</param>
        /// <returns>Resultado de la operación.</returns>
        Task<RoleResult> AddUserToRoleAsync(Guid userId, string roleName);

        /// <summary>
        /// Elimina un rol de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="roleName">Nombre del rol.</param>
        /// <returns>Resultado de la operación.</returns>
        Task<RoleResult> RemoveUserFromRoleAsync(Guid userId, string roleName);

        /// <summary>
        /// Obtiene los roles de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Lista de roles del usuario.</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// Obtiene los usuarios que tienen un rol específico.
        /// </summary>
        /// <param name="roleName">Nombre del rol.</param>
        /// <returns>Lista de usuarios con el rol especificado.</returns>
        Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName);

        /// <summary>
        /// Verifica si un usuario tiene un rol específico.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="roleName">Nombre del rol.</param>
        /// <returns>True si el usuario tiene el rol, de lo contrario false.</returns>
        Task<bool> IsUserInRoleAsync(Guid userId, string roleName);
    }

    /// <summary>
    /// Clase para el resultado de las operaciones de roles.
    /// </summary>
    public class RoleResult
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="RoleResult"/>.
        /// </summary>
        public RoleResult()
        {
            Success = false;
            Message = string.Empty;
            Errors = new List<string>();
        }

        /// <summary>
        /// Indica si la operación fue exitosa.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Lista de errores, si los hay.
        /// </summary>
        public IEnumerable<string> Errors { get; set; }

        /// <summary>
        /// Crea un resultado exitoso.
        /// </summary>
        public static RoleResult Succeeded(string? message = null)
        {
            return new RoleResult
            {
                Success = true,
                Message = message ?? "Operación completada con éxito"
            };
        }

        /// <summary>
        /// Crea un resultado fallido.
        /// </summary>
        public static RoleResult Failed(string error, IEnumerable<string>? errors = null)
        {
            var result = new RoleResult
            {
                Success = false,
                Message = error ?? "Error en la operación"
            };

            if (errors != null)
            {
                result.Errors = errors;
            }

            return result;
        }
    }
}
