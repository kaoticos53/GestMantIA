using AutoMapper;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Shared.Identity.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace GestMantIA.Application.Features.UserManagement.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de roles y permisos.
    /// </summary>
    public class RoleService : IRoleService, IDisposable
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RoleService> _logger;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private bool _disposed = false;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="RoleService"/>
        /// </summary>
        public RoleService(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<RoleService> logger,
            IRoleRepository roleRepository,
            IMapper mapper)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _roleRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<RoleDto>>(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los roles");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<RoleDto?> GetRoleByIdAsync(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
                throw new ArgumentNullException(nameof(roleId));

            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("No se encontró el rol con ID: {RoleId}", roleId);
                    return null;
                }

                return _mapper.Map<RoleDto>(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el rol con ID: {RoleId}", roleId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<RoleResult> CreateRoleAsync(RoleDto RoleDto)
        {
            if (RoleDto == null)
                throw new ArgumentNullException(nameof(RoleDto));

            try
            {
                // Verificar si el rol ya existe
                var roleExists = await _roleManager.RoleExistsAsync(RoleDto.Name);
                if (roleExists)
                {
                    return RoleResult.Failed($"El rol '{RoleDto.Name}' ya existe.");
                }

                // Crear el nuevo rol
                var role = new ApplicationRole
                {
                    Name = RoleDto.Name,
                    Description = RoleDto.Description ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Error al crear el rol: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return RoleResult.Failed("Error al crear el rol.",
                        result.Errors.Select(e => e.Description));
                }

                // Asignar permisos si se especificaron
                if (RoleDto.Permissions != null && RoleDto.Permissions.Any())
                {
                    await AssignPermissionsToRoleAsync(role, RoleDto.Permissions);
                }

                _logger.LogInformation("Rol creado exitosamente: {RoleName}", role.Name);
                return RoleResult.Succeeded("Rol creado exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el rol: {RoleName}", RoleDto.Name);
                return RoleResult.Failed($"Error al crear el rol: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<RoleResult> UpdateRoleAsync(RoleDto RoleDto)
        {
            if (RoleDto == null)
                throw new ArgumentNullException(nameof(RoleDto));

            try
            {
                var role = await _roleManager.FindByIdAsync(RoleDto.Id);
                if (role == null)
                {
                    return RoleResult.Failed("El rol especificado no existe.");
                }

                // Actualizar propiedades del rol
                if (RoleDto.Name != null)
                {
                    role.Name = RoleDto.Name;
                }
                
                if (RoleDto.Description != null)
                {
                    role.Description = RoleDto.Description;
                }
                
                // Usar reflexión para establecer UpdatedAt si existe
                var updatedAtProperty = role.GetType().GetProperty("UpdatedAt");
                updatedAtProperty?.SetValue(role, DateTime.UtcNow);

                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Error al actualizar el rol: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return RoleResult.Failed("Error al actualizar el rol.",
                        result.Errors.Select(e => e.Description));
                }

                // Actualizar permisos si se especificaron
                if (RoleDto.Permissions != null)
                {
                    await UpdateRolePermissionsAsync(role, RoleDto.Permissions);
                }

                _logger.LogInformation("Rol actualizado exitosamente: {RoleName}", role.Name);
                return RoleResult.Succeeded("Rol actualizado exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el rol con ID: {RoleId}", RoleDto.Id);
                return RoleResult.Failed($"Error al actualizar el rol: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<RoleResult> DeleteRoleAsync(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
                throw new ArgumentNullException(nameof(roleId));

            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return RoleResult.Failed("El rol especificado no existe.");
                }

                // Verificar si hay usuarios con este rol
                if (string.IsNullOrEmpty(role.Name))
                {
                    _logger.LogError("El nombre del rol con ID {RoleId} es nulo o vacío.", role.Id);
                    // Considerar devolver un error específico o lanzar una excepción
                    return RoleResult.Failed("Error interno: El nombre del rol es inválido.");
                }
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                if (usersInRole.Any())
                {
                    return RoleResult.Failed("No se puede eliminar el rol porque tiene usuarios asignados.");
                }

                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Error al eliminar el rol: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return RoleResult.Failed("Error al eliminar el rol.",
                        result.Errors.Select(e => e.Description));
                }

                _logger.LogInformation("Rol eliminado exitosamente: {RoleName}", role.Name);
                return RoleResult.Succeeded("Rol eliminado exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el rol con ID: {RoleId}", roleId);
                return RoleResult.Failed($"Error al eliminar el rol: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<RoleResult> AddUserToRoleAsync(Guid userId, string roleName)
        {
            if (userId == Guid.Empty)
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return RoleResult.Failed("El usuario especificado no existe.");
                }

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    return RoleResult.Failed("El rol especificado no existe.");
                }

                // Verificar si el usuario ya tiene el rol
                var isInRole = await _userManager.IsInRoleAsync(user, roleName);
                if (isInRole)
                {
                    return RoleResult.Failed($"El usuario ya tiene asignado el rol '{roleName}'.");
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Error al asignar el rol al usuario: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return RoleResult.Failed("Error al asignar el rol al usuario.",
                        result.Errors.Select(e => e.Description));
                }

                _logger.LogInformation("Rol '{RoleName}' asignado exitosamente al usuario {UserId}", roleName, userId);
                return RoleResult.Succeeded("Rol asignado exitosamente al usuario.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar el rol '{RoleName}' al usuario {UserId}", roleName, userId);
                return RoleResult.Failed($"Error al asignar el rol: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<RoleResult> RemoveUserFromRoleAsync(Guid userId, string roleName)
        {
            if (userId == Guid.Empty)
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return RoleResult.Failed("El usuario especificado no existe.");
                }

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    return RoleResult.Failed("El rol especificado no existe.");
                }

                // Verificar si el usuario tiene el rol
                var isInRole = await _userManager.IsInRoleAsync(user, roleName);
                if (!isInRole)
                {
                    return RoleResult.Failed($"El usuario no tiene asignado el rol '{roleName}'.");
                }

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Error al eliminar el rol del usuario: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return RoleResult.Failed("Error al eliminar el rol del usuario.",
                        result.Errors.Select(e => e.Description));
                }

                _logger.LogInformation("Rol '{RoleName}' eliminado exitosamente del usuario {UserId}", roleName, userId);
                return RoleResult.Succeeded("Rol eliminado exitosamente del usuario.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el rol '{RoleName}' del usuario {UserId}", roleName, userId);
                return RoleResult.Failed($"Error al eliminar el rol: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentNullException(nameof(userId));

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return Enumerable.Empty<string>();
                }

                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los roles del usuario con ID: {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            try
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning("No se encontró el rol: {RoleName}", roleName);
                    return new List<ApplicationUser>();
                }

                // Asegurarse de que el rol tenga un nombre válido
                if (string.IsNullOrEmpty(role.Name))
                {
                    _logger.LogWarning("El rol encontrado no tiene un nombre válido: {RoleId}", role.Id);
                    return new List<ApplicationUser>();
                }

                var users = await _userManager.GetUsersInRoleAsync(role.Name);
                return users?.ToList() ?? new List<ApplicationUser>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los usuarios del rol: {RoleName}", roleName);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> IsUserInRoleAsync(Guid userId, string roleName)
        {
            if (userId == Guid.Empty)
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return false;
                }

                return await _userManager.IsInRoleAsync(user, roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el usuario {UserId} tiene el rol {RoleName}", userId, roleName);
                throw;
            }
        }

        #region Métodos privados

        private async Task AssignPermissionsToRoleAsync(ApplicationRole role, IEnumerable<string> permissions)
        {
            // Implementar lógica para asignar permisos al rol
            // Esto puede involucrar una tabla de relación entre roles y permisos
            await Task.CompletedTask;
        }

        private async Task UpdateRolePermissionsAsync(ApplicationRole role, IEnumerable<string> permissions)
        {
            // Implementar lógica para actualizar los permisos del rol
            // Esto puede involucrar eliminar permisos existentes y agregar los nuevos
            await Task.CompletedTask;
        }

        #endregion

        #region IDisposable Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
