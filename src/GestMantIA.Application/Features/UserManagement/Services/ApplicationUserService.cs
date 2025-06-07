using AutoMapper;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Shared; // For PagedResult
using GestMantIA.Shared.Identity.DTOs; // For general DTOs if not in subfolders
using GestMantIA.Shared.Identity.DTOs.Requests; // For CreateUserDTO, UpdateUserDTO etc.
using GestMantIA.Shared.Identity.DTOs.Responses; // For UserResponseDTO, RoleDto etc.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // For ToListAsync, CountAsync etc.
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GestMantIA.Application.Features.UserManagement.Services
{
    public class ApplicationUserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ApplicationUserService> _logger;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IHttpContextAccessor _httpContextAccessor; // TODO: Review usage, prefer explicit audit IDs

        public ApplicationUserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper,
            ILogger<ApplicationUserService> logger,
            IOptions<IdentityOptions> identityOptions,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _identityOptions = identityOptions ?? throw new ArgumentNullException(nameof(identityOptions));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<UserResponseDTO?> GetUserProfileAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' o está marcado como eliminado.", userId);
                    return null;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userProfileResponse = _mapper.Map<UserResponseDTO>(user) with
                {
                    Roles = userRoles.ToList()
                };

                return userProfileResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario con ID '{UserId}'.", userId);
                return null;
            }
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogInformation("GetUserByIdAsync: No se encontró el usuario con ID '{UserId}' o está marcado como eliminado.", userId);
                    return null;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userResponse = _mapper.Map<UserResponseDTO>(user) with
                {
                    Roles = userRoles.ToList()
                };

                return userResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserByIdAsync: Error al obtener el usuario con ID '{UserId}'.", userId);
                return null;
            }
        }

        public async Task<PagedResult<UserResponseDTO>> GetAllUsersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool activeOnly = true)
        {
            _logger.LogInformation("GetAllUsersAsync invocado con pageNumber: {PageNumber}, pageSize: {PageSize}, searchTerm: {SearchTerm}, activeOnly: {ActiveOnly}", 
                pageNumber, pageSize, searchTerm, activeOnly);

            try
            {
                var query = _userManager.Users.AsQueryable();

                // Filtrar usuarios eliminados lógicamente
                query = query.Where(u => !u.IsDeleted);

                // Filtrar por usuarios activos si es necesario
                if (activeOnly)
                {
                    query = query.Where(u => u.IsActive && !u.IsDeleted);
                }
                else
                {
                    // Si no es solo activos, solo excluir eliminados lógicamente
                    query = query.Where(u => !u.IsDeleted);
                }


                // Aplicar búsqueda si se proporciona un término de búsqueda
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(u => 
                        (u.UserName != null && u.UserName.ToLower().Contains(searchTermLower)) ||
                        (u.Email != null && u.Email.ToLower().Contains(searchTermLower)) ||
                        (u.FirstName != null && u.FirstName.ToLower().Contains(searchTermLower)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(searchTermLower)));
                }

                // Obtener el conteo total antes de la paginación
                var totalCount = await query.CountAsync();

                // Aplicar paginación
                var users = await query
                    .OrderBy(u => u.UserName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Mapear a DTOs
                var mappedUserResponses = _mapper.Map<List<UserResponseDTO>>(users);

                // Obtener roles para cada usuario
                for (int i = 0; i < users.Count; i++)
                {
                    var user = users[i];
                    var roles = await _userManager.GetRolesAsync(user);
                    mappedUserResponses[i] = mappedUserResponses[i] with { Roles = roles.ToArray() };
                }

                _logger.LogInformation("Se encontraron {Count} usuarios de un total de {TotalCount}", users.Count, totalCount);

                return new PagedResult<UserResponseDTO>
                {
                    Items = mappedUserResponses,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de usuarios");
                throw; // Relanzar la excepción para que el controlador la maneje
            }
        }

        public async Task<PagedResult<UserResponseDTO>> SearchUsersAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            _logger.LogInformation("SearchUsersAsync invocado con pageNumber: {PageNumber}, pageSize: {PageSize}, searchTerm: {SearchTerm}", pageNumber, pageSize, searchTerm);

            var query = _userManager.Users.Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(searchTermLower)) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchTermLower)) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(searchTermLower)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchTermLower)));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userResponses = _mapper.Map<List<UserResponseDTO>>(users);

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var roles = await _userManager.GetRolesAsync(user);
                userResponses[i] = userResponses[i] with { Roles = roles.ToList() }; // Fix for CS8852
            }

            return new PagedResult<UserResponseDTO>
            {
                Items = userResponses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Implementations for IUserService methods that were previously throwing NotImplementedException
        // These will now be aligned with IUserService signatures (using DTOs from GestMantIA.Shared.Identity.DTOs)

        public async Task<UserResponseDTO> CreateUserAsync(CreateUserDTO createUserDto)
        {
            if (createUserDto == null)
            {
                _logger.LogWarning("El DTO para crear usuario no puede ser nulo.");
                // Como el método ahora debe devolver UserResponseDTO (no nullable), 
                // debemos lanzar una excepción o devolver un objeto UserResponseDTO que indique error.
                // Por simplicidad para la compilación, lanzaremos una excepción.
                // En una implementación real, se podría devolver un DTO con un estado de error.
                throw new ArgumentNullException(nameof(createUserDto));
            }

            try
            {
                var existingUserByUserName = await _userManager.FindByNameAsync(createUserDto.UserName);
                if (existingUserByUserName != null)
                {
                    _logger.LogWarning("Intento de crear usuario con UserName '{UserName}' que ya existe.", createUserDto.UserName);
                    throw new InvalidOperationException($"El nombre de usuario '{createUserDto.UserName}' ya existe.");
                }

                var existingUserByEmail = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (existingUserByEmail != null)
                {
                    _logger.LogWarning("Intento de crear usuario con Email '{Email}' que ya existe.", createUserDto.Email);
                    throw new InvalidOperationException($"El email '{createUserDto.Email}' ya existe.");
                }

                var user = _mapper.Map<ApplicationUser>(createUserDto);
                user.EmailConfirmed = !createUserDto.RequireEmailConfirmation;
                user.IsActive = true; // Default to active
                user.CreatedAt = DateTime.UtcNow;
                // user.CreatedById cannot be set as it's not part of IUserService.CreateUserAsync signature

                var identityResult = await _userManager.CreateAsync(user, createUserDto.Password);

                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error al crear usuario '{UserName}': {Errors}", createUserDto.UserName, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException($"Error al crear usuario '{createUserDto.UserName}': {string.Join(", ", identityResult.Errors.Select(e => e.Description))}");
                }

                _logger.LogInformation("Usuario '{UserName}' creado con ID '{UserId}'.", user.UserName, user.Id);

                var assignedRoles = new List<string>();
                if (createUserDto.Roles != null && createUserDto.Roles.Any())
                {
                    foreach (var roleName in createUserDto.Roles)
                    {
                        if (await _roleManager.RoleExistsAsync(roleName))
                        {
                            var addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);
                            if (addToRoleResult.Succeeded)
                            {
                                assignedRoles.Add(roleName);
                                _logger.LogInformation("Rol '{RoleName}' asignado al usuario '{UserName}'.", roleName, user.UserName);
                            }
                            else
                            {
                                _logger.LogWarning("No se pudo asignar el rol '{RoleName}' al usuario '{UserName}': {Errors}", roleName, user.UserName, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                            }
                        }
                        else
                        {
                            _logger.LogWarning("El rol '{RoleName}' no existe y no se pudo asignar al usuario '{UserName}'.", roleName, user.UserName);
                        }
                    }
                }

                var userResponseDto = _mapper.Map<UserResponseDTO>(user);

                userResponseDto = userResponseDto with { Roles = assignedRoles };
                return userResponseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear el usuario '{UserName}'.", createUserDto.UserName);
                throw; // Re-lanza la excepción original para preservar el stack trace
            }
        }

        public async Task<UserResponseDTO> UpdateUserAsync(Guid userId, UpdateUserDTO userDto)
        {
            if (userDto == null)
            {
                _logger.LogWarning("El DTO para actualizar usuario no puede ser nulo.");
                throw new ArgumentNullException(nameof(userDto));
            }

            if (userDto.Id != userId)
            {
                _logger.LogWarning("El ID de usuario en la ruta ('{RouteUserId}') y en el DTO ('{DtoUserId}') no coinciden o son inválidos.", userId, userDto.Id);
                throw new ArgumentException("El ID de usuario en la ruta y en el DTO no coinciden o son inválidos.", nameof(userId));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' para actualizar o está marcado como eliminado.", userId);
                    throw new KeyNotFoundException($"Usuario con ID '{userId}' no encontrado o está marcado como eliminado.");
                }

                // Actualizar UserName si se proporciona y es diferente
                if (!string.IsNullOrWhiteSpace(userDto.UserName) && user.UserName != userDto.UserName)
                {
                    var existingUserByUserName = await _userManager.FindByNameAsync(userDto.UserName);
                    if (existingUserByUserName != null && existingUserByUserName.Id != user.Id)
                    {
                        _logger.LogWarning("Intento de actualizar UserName a '{UserName}' que ya está en uso por otro usuario.", userDto.UserName);
                        throw new InvalidOperationException($"El nombre de usuario '{userDto.UserName}' ya está en uso por otro usuario.");
                    }
                    user.UserName = userDto.UserName;
                }

                // Actualizar Email si se proporciona y es diferente
                if (!string.IsNullOrWhiteSpace(userDto.Email) && user.Email != userDto.Email)
                {
                    var existingUserByEmail = await _userManager.FindByEmailAsync(userDto.Email);
                    if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
                    {
                        _logger.LogWarning("Intento de actualizar Email a '{Email}' que ya está en uso por otro usuario.", userDto.Email);
                        throw new InvalidOperationException($"El email '{userDto.Email}' ya está en uso por otro usuario.");
                    }
                    user.Email = userDto.Email;
                    // Considerar: user.EmailConfirmed = false; // Si el email cambia, podría requerir re-confirmación
                }

                // Actualizar otras propiedades
                if (userDto.FirstName != null) user.FirstName = userDto.FirstName; 
                if (userDto.LastName != null) user.LastName = userDto.LastName;
                if (userDto.PhoneNumber != null) user.PhoneNumber = userDto.PhoneNumber;

                if (userDto.IsActive.HasValue) user.IsActive = userDto.IsActive.Value;
                if (userDto.EmailConfirmed.HasValue) user.EmailConfirmed = userDto.EmailConfirmed.Value;
                if (userDto.PhoneNumberConfirmed.HasValue) user.PhoneNumberConfirmed = userDto.PhoneNumberConfirmed.Value;
                if (userDto.TwoFactorEnabled.HasValue) user.TwoFactorEnabled = userDto.TwoFactorEnabled.Value;
                
                user.UpdatedAt = DateTime.UtcNow;
                // user.UpdatedById = _currentUserService.UserId; // Si se implementa un servicio de usuario actual

                var identityResult = await _userManager.UpdateAsync(user);

                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error al actualizar datos del usuario '{UserId}': {Errors}", userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException($"Error al actualizar datos del usuario '{userId}': {string.Join(", ", identityResult.Errors.Select(e => e.Description))}");
                }
                _logger.LogInformation("Datos del usuario '{UserId}' actualizados correctamente.", userId);

                // Actualizar roles si se proporcionan
                if (userDto.Roles != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToAdd = userDto.Roles.Except(currentRoles).ToList();
                    var rolesToRemove = currentRoles.Except(userDto.Roles).ToList();

                    if (rolesToAdd.Any())
                    {
                        var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                        if (!addRolesResult.Succeeded)
                        {
                            _logger.LogWarning("Error al añadir roles al usuario '{UserId}': {Errors}", userId, string.Join(", ", addRolesResult.Errors.Select(e => e.Description)));
                        }
                    }

                    if (rolesToRemove.Any())
                    {
                        var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                        if (!removeRolesResult.Succeeded)
                        {
                            _logger.LogWarning("Error al remover roles del usuario '{UserId}': {Errors}", userId, string.Join(", ", removeRolesResult.Errors.Select(e => e.Description)));
                        }
                    }
                }

                // Manejar SetLockoutStatus
                if (userDto.SetLockoutStatus.HasValue)
                {
                    if (userDto.SetLockoutStatus.Value) // True significa Bloquear
                    {
                        var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                        if (!lockoutResult.Succeeded)
                        {
                            _logger.LogWarning("Error al bloquear al usuario '{UserId}': {Errors}", userId, string.Join(", ", lockoutResult.Errors.Select(e => e.Description)));
                        }
                        else
                        {
                            _logger.LogInformation("Usuario '{UserId}' bloqueado.", userId);
                        }
                    }
                    else // False significa Desbloquear
                    {
                        var unlockResult = await _userManager.SetLockoutEndDateAsync(user, null); 
                        if (!unlockResult.Succeeded)
                        {
                            _logger.LogWarning("Error al desbloquear al usuario '{UserId}': {Errors}", userId, string.Join(", ", unlockResult.Errors.Select(e => e.Description)));
                        }
                        else
                        {
                            _logger.LogInformation("Usuario '{UserId}' desbloqueado.", userId);
                        }
                    }
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userResponseDto = _mapper.Map<UserResponseDTO>(user) with { Roles = roles.ToList() };

                return userResponseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar el usuario '{UserId}'.", userId);
                throw; 
            }
        }

        public async Task<bool> DeleteUserAsync(Guid userId, bool hardDelete = false)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' para eliminar.", userId);
                    return false;
                }

                IdentityResult identityResult;
                if (hardDelete)
                {
                    _logger.LogInformation("Iniciando eliminación física (hard delete) para el usuario '{UserId}'.", userId);
                    identityResult = await _userManager.DeleteAsync(user);
                }
                else
                {
                    if (user.IsDeleted)
                    {
                        _logger.LogInformation("El usuario '{UserId}' ya está marcado como eliminado (soft delete).", userId);
                        return true; // Considerar esto como éxito ya que el estado deseado ya está aplicado.
                    }
                    _logger.LogInformation("Iniciando eliminación lógica (soft delete) para el usuario '{UserId}'.", userId);
                    user.IsDeleted = true;
                    user.DeletedAt = DateTime.UtcNow;
                    // user.DeletedById no se puede establecer ya que no es parte de la firma de IUserService.
                    user.LockoutEnabled = false; // Prevenir login si está soft-deleted
                    user.SecurityStamp = Guid.NewGuid().ToString(); // Invalidar tokens existentes
                    identityResult = await _userManager.UpdateAsync(user);
                }

                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error al eliminar usuario '{UserId}' (HardDelete={HardDelete}): {Errors}", userId, hardDelete, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    return false;
                }

                _logger.LogInformation("Usuario '{UserId}' eliminado correctamente (HardDelete={HardDelete}).", userId, hardDelete);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar el usuario '{UserId}' (HardDelete={HardDelete}).", userId, hardDelete);
                return false;
            }
        }

        public async Task<bool> UpdateUserRolesAsync(Guid userId, IEnumerable<string> roleNames)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' para actualizar roles.", userId);
                    return false;
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToAdd = roleNames.Except(currentRoles).ToList();
                var rolesToRemove = currentRoles.Except(roleNames).ToList();

                bool success = true;

                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (addResult.Succeeded)
                    {
                        _logger.LogInformation("Roles {Roles} añadidos al usuario '{UserId}'.", string.Join(", ", rolesToAdd), userId);
                    }
                    else
                    {
                        _logger.LogWarning("Error al añadir roles {Roles} al usuario '{UserId}': {Errors}", string.Join(", ", rolesToAdd), userId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                        success = false;
                    }
                }

                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (removeResult.Succeeded)
                    {
                        _logger.LogInformation("Roles {Roles} eliminados del usuario '{UserId}'.", string.Join(", ", rolesToRemove), userId);
                    }
                    else
                    {
                        _logger.LogWarning("Error al eliminar roles {Roles} del usuario '{UserId}': {Errors}", string.Join(", ", rolesToRemove), userId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        success = false;
                    }
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar los roles del usuario '{UserId}'. Roles deseados: {Roles}", userId, string.Join(", ", roleNames));
                return false;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(Guid userId, GestMantIA.Shared.Identity.DTOs.Requests.UpdateProfileDTO profile)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' para actualizar el perfil.", userId);
                    return false;
                }

                bool updated = false;
                if (profile.FirstName != null)
                {
                    user.FirstName = profile.FirstName;
                    updated = true;
                }

                if (profile.LastName != null)
                {
                    user.LastName = profile.LastName;
                    updated = true;
                }

                if (profile.PhoneNumber != null) // Se permite string vacío para borrarlo, si se desea.
                {
                    user.PhoneNumber = profile.PhoneNumber;
                    updated = true;
                }

                if (!updated)
                {
                    _logger.LogInformation("No se proporcionaron datos para actualizar en el perfil del usuario '{UserId}'.", userId);
                    return true; // O false si se considera un fallo no actualizar nada.
                }

                user.UpdatedAt = DateTime.UtcNow;
                var identityResult = await _userManager.UpdateAsync(user);

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation("Perfil de usuario '{UserId}' actualizado exitosamente.", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al actualizar el perfil del usuario '{UserId}': {Errors}", userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar el perfil del usuario '{UserId}'.", userId);
                return false;
            }
        }

        public async Task<bool> LockUserAsync(Guid userId, TimeSpan? duration = null, string? reason = null)
        {
            ApplicationUser? user;
            try
            {
                user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo con ID '{UserId}' para bloquear.", userId);
                    return false;
                }

                if (await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogInformation("El usuario '{UserId}' ya se encuentra bloqueado. Fecha de finalización del bloqueo: {LockoutEnd}", userId, user.LockoutEnd);
                    return true; // Ya está en el estado deseado
                }

                DateTimeOffset lockoutEndDto;
                if (duration.HasValue)
                {
                    lockoutEndDto = DateTimeOffset.UtcNow.Add(duration.Value);
                }
                else
                {
                    lockoutEndDto = DateTimeOffset.UtcNow.Add(_userManager.Options.Lockout.DefaultLockoutTimeSpan);
                }

                var identityResult = await _userManager.SetLockoutEndDateAsync(user, lockoutEndDto);

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation("Usuario '{UserId}' bloqueado exitosamente hasta {LockoutEnd}. Razón: {Reason}",
                        userId, lockoutEndDto, string.IsNullOrWhiteSpace(reason) ? "No especificada" : reason);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al bloquear al usuario '{UserId}': {Errors}", userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al bloquear al usuario '{UserId}'.", userId);
                return false;
            }
        }

        public async Task<bool> UnlockUserAsync(Guid userId)
        {
            ApplicationUser? user;
            try
            {
                user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo con ID '{UserId}' para desbloquear.", userId);
                    return false;
                }

                if (!await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogInformation("El usuario '{UserId}' no se encuentra actualmente bloqueado.", userId);
                    return true; // Ya está en el estado deseado
                }

                var identityResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation("Usuario '{UserId}' desbloqueado exitosamente.", userId);
                    await _userManager.ResetAccessFailedCountAsync(user);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al desbloquear al usuario '{UserId}': {Errors}", userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al desbloquear al usuario '{UserId}'.", userId);
                return false;
            }
        }

        public async Task<bool> IsUserLockedOutAsync(Guid userId)
        {
            ApplicationUser? user;
            try
            {
                user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.IsDeleted)
                {
                    _logger.LogInformation("No se encontró un usuario activo con ID '{UserId}' para verificar si está bloqueado.", userId);
                    return false;
                }

                return await _userManager.IsLockedOutAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al verificar si el usuario '{UserId}' está bloqueado.", userId);
                return false;
            }
        }

        public async Task<UserLockoutInfo?> GetUserLockoutInfoAsync(Guid userId)
        {
            ApplicationUser? user;
            try
            {
                user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.IsDeleted)
                {
                    _logger.LogInformation("No se encontró un usuario activo con ID '{UserId}' para obtener información de bloqueo.", userId);
                    return null;
                }

                var isLockedOut = await _userManager.IsLockedOutAsync(user);
                var lockoutEndDateOffset = await _userManager.GetLockoutEndDateAsync(user);

                return new UserLockoutInfo
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    IsLockedOut = isLockedOut,
                    LockoutEnd = lockoutEndDateOffset?.UtcDateTime,
                    IsPermanent = lockoutEndDateOffset == DateTimeOffset.MaxValue,
                    Reason = user.LockoutReason, 
                    LockoutStart = user.LockoutDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener la información de bloqueo para el usuario '{UserId}'.", userId);
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            ApplicationUser? user;
            try
            {
                user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' para obtener roles.", userId);
                    return Enumerable.Empty<string>();
                }
                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener los roles del usuario '{UserId}'.", userId);
                return Enumerable.Empty<string>();
            }
        }

        #region Password and Email Confirmation

        private async Task<ApplicationUser?> FindUserByIdOrEmailOrUsernameAsync(String userIdOrEmailOrUsername)
        {
            if (string.IsNullOrWhiteSpace(userIdOrEmailOrUsername))
            {
                return null;
            }

            ApplicationUser? user = null;
            // Try to parse as Guid for ID
            if (Guid.TryParse(userIdOrEmailOrUsername, out Guid userIdGuid))
            {
                user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user != null) return user;
            }

            // Try to find by email
            if (userIdOrEmailOrUsername.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(userIdOrEmailOrUsername);
                if (user != null) return user;
            }

            // Try to find by username
            user = await _userManager.FindByNameAsync(userIdOrEmailOrUsername);
            return user;
        }

        public async Task<string?> GetPasswordResetTokenAsync(string userIdOrEmail)
        {
            try
            {
                var user = await FindUserByIdOrEmailOrUsernameAsync(userIdOrEmail);
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo con ID o email '{UserIdOrEmail}' para generar token de reseteo de contraseña.", userIdOrEmail);
                    return null;
                }
                // Ensure email is confirmed if required by policy, though typically not for password reset
                // if (!await _userManager.IsEmailConfirmedAsync(user))
                // {
                // _logger.LogWarning("El email del usuario '{UserIdOrEmail}' no está confirmado.", userIdOrEmail);
                // return null; 
                // }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                _logger.LogInformation("Token de reseteo de contraseña generado para el usuario '{UserIdOrEmail}'.", userIdOrEmail);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al generar token de reseteo de contraseña para '{UserIdOrEmail}'.", userIdOrEmail);
                return null;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto)
        {
            if (resetPasswordDto == null)
            {
                _logger.LogWarning("El DTO para resetear contraseña no puede ser nulo.");
                return false;
            }

            try
            {
                var user = await FindUserByIdOrEmailOrUsernameAsync(resetPasswordDto.EmailOrUserId);
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo con ID o email '{EmailOrUserId}' para resetear contraseña.", resetPasswordDto.EmailOrUserId);
                    return false;
                }

                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Contraseña reseteada exitosamente para el usuario '{EmailOrUserId}'.", resetPasswordDto.EmailOrUserId);
                    if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                    {
                         await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow); // Unlock user if locked
                        _logger.LogInformation("Usuario '{EmailOrUserId}' desbloqueado después del reseteo de contraseña.", resetPasswordDto.EmailOrUserId);
                    }
                    return true;
                }
                else
                {
                    _logger.LogError("Error al resetear la contraseña para el usuario '{EmailOrUserId}': {Errors}", resetPasswordDto.EmailOrUserId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al resetear la contraseña para '{EmailOrUserId}'.", resetPasswordDto.EmailOrUserId);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDTO changePasswordDto)
        {
             if (userId == Guid.Empty || changePasswordDto == null)
            {
                _logger.LogWarning("ID de usuario o DTO de cambio de contraseña inválidos.");
                return false;
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo con ID '{UserId}' para cambiar contraseña.", userId);
                    return false;
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Contraseña cambiada exitosamente para el usuario '{UserId}'.", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al cambiar la contraseña para el usuario '{UserId}': {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cambiar la contraseña para el usuario '{UserId}'.", userId);
                return false;
            }
        }

        public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
        {
            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("ID de usuario o token de confirmación de email inválidos.");
                return false;
            }
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' para confirmar email.", userId);
                    return false;
                }
                // Note: user.IsDeleted check might be relevant if soft-deleted users cannot confirm email.

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Email confirmado exitosamente para el usuario '{UserId}'.", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al confirmar el email para el usuario '{UserId}': {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al confirmar el email para el usuario '{UserId}'.", userId);
                return false;
            }
        }

        public async Task<string?> ResendConfirmationEmailAsync(String userIdOrEmail)
        {
            try
            {
                var user = await FindUserByIdOrEmailOrUsernameAsync(userIdOrEmail);
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo con ID o email '{UserIdOrEmail}' para reenviar email de confirmación.", userIdOrEmail);
                    return null;
                }

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    _logger.LogInformation("El email del usuario '{UserIdOrEmail}' ya está confirmado. No se reenviará token.", userIdOrEmail);
                    return null; // Or a specific message indicating already confirmed
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _logger.LogInformation("Nuevo token de confirmación de email generado para el usuario '{UserIdOrEmail}'.", userIdOrEmail);
                // Here you would typically use an IEmailSender service to send the email with the token.
                // For example: await _emailSender.SendEmailConfirmationAsync(user.Email, token, user.Id.ToString());
                // Since IEmailSender is not injected here, this method will just return the token.
                // The caller (e.g., API controller) would be responsible for sending the email.
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al reenviar el email de confirmación para '{UserIdOrEmail}'.", userIdOrEmail);
                return null;
            }
        }

        #endregion
    }
}
