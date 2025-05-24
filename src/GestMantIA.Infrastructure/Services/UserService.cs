using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GestMantIA.Core.Identity.DTOs;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Shared;
using GestMantIA.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GestMantIA.Core.Identity.DTOs.Responses;

namespace GestMantIA.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de usuarios.
    /// </summary>
    public class UserService : IUserService, IDisposable
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserService"/>
        /// </summary>
        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper,
            ILogger<UserService> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<UserResponseDTO?> GetUserProfileAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogDebug("No se encontró el usuario con ID: {UserId}", userId);
                    return null;
                }


                var userDto = _mapper.Map<UserResponseDTO>(user);
                var roles = await _userManager.GetRolesAsync(user);
                userDto.Roles = roles.ToList();
                userDto.IsLockedOut = await _userManager.IsLockedOutAsync(user);
                if (userDto.IsLockedOut)
                {
                    userDto.LockoutEnd = user.LockoutEnd;
                    userDto.LockoutReason = user.LockoutReason;
                }

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario con ID: {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDTO profile)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId} para actualizar", userId);
                    return false;
                }

                // Actualizar propiedades
                user.FirstName = profile.FirstName ?? user.FirstName;
                user.LastName = profile.LastName ?? user.LastName;
                user.PhoneNumber = profile.PhoneNumber ?? user.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Error al actualizar el usuario: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perfil del usuario con ID: {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<PagedResult<UserResponseDTO>> SearchUsersAsync(
            string? searchTerm = null, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            try
            {
                var query = _userManager.Users.AsQueryable();

                // Aplicar filtro de búsqueda si se proporciona
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var normalizedSearchTerm = searchTerm.ToLower();
                    query = query.Where(u =>
                        (u.NormalizedUserName != null && u.NormalizedUserName.Contains(normalizedSearchTerm)) ||
                        (u.NormalizedEmail != null && u.NormalizedEmail.Contains(normalizedSearchTerm)) ||
                        (u.FirstName != null && u.FirstName.ToLower().Contains(normalizedSearchTerm)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(normalizedSearchTerm)));
                }

                // Obtener el total de registros
                var totalCount = await query.CountAsync();

                // Aplicar paginación
                var users = await query
                    .OrderBy(u => u.UserName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Mapear a DTOs y obtener roles
                var userDtos = new List<UserResponseDTO>();
                foreach (var user in users)
                {
                    var userDto = _mapper.Map<UserResponseDTO>(user);
                    var roles = await _userManager.GetRolesAsync(user);
                    userDto.Roles = roles.ToList();
                    userDto.IsLockedOut = await _userManager.IsLockedOutAsync(user);
                    if (userDto.IsLockedOut)
                    {
                        userDto.LockoutEnd = user.LockoutEnd;
                        userDto.LockoutReason = user.LockoutReason;
                    }
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
                _logger.LogError(ex, "Error al buscar usuarios con el término: {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> LockUserAsync(string userId, TimeSpan? duration = null, string? reason = null)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return false;
                }

                // Configurar la fecha de fin de bloqueo
                DateTimeOffset? lockoutEnd = null;
                if (duration.HasValue)
                {
                    lockoutEnd = DateTimeOffset.UtcNow.Add(duration.Value);
                }
                else
                {
                    // Bloqueo indefinido (hasta que se desbloquee manualmente)
                    lockoutEnd = DateTimeOffset.MaxValue;
                }

                // Establecer el bloqueo
                var result = await _userManager.SetLockoutEnabledAsync(user, true);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("No se pudo habilitar el bloqueo para el usuario {UserId}: {Errors}", 
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }

                result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("No se pudo establecer la fecha de fin de bloqueo para el usuario {UserId}: {Errors}", 
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }

                // Registrar el motivo del bloqueo (si se proporciona)
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    user.LockoutReason = reason;
                    user.LockoutDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                _logger.LogInformation("Usuario {UserId} bloqueado hasta {LockoutEnd}", userId, lockoutEnd);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al bloquear al usuario {UserId}", userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UnlockUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return false;
                }

                // Deshabilitar el bloqueo
                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("No se pudo desbloquear al usuario {UserId}: {Errors}", 
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }

                // Limpiar la razón del bloqueo
                user.LockoutReason = null;
                user.LockoutDate = null;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Usuario {UserId} desbloqueado exitosamente", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desbloquear al usuario {UserId}", userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> IsUserLockedOutAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return false;
                }

                return await _userManager.IsLockedOutAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el estado de bloqueo del usuario {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<UserLockoutInfo> GetUserLockoutInfoAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return null;
                }

                var lockoutInfo = new UserLockoutInfo
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    IsLockedOut = await _userManager.IsLockedOutAsync(user),
                    LockoutEnd = user.LockoutEnd,
                    LockoutStart = user.LockoutDate,
                    Reason = user.LockoutReason,
                    IsPermanent = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(100) // Consideramos bloqueo permanente si es más de 100 años
                };

                return lockoutInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la información de bloqueo del usuario {UserId}", userId);
                throw;
            }
        }

        #region Gestión de Usuarios

        /// <inheritdoc />
        public async Task<UserResponseDTO> CreateUserAsync(CreateUserDTO createUserDto, IEnumerable<string>? roleNames = null)
        {
            if (createUserDto == null)
                throw new ArgumentNullException(nameof(createUserDto));

            try
            {
                // Verificar si el nombre de usuario ya existe
                var existingUser = await _userManager.FindByNameAsync(createUserDto.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning("El nombre de usuario '{Username}' ya está en uso.", createUserDto.Username);
                    return null;
                }


                // Verificar si el correo ya está registrado
                existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("El correo electrónico '{Email}' ya está registrado.", createUserDto.Email);
                    return null;
                }

                // Crear el nuevo usuario
                var user = new ApplicationUser
                {
                    UserName = createUserDto.Username,
                    Email = createUserDto.Email,
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    PhoneNumber = createUserDto.PhoneNumber,
                    IsActive = true,
                    EmailConfirmed = !createUserDto.RequireEmailConfirmation,
                    CreatedAt = DateTime.UtcNow
                };

                // Crear el usuario
                var result = await _userManager.CreateAsync(user, createUserDto.Password);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("No se pudo crear el usuario. Errores: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }

                // Asignar roles si se especificaron
                if (roleNames != null && roleNames.Any())
                {
                    await AssignRolesToUserAsync(user.Id, roleNames);
                }


                _logger.LogInformation("Usuario creado exitosamente con ID: {UserId}", user.Id);
                return _mapper.Map<UserResponseDTO>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario con email: {Email}", createUserDto.Email);
                throw;
            }
        }


        /// <inheritdoc />
        public async Task<UserResponseDTO> UpdateUserAsync(UpdateUserDTO updateUserDto)
        {
            if (updateUserDto == null)
                throw new ArgumentNullException(nameof(updateUserDto));

            try
            {
                // Obtener el usuario existente
                var user = await _userManager.FindByIdAsync(updateUserDto.Id);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", updateUserDto.Id);
                    return null;
                }

                // Actualizar propiedades si se proporcionan
                if (!string.IsNullOrWhiteSpace(updateUserDto.Username) && user.UserName != updateUserDto.Username)
                {
                    var existingUser = await _userManager.FindByNameAsync(updateUserDto.Username);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        _logger.LogWarning("El nombre de usuario '{Username}' ya está en uso.", updateUserDto.Username);
                        return null;
                    }
                    user.UserName = updateUserDto.Username;
                }

                if (!string.IsNullOrWhiteSpace(updateUserDto.Email) && user.Email != updateUserDto.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(updateUserDto.Email);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        _logger.LogWarning("El correo electrónico '{Email}' ya está registrado.", updateUserDto.Email);
                        return null;
                    }
                    user.Email = updateUserDto.Email;
                }


                if (updateUserDto.FirstName != null) user.FirstName = updateUserDto.FirstName;
                if (updateUserDto.LastName != null) user.LastName = updateUserDto.LastName;
                if (updateUserDto.PhoneNumber != null) user.PhoneNumber = updateUserDto.PhoneNumber;
                if (updateUserDto.IsActive.HasValue) user.IsActive = updateUserDto.IsActive.Value;
                if (updateUserDto.EmailConfirmed.HasValue) user.EmailConfirmed = updateUserDto.EmailConfirmed.Value;
                if (updateUserDto.PhoneNumberConfirmed.HasValue) user.PhoneNumberConfirmed = updateUserDto.PhoneNumberConfirmed.Value;
                if (updateUserDto.TwoFactorEnabled.HasValue) user.TwoFactorEnabled = updateUserDto.TwoFactorEnabled.Value;


                user.UpdatedAt = DateTime.UtcNow;

                // Actualizar el usuario
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("No se pudo actualizar el usuario {UserId}. Errores: {Errors}", 
                        user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }

                _logger.LogInformation("Usuario actualizado exitosamente: {UserId}", user.Id);
                return _mapper.Map<UserResponseDTO>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con ID: {UserId}", updateUserDto.Id);
                throw;
            }
        }


        /// <inheritdoc />
        public async Task<bool> DeleteUserAsync(string userId, bool hardDelete = false)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return false;
                }


                if (hardDelete)
                {
                    // Eliminación física
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("No se pudo eliminar el usuario {UserId}. Errores: {Errors}", 
                            userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                        return false;
                    }
                    _logger.LogInformation("Usuario eliminado permanentemente: {UserId}", userId);
                }
                else
                {
                    // Eliminación lógica
                    user.IsActive = false;
                    user.UpdatedAt = DateTime.UtcNow;
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("No se pudo desactivar el usuario {UserId}. Errores: {Errors}", 
                            userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                        return false;
                    }
                    _logger.LogInformation("Usuario desactivado: {UserId}", userId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario con ID: {UserId}", userId);
                throw;
            }
        }


        /// <inheritdoc />
        public async Task<UserResponseDTO?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogDebug("No se encontró el usuario con ID: {UserId}", userId);
                    return null;
                }

                var userDto = _mapper.Map<UserResponseDTO>(user);
                
                // Obtener roles del usuario
                var roles = await _userManager.GetRolesAsync(user);
                userDto.Roles = roles.ToList();
                
                // Verificar si el usuario está bloqueado
                userDto.IsLockedOut = await _userManager.IsLockedOutAsync(user);
                if (userDto.IsLockedOut)
                {
                    userDto.LockoutEnd = user.LockoutEnd;
                    userDto.LockoutReason = user.LockoutReason;
                }

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con ID: {UserId}", userId);
                throw;
            }
        }


        /// <inheritdoc />
        public async Task<PagedResult<UserResponseDTO>> GetAllUsersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool? activeOnly = null)
        {
            try
            {
                // Validar parámetros
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // Obtener usuarios con paginación
                var users = _userManager.Users.AsQueryable();
                
                // Aplicar filtro de búsqueda si se proporciona
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var normalizedSearchTerm = searchTerm.ToLower();
                    users = users.Where(u => 
                        u.NormalizedUserName != null && u.NormalizedUserName.Contains(normalizedSearchTerm) ||
                        u.NormalizedEmail != null && u.NormalizedEmail.Contains(normalizedSearchTerm) ||
                        u.FirstName != null && u.FirstName.ToLower().Contains(normalizedSearchTerm) ||
                        u.LastName != null && u.LastName.ToLower().Contains(normalizedSearchTerm)
                    );
                }

                // Aplicar filtro de estado activo si se proporciona
                if (activeOnly.HasValue)
                {
                    users = users.Where(u => u.IsActive == activeOnly.Value);
                }

                // Contar el total de usuarios que coinciden con los filtros
                var totalCount = await users.CountAsync();

                // Aplicar paginación
                var pagedUsers = await users
                    .OrderBy(u => u.UserName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Mapear a DTOs
                var userDtos = new List<UserResponseDTO>();
                foreach (var user in pagedUsers)
                {
                    var userDto = _mapper.Map<UserResponseDTO>(user);
                    
                    // Obtener roles del usuario
                    var roles = await _userManager.GetRolesAsync(user);
                    userDto.Roles = roles.ToList();
                    
                    // Verificar si el usuario está bloqueado
                    userDto.IsLockedOut = await _userManager.IsLockedOutAsync(user);
                    if (userDto.IsLockedOut)
                    {
                        userDto.LockoutEnd = user.LockoutEnd;
                        userDto.LockoutReason = user.LockoutReason;
                    }
                    
                    userDtos.Add(userDto);
                }


                // Devolver resultado paginado
                var result = new PagedResult<UserResponseDTO>
                {
                    Items = userDtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de usuarios");
                throw;
            }
        }


        /// <inheritdoc />
        public async Task<bool> AssignRolesToUserAsync(string userId, IEnumerable<string> roleNames)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            if (roleNames == null)
                throw new ArgumentNullException(nameof(roleNames));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return false;
                }

                // Validar que los roles existan
                var validRoles = new List<string>();
                foreach (var roleName in roleNames.Distinct())
                {
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (roleExists)
                    {
                        validRoles.Add(roleName);
                    }
                    else
                    {
                        _logger.LogWarning("El rol '{RoleName}' no existe y no se asignará al usuario {UserId}", 
                            roleName, userId);
                    }
                }


                if (!validRoles.Any())
                {
                    _logger.LogWarning("No se proporcionaron roles válidos para asignar al usuario {UserId}", userId);
                    return false;
                }


                // Asignar roles al usuario
                var result = await _userManager.AddToRolesAsync(user, validRoles);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("No se pudieron asignar los roles al usuario {UserId}. Errores: {Errors}", 
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }

                _logger.LogInformation("Roles asignados correctamente al usuario {UserId}: {Roles}", 
                    userId, string.Join(", ", validRoles));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar roles al usuario {UserId}", userId);
                throw;
            }
        }


        /// <inheritdoc />
        public async Task<bool> RemoveRolesFromUserAsync(string userId, IEnumerable<string> roleNames)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            if (roleNames == null)
                throw new ArgumentNullException(nameof(roleNames));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return false;
                }

                // Validar que los roles existan
                var validRoles = new List<string>();
                foreach (var roleName in roleNames.Distinct())
                {
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (roleExists)
                    {
                        validRoles.Add(roleName);
                    }
                    else
                    {
                        _logger.LogWarning("El rol '{RoleName}' no existe y no se puede eliminar del usuario {UserId}", 
                            roleName, userId);
                    }
                }


                if (!validRoles.Any())
                {
                    _logger.LogWarning("No se proporcionaron roles válidos para eliminar del usuario {UserId}", userId);
                    return false;
                }


                // Eliminar roles del usuario
                var result = await _userManager.RemoveFromRolesAsync(user, validRoles);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("No se pudieron eliminar los roles del usuario {UserId}. Errores: {Errors}", 
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }

                _logger.LogInformation("Roles eliminados correctamente del usuario {UserId}: {Roles}", 
                    userId, string.Join(", ", validRoles));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar roles del usuario {UserId}", userId);
                throw;
            }
        }


        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return Enumerable.Empty<string>();
                }

                var roles = await _userManager.GetRolesAsync(user);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los roles del usuario {UserId}", userId);
                throw;
            }
        }

        #endregion

        /// <inheritdoc />
        public async Task<PagedResult<UserResponseDTO>> GetAllUsersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool? activeOnly = null)
        {
            try
            {
                // Validar parámetros
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // Obtener usuarios con paginación
                var users = _userManager.Users.AsQueryable();
                
                // Aplicar filtro de búsqueda si se proporciona
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var normalizedSearchTerm = searchTerm.ToLower();
                    users = users.Where(u => 
                        (u.NormalizedUserName != null && u.NormalizedUserName.Contains(normalizedSearchTerm)) ||
                        (u.NormalizedEmail != null && u.NormalizedEmail.Contains(normalizedSearchTerm)) ||
                        (u.FirstName != null && u.FirstName.ToLower().Contains(normalizedSearchTerm)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(normalizedSearchTerm)));
                }

                // Aplicar filtro de estado activo si se proporciona
                if (activeOnly.HasValue)
                {
                    users = users.Where(u => u.IsActive == activeOnly.Value);
                }

                // Contar el total de usuarios que coinciden con los filtros
                var totalCount = await users.CountAsync();

                // Aplicar paginación
                var pagedUsers = await users
                    .OrderBy(u => u.UserName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Mapear a DTOs
                var userDtos = _mapper.Map<List<UserResponseDTO>>(pagedUsers);

                // Obtener roles para cada usuario
                foreach (var userDto in userDtos)
                {
                    var user = pagedUsers.First(u => u.Id == userDto.Id);
                    var roles = await _userManager.GetRolesAsync(user);
                    userDto.Roles = roles.ToList();
                    
                    // Verificar si el usuario está bloqueado
                    userDto.IsLockedOut = await _userManager.IsLockedOutAsync(user);
                    if (userDto.IsLockedOut)
                    {
                        userDto.LockoutEnd = user.LockoutEnd;
                        userDto.LockoutReason = user.LockoutReason;
                    }
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
                _logger.LogError(ex, "Error al obtener la lista de usuarios. Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);
                throw;
            }
        }
        #endregion

        // Implementación explícita de IUserService.SearchUsersAsync
        async Task<PagedResult<UserResponseDTO>> IUserService.SearchUsersAsync(string? searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                // Validar parámetros de paginación
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // Construir la consulta base
                var query = _userManager.Users.AsQueryable();

                // Aplicar filtro de búsqueda si se proporciona
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.Trim().ToLower();
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
                    .OrderBy(u => u.UserName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Mapear a DTOs
                var userDtos = _mapper.Map<List<UserResponseDTO>>(users);

                // Obtener roles para cada usuario
                foreach (var userDto in userDtos)
                {
                    var user = users.First(u => u.Id == userDto.Id);
                    var roles = await _userManager.GetRolesAsync(user);
                    userDto.Roles = roles.ToList();
                    
                    // Verificar si el usuario está bloqueado
                    userDto.IsLockedOut = await _userManager.IsLockedOutAsync(user);
                    if (userDto.IsLockedOut)
                    {
                        userDto.LockoutEnd = user.LockoutEnd;
                        userDto.LockoutReason = user.LockoutReason;
                    }
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
                _logger.LogError(ex, "Error al buscar usuarios con el término: {SearchTerm}", searchTerm);
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
