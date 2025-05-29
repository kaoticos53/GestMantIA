using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GestMantIA.Shared.Identity.DTOs;
using GestMantIA.Shared.Identity.DTOs.Requests;
using GestMantIA.Shared.Identity.DTOs.Responses;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Shared;
using GestMantIA.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GestMantIA.Infrastructure.Services
{
    public class UserService : IUserService, IDisposable
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private bool _disposed = false;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper,
            ILogger<UserService> logger,
            ApplicationDbContext context,
            IOptions<IdentityOptions> identityOptions)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _identityOptions = identityOptions ?? throw new ArgumentNullException(nameof(identityOptions));
        }

        public async Task<GestMantIA.Shared.Identity.DTOs.Responses.UserResponseDTO?> GetUserProfileAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return null;
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return null;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userResponse = _mapper.Map<GestMantIA.Shared.Identity.DTOs.Responses.UserResponseDTO>(user);
                userResponse.Roles = userRoles.ToList();

                return userResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario con ID {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDTO profile)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return false;
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return false;
                }

                _mapper.Map(profile, user);
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al actualizar el perfil del usuario {UserId}: {Errors}", userId, errors);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perfil del usuario con ID {UserId}", userId);
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                }

                // Free any unmanaged objects here.

                _disposed = true;
            }
        }

        public async Task<PagedResult<GestMantIA.Shared.Identity.DTOs.Responses.UserResponseDTO>> SearchUsersAsync(
            string? searchTerm = null, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(u => 
                        u.UserName != null && u.UserName.ToLower().Contains(searchTermLower) ||
                        u.Email != null && u.Email.ToLower().Contains(searchTermLower) ||
                        u.FirstName != null && u.FirstName.ToLower().Contains(searchTermLower) ||
                        u.LastName != null && u.LastName.ToLower().Contains(searchTermLower));
                }


                var totalCount = await query.CountAsync();
                var users = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDtos = _mapper.Map<List<GestMantIA.Shared.Identity.DTOs.Responses.UserResponseDTO>>(users);

                foreach (var userDto in userDtos)
                {
                    var userId = Guid.Parse(userDto.Id);
                    var user = users.First(u => u.Id == userId);
                    var roles = await _userManager.GetRolesAsync(user);
                    userDto.Roles = roles.ToList();
                }

                return new PagedResult<GestMantIA.Shared.Identity.DTOs.Responses.UserResponseDTO>
                {
                    Items = userDtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuarios con el término: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<bool> LockUserAsync(string userId, TimeSpan? duration = null, string? reason = null)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return false;
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null)
                {
                    _logger.LogWarning("Intento de bloquear usuario no encontrado con ID {UserId}", userId);
                    return false;
                }

                // Asegurarse de que el bloqueo esté habilitado para el usuario
                if (!await _userManager.GetLockoutEnabledAsync(user))
                {
                    await _userManager.SetLockoutEnabledAsync(user, true);
                }

                DateTimeOffset lockoutEnd;
                if (duration.HasValue)
                {
                    lockoutEnd = DateTimeOffset.UtcNow.Add(duration.Value);
                }
                else
                {
                    // Usar la duración de bloqueo por defecto de Identity si no se especifica
                    // Esto podría requerir configuración adicional en IdentityOptions.Lockout
                    lockoutEnd = DateTimeOffset.UtcNow.Add(_identityOptions.Value.Lockout.DefaultLockoutTimeSpan);
                }

                user.LockoutReason = reason ?? string.Empty;
                var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario con ID {UserId} bloqueado hasta {LockoutEnd} por razón: {Reason}", userId, lockoutEnd, reason ?? "N/A");
                }
                else
                {
                    _logger.LogError("Error al bloquear usuario con ID {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al bloquear usuario con ID {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UnlockUserAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return false;
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null)
                {
                    _logger.LogWarning("Intento de desbloquear usuario no encontrado con ID {UserId}", userId);
                    return false;
                }

                // Para desbloquear, se establece la fecha de fin de bloqueo a null o a un tiempo pasado.
                // Identity considera null como no bloqueado, o Now/UtcNow.
                var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                if (result.Succeeded)
                {
                     user.LockoutReason = string.Empty; // Limpiar la razón del bloqueo
                     await _userManager.UpdateAsync(user); // Guardar el cambio de LockoutReason
                    _logger.LogInformation("Usuario con ID {UserId} desbloqueado.", userId);
                }
                else
                {
                    _logger.LogError("Error al desbloquear usuario con ID {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al desbloquear usuario con ID {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsUserLockedOutAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return false;
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null)
                {
                    _logger.LogWarning("Intento de verificar bloqueo de usuario no encontrado con ID {UserId}", userId);
                    return false; // O lanzar excepción, dependiendo de cómo se quiera manejar usuarios no encontrados
                }
                return await _userManager.IsLockedOutAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al verificar si el usuario con ID {UserId} está bloqueado", userId);
                throw;
            }
        }

        public async Task<GestMantIA.Shared.Identity.DTOs.UserLockoutInfo?> GetUserLockoutInfoAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return null;
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró usuario con ID {UserId} al obtener información de bloqueo.", userId);
                    return null;
                }

                var isLockedOut = await _userManager.IsLockedOutAsync(user);
                var lockoutEnd = user.LockoutEnd;

                return new GestMantIA.Shared.Identity.DTOs.UserLockoutInfo
                {
                    UserId = user.Id.ToString(),
                    UserName = user.UserName ?? string.Empty,
                    IsLockedOut = isLockedOut,
                    LockoutEnd = lockoutEnd?.UtcDateTime,
                    Reason = user.LockoutReason // Corregido: la propiedad en UserLockoutInfo se llama Reason
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al obtener información de bloqueo para el usuario con ID {UserId}", userId);
                throw;
            }
        }

        public async Task<GestMantIA.Shared.Identity.DTOs.Responses.UserResponseDTO> CreateUserAsync(GestMantIA.Shared.Identity.DTOs.Requests.CreateUserDTO createUserDto, IEnumerable<string>? roleNames = null)
        {
            try
            {
                var user = _mapper.Map<ApplicationUser>(createUserDto);
                user.UserName = createUserDto.Email; // Asegurar que el nombre de usuario sea el email
                
                var result = await _userManager.CreateAsync(user, createUserDto.Password);
                
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al crear el usuario {Email}: {Errors}", createUserDto.Email, errors);
                    throw new ApplicationException($"Error al crear el usuario: {errors}");
                }

                // Asignar roles si se especificaron
                if (roleNames != null && roleNames.Any())
                {
                    var rolesToAdd = roleNames.Where(r => _roleManager.Roles.Any(role => role.Name == r)).ToList();
                    if (rolesToAdd.Any())
                    {
                        var roleResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                        if (!roleResult.Succeeded)
                        {
                            _logger.LogWarning("No se pudieron asignar los roles al usuario {UserId}", user.Id);
                        }
                    }
                }

                var userDto = _mapper.Map<GestMantIA.Shared.Identity.DTOs.Responses.UserResponseDTO>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario con email {Email}", createUserDto.Email);
                throw;
            }
        }

        public async Task<UserResponseDTO> UpdateUserAsync(string userId, UpdateUserDTO updateUserDto)
        {
            if (updateUserDto == null || userId != updateUserDto.Id)
            {
                _logger.LogWarning("UpdateUserAsync fue llamado con argumentos inválidos. UserID: {UserId}, DTO ID: {DtoId}", userId, updateUserDto?.Id);
                throw new ArgumentException("Los datos proporcionados para la actualización del usuario son inválidos.", nameof(updateUserDto));
            }

            if (!Guid.TryParse(updateUserDto.Id, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", updateUserDto.Id);
                throw new ArgumentException("El ID de usuario proporcionado no es válido.", nameof(updateUserDto.Id));
            }

            var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
            if (user == null)
            {
                _logger.LogWarning("No se encontró el usuario con ID {UserId} para actualizar.", updateUserDto.Id);
                throw new KeyNotFoundException($"No se encontró el usuario con ID {updateUserDto.Id}");
            }

            _mapper.Map(updateUserDto, user);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Error al actualizar el usuario {UserId}: {Errors}", user.Id, errors);
                throw new ApplicationException($"Error al actualizar el usuario: {errors}");
            }

            /*
            // Actualizar roles si se proporcionan
            if (updateUserDto.Roles != null && updateUserDto.Roles.Any()) // updateUserDto es el nombre correcto del parámetro
            { 
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(updateUserDto.Roles).ToList();
                var rolesToAdd = updateUserDto.Roles.Except(currentRoles).ToList();

                if (rolesToRemove.Any())
                { 
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    { 
                        // Log error pero no necesariamente fallar toda la operación por esto
                        _logger.LogWarning("UpdateUserAsync: Error al quitar roles para {UserId}: {Errors}", userId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    }
                }

                if (rolesToAdd.Any())
                { 
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded)
                    { 
                        _logger.LogWarning("UpdateUserAsync: Error al añadir roles para {UserId}: {Errors}", userId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    }
                }
            }
            */

            // Recargar el usuario para obtener el estado más reciente y evitar problemas de concurrencia o datos obsoletos
            var reloadedUser = await _userManager.FindByIdAsync(userId);
            if (reloadedUser == null)
            {
                _logger.LogError("CRÍTICO: No se pudo recargar el usuario {UserId} después de una actualización exitosa.", user.Id);
                throw new InvalidOperationException($"No se pudo recargar el usuario con ID {user.Id} después de la actualización.");
            }
            
            var responseDto = _mapper.Map<UserResponseDTO>(reloadedUser);
            if (responseDto == null)
            {
                 _logger.LogError("El mapeo del usuario recargado {UserId} a UserResponseDTO resultó en null.", reloadedUser.Id);
                 throw new InvalidOperationException($"Error al mapear el usuario recargado {reloadedUser.Id} a DTO.");
            }

            responseDto.Roles = (await _userManager.GetRolesAsync(reloadedUser)).ToList();
            
            return responseDto;
        }

        public async Task<bool> DeleteUserAsync(string userId, bool hardDelete = false)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return false;
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null) 
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId} para eliminar.", userId);
                    return false;
                }

                if (hardDelete)
                {
                    var result = await _userManager.DeleteAsync(user);
                    return result.Succeeded;
                }
                else
                {
                    user.IsDeleted = true;
                    user.DeletedAt = DateTime.UtcNow;
                    var result = await _userManager.UpdateAsync(user);
                    return result.Succeeded;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario con ID {UserId}", userId);
                throw;
            }
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return null;
                }

                var user = await _userManager.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userIdGuid);
                    
                if (user == null) 
                {
                    _logger.LogWarning("No se encontró el usuario con ID {UserId}", userId);
                    return null;
                }

                var userDto = _mapper.Map<UserResponseDTO>(user);
                
                // Obtener los roles del usuario
                var roles = await _userManager.GetRolesAsync(user);
                userDto.Roles = roles.ToList();
                
                // Mapear propiedades adicionales
                userDto.IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
                userDto.LockoutEnd = user.LockoutEnd;
                userDto.LockoutReason = user.LockoutReason;
                userDto.IsActive = user.IsActive;
                userDto.DateRegistered = user.CreatedAt;
                
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con ID {UserId}", userId);
                throw;
            }
        }

        public async Task<PagedResult<UserResponseDTO>> GetAllUsersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool activeOnly = false)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();

                // Aplicar filtros
                if (activeOnly)
                {
                    query = query.Where(u => !u.IsDeleted);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(u => 
                        (u.UserName != null && u.UserName.ToLower().Contains(searchTermLower)) ||
                        (u.Email != null && u.Email.ToLower().Contains(searchTermLower)) ||
                        (u.FirstName != null && u.FirstName.ToLower().Contains(searchTermLower)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(searchTermLower)));
                }

                // Obtener el total de registros
                var totalCount = await query.CountAsync();

                // Aplicar paginación
                var users = await query
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Mapear a DTOs y cargar roles
                var userDtos = new List<UserResponseDTO>();
                foreach (var user in users)
                {
                    var userDto = _mapper.Map<UserResponseDTO>(user);
                    
                    // Obtener los roles del usuario
                    var roles = await _userManager.GetRolesAsync(user);
                    userDto.Roles = roles.ToList();
                    
                    // Mapear propiedades adicionales
                    userDto.IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
                    userDto.LockoutEnd = user.LockoutEnd;
                    userDto.LockoutReason = user.LockoutReason;
                    userDto.IsActive = user.IsActive;
                    userDto.DateRegistered = user.CreatedAt;
                    
                    userDtos.Add(userDto);
                }


                return new PagedResult<UserResponseDTO>
                {
                    Items = userDtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios. Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);
                throw;
            }
        }

        public async Task<bool> AssignRolesToUserAsync(string userId, IEnumerable<string> roleNames)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return false;
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null) 
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId} para asignar roles.", userId);
                    return false;
                }

                var result = await _userManager.AddToRolesAsync(user, roleNames);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar roles al usuario con ID {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RemoveRolesFromUserAsync(string userId, IEnumerable<string> roleNames)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return false;
                }


                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null) 
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId} para eliminar roles.", userId);
                    return false;
                }

                var result = await _userManager.RemoveFromRolesAsync(user, roleNames);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar roles del usuario con ID {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario no es un GUID válido: {UserId}", userId);
                    return Enumerable.Empty<string>();
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID {UserId}", userId);
                    return Enumerable.Empty<string>();
                }

                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("Se obtuvieron {Count} roles para el usuario con ID {UserId}", roles.Count, userId);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los roles del usuario con ID {UserId}", userId);
                throw;
            }
        }
    }
}
