using AutoMapper;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Shared; // For PagedResult
using GestMantIA.Shared.Identity.DTOs; // For general DTOs if not in subfolders
using GestMantIA.Shared.Identity.DTOs.Requests; // For CreateUserDTO, UpdateUserDTO etc.
using GestMantIA.Shared.Identity.DTOs.Responses; // For UserResponseDTO, RoleDTO etc.
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

        public async Task<UserResponseDTO?> GetUserProfileAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido.", userId);
                    return null;
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' o está marcado como eliminado.", userId);
                    return null;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userProfileResponse = _mapper.Map<UserResponseDTO>(user);
                userProfileResponse.Roles = userRoles.ToList();

                return userProfileResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario con ID '{UserId}'.", userId);
                return null;
            }
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                {
                    _logger.LogWarning("GetUserByIdAsync: El ID de usuario '{UserId}' no es un GUID válido.", userId);
                    return null;
                }

                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogInformation("GetUserByIdAsync: No se encontró el usuario con ID '{UserId}' o está marcado como eliminado.", userId);
                    return null;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userResponse = _mapper.Map<UserResponseDTO>(user);
                userResponse.Roles = userRoles.ToList();

                return userResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserByIdAsync: Error al obtener el usuario con ID '{UserId}'.", userId);
                return null;
            }
        }

        public async Task<PagedResult<UserResponseDTO>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool activeOnly = false)
        {
            // Esta implementación es similar a SearchUsersAsync pero podría tener lógicas específicas en el futuro.
            // Por ahora, podemos reutilizar SearchUsersAsync o implementar una lógica más simple si es necesario.
            // Para TDD, empezamos con una implementación que compile:
            _logger.LogInformation("GetAllUsersAsync invocado con pageNumber: {PageNumber}, pageSize: {PageSize}, searchTerm: {SearchTerm}, activeOnly: {ActiveOnly}", pageNumber, pageSize, searchTerm, activeOnly);

            var query = _userManager.Users.AsQueryable();

            if (activeOnly)
            {
                query = query.Where(u => u.EmailConfirmed && !u.LockoutEnabled); // Asumiendo que 'activo' significa email confirmado y no bloqueado
            }
            else
            {
                query = query.Where(u => !u.IsDeleted); // Comportamiento por defecto si no se filtra por activos
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

            var totalCount = await query.CountAsync();
            var users = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var userResponses = _mapper.Map<List<UserResponseDTO>>(users);

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var roles = await _userManager.GetRolesAsync(user);
                userResponses[i].Roles = roles.ToList();
            }

            return new PagedResult<UserResponseDTO>
            {
                Items = userResponses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<UserResponseDTO>> SearchUsersAsync(string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
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

                // TODO: Implementar ordenación si es necesario y se añade a IUserService

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
                    userResponses[i].Roles = roles.ToList();
                }

                return new PagedResult<UserResponseDTO>
                {
                    Items = userResponses,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuarios con término '{SearchTerm}'.", searchTerm);
                return new PagedResult<UserResponseDTO> // Return empty paged result on error
                {
                    Items = new List<UserResponseDTO>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        // Implementations for IUserService methods that were previously throwing NotImplementedException
        // These will now be aligned with IUserService signatures (using DTOs from GestMantIA.Shared.Identity.DTOs)

        public async Task<UserResponseDTO> CreateUserAsync(CreateUserDTO createUserDto, IEnumerable<string>? roleNames = null)
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
                if (roleNames != null && roleNames.Any())
                {
                    foreach (var roleName in roleNames)
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
                userResponseDto.Roles = assignedRoles;

                return userResponseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear el usuario '{UserName}'.", createUserDto.UserName);
                throw; // Re-lanza la excepción original para preservar el stack trace
            }
        }

        public async Task<UserResponseDTO> UpdateUserAsync(string userId, UpdateUserDTO userDto)
        {
            if (userDto == null)
            {
                _logger.LogWarning("El DTO para actualizar usuario no puede ser nulo.");
                throw new ArgumentNullException(nameof(userDto));
            }

            if (string.IsNullOrWhiteSpace(userId) || userDto.Id != userId)
            {
                _logger.LogWarning("El ID de usuario en la ruta ('{RouteUserId}') y en el DTO ('{DtoUserId}') no coinciden o son inválidos.", userId, userDto.Id);
                throw new ArgumentException("El ID de usuario en la ruta y en el DTO no coinciden o son inválidos.", nameof(userId));
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido.", userId);
                throw new ArgumentException($"El ID de usuario '{userId}' no es un GUID válido.", nameof(userId));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' para actualizar o está marcado como eliminado.", userId);
                    throw new KeyNotFoundException($"No se encontró el usuario con ID '{userId}' para actualizar o está marcado como eliminado.");
                }

                // Actualizar UserName si se proporciona y es diferente
                if (!string.IsNullOrWhiteSpace(userDto.UserName) && user.UserName != userDto.UserName)
                {
                    var existingUserByUserName = await _userManager.FindByNameAsync(userDto.UserName);
                    if (existingUserByUserName != null && existingUserByUserName.Id != user.Id)
                    {
                        _logger.LogWarning("Intento de actualizar UserName a '{UserName}' que ya está en uso por otro usuario.", userDto.UserName);
                        throw new InvalidOperationException($"Intento de actualizar UserName a '{userDto.UserName}' que ya está en uso por otro usuario.");
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
                        throw new InvalidOperationException($"Intento de actualizar Email a '{userDto.Email}' que ya está en uso por otro usuario.");
                    }
                    user.Email = userDto.Email;
                }

                // Actualizar otras propiedades
                if (!string.IsNullOrWhiteSpace(userDto.FirstName)) user.FirstName = userDto.FirstName;
                if (!string.IsNullOrWhiteSpace(userDto.LastName)) user.LastName = userDto.LastName;
                if (!string.IsNullOrWhiteSpace(userDto.PhoneNumber)) user.PhoneNumber = userDto.PhoneNumber;

                if (userDto.IsActive.HasValue) user.IsActive = userDto.IsActive.Value;
                if (userDto.EmailConfirmed.HasValue) user.EmailConfirmed = userDto.EmailConfirmed.Value;
                if (userDto.PhoneNumberConfirmed.HasValue) user.PhoneNumberConfirmed = userDto.PhoneNumberConfirmed.Value;
                if (userDto.TwoFactorEnabled.HasValue) user.TwoFactorEnabled = userDto.TwoFactorEnabled.Value;

                user.UpdatedAt = DateTime.UtcNow;
                // user.UpdatedById cannot be set as it's not part of IUserService.UpdateUserAsync signature

                var identityResult = await _userManager.UpdateAsync(user);

                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error al actualizar usuario '{UserId}': {Errors}", userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException($"Error al actualizar usuario '{userId}': {string.Join(", ", identityResult.Errors.Select(e => e.Description))}");
                }

                _logger.LogInformation("Usuario '{UserId}' actualizado correctamente.", userId);

                var userResponseDto = _mapper.Map<UserResponseDTO>(user);
                var roles = await _userManager.GetRolesAsync(user);
                userResponseDto.Roles = roles.ToList();

                return userResponseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar el usuario '{UserId}'.", userId);
                throw; // Re-lanza la excepción original para preservar el stack trace
            }
        }

        public async Task<bool> DeleteUserAsync(string userId, bool hardDelete = false)
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para eliminar.", userId);
                return false;
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
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



        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para obtener roles.", userId);
                return Enumerable.Empty<string>();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' o está eliminado, no se pueden obtener roles.", userId);
                    return Enumerable.Empty<string>();
                }

                var roles = await _userManager.GetRolesAsync(user);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener los roles del usuario '{UserId}'.", userId);
                return Enumerable.Empty<string>();
            }
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                _logger.LogWarning("El nombre del rol no puede ser nulo o vacío para asignar al usuario '{UserId}'.", userId);
                return false;
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para asignar rol.", userId);
                return false;
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' o está eliminado, no se puede asignar el rol '{RoleName}'.", userId, roleName);
                    return false;
                }

                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogWarning("El rol '{RoleName}' no existe. No se puede asignar al usuario '{UserId}'.", roleName, userId);
                    return false;
                }

                if (await _userManager.IsInRoleAsync(user, roleName))
                {
                    _logger.LogInformation("El usuario '{UserId}' ya pertenece al rol '{RoleName}'.", userId, roleName);
                    return true; // Considerar éxito si ya está en el rol
                }

                var identityResult = await _userManager.AddToRoleAsync(user, roleName);

                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error al asignar el rol '{RoleName}' al usuario '{UserId}': {Errors}", roleName, userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    return false;
                }

                _logger.LogInformation("Rol '{RoleName}' asignado correctamente al usuario '{UserId}'.", roleName, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al asignar el rol '{RoleName}' al usuario '{UserId}'.", roleName, userId);
                return false;
            }
        }

        public async Task<bool> AddUserToRolesAsync(string userId, IEnumerable<string> roleNames)
        {
            if (roleNames == null)
            {
                _logger.LogWarning("La lista de nombres de roles no puede ser nula para asignar al usuario '{UserId}'.", userId);
                return false;
            }

            var validRoleNames = roleNames.Where(rn => !string.IsNullOrWhiteSpace(rn)).Distinct().ToList();
            if (!validRoleNames.Any())
            {
                _logger.LogInformation("No se proporcionaron nombres de roles válidos para asignar al usuario '{UserId}'.", userId);
                return true; // No hay roles válidos para agregar, considerar éxito.
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para asignar roles.", userId);
                return false;
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' o está eliminado, no se pueden asignar roles.", userId);
                    return false;
                }

                var rolesToAdd = new List<string>();
                foreach (var roleName in validRoleNames)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        _logger.LogWarning("El rol '{RoleName}' no existe y no se puede asignar al usuario '{UserId}'.", roleName, userId);
                        continue; // Saltar este rol, pero no fallar toda la operación aún.
                    }
                    if (await _userManager.IsInRoleAsync(user, roleName))
                    {
                        _logger.LogInformation("El usuario '{UserId}' ya pertenece al rol '{RoleName}'. Se omitirá la asignación.", userId, roleName);
                        continue;
                    }
                    rolesToAdd.Add(roleName);
                }

                if (!rolesToAdd.Any())
                {
                    _logger.LogInformation("No hay nuevos roles válidos para asignar al usuario '{UserId}' (ya los posee o los roles proporcionados no existen).", userId);
                    return true; // Todos los roles válidos ya estaban asignados o no existían.
                }

                var identityResult = await _userManager.AddToRolesAsync(user, rolesToAdd);

                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error al asignar uno o más roles al usuario '{UserId}': {Errors}. Roles intentados: {Roles}",
                        userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)), string.Join(", ", rolesToAdd));
                    return false;
                }

                _logger.LogInformation("Roles '{Roles}' asignados correctamente al usuario '{UserId}'.", string.Join(", ", rolesToAdd), userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al asignar roles al usuario '{UserId}'. Roles intentados: {Roles}", userId, string.Join(", ", validRoleNames));
                return false;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                _logger.LogWarning("El nombre del rol no puede ser nulo o vacío para remover del usuario '{UserId}'.", userId);
                return false;
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para remover rol.", userId);
                return false;
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null || user.IsDeleted) // No se puede remover rol de usuario inexistente o eliminado
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' o está eliminado, no se puede remover el rol '{RoleName}'.", userId, roleName);
                    return false;
                }

                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogWarning("El rol '{RoleName}' no existe. No se puede remover del usuario '{UserId}' ya que no podría tenerlo.", roleName, userId);
                    return true; // Si el rol no existe, el usuario no puede estar en él. Considerar éxito.
                }

                if (!await _userManager.IsInRoleAsync(user, roleName))
                {
                    _logger.LogInformation("El usuario '{UserId}' no pertenece al rol '{RoleName}'. No es necesario removerlo.", userId, roleName);
                    return true; // Considerar éxito si ya no está en el rol
                }

                var identityResult = await _userManager.RemoveFromRoleAsync(user, roleName);

                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error al remover el rol '{RoleName}' del usuario '{UserId}': {Errors}", roleName, userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    return false;
                }

                _logger.LogInformation("Rol '{RoleName}' removido correctamente del usuario '{UserId}'.", roleName, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al remover el rol '{RoleName}' del usuario '{UserId}'.", roleName, userId);
                return false;
            }
        }

        public async Task<bool> RemoveUserFromRolesAsync(string userId, IEnumerable<string> roleNames)
        {
            if (roleNames == null)
            {
                _logger.LogWarning("La lista de nombres de roles no puede ser nula para remover del usuario '{UserId}'.", userId);
                return false;
            }

            var validRoleNames = roleNames.Where(rn => !string.IsNullOrWhiteSpace(rn)).Distinct().ToList();
            if (!validRoleNames.Any())
            {
                _logger.LogInformation("No se proporcionaron nombres de roles válidos para remover del usuario '{UserId}'.", userId);
                return true; // No hay roles válidos para remover, considerar éxito.
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para remover roles.", userId);
                return false;
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' o está eliminado, no se pueden remover roles.", userId);
                    return false;
                }

                var rolesToRemove = new List<string>();
                foreach (var roleName in validRoleNames)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        _logger.LogWarning("El rol '{RoleName}' no existe. Se omitirá de la lista de roles a remover del usuario '{UserId}'.", roleName, userId);
                        continue;
                    }
                    if (!await _userManager.IsInRoleAsync(user, roleName))
                    {
                        _logger.LogInformation("El usuario '{UserId}' no pertenece al rol '{RoleName}'. Se omitirá de la lista de roles a remover.", userId, roleName);
                        continue;
                    }
                    rolesToRemove.Add(roleName);
                }

                if (!rolesToRemove.Any())
                {
                    _logger.LogInformation("No hay roles válidos para remover del usuario '{UserId}' (no los posee o los roles proporcionados no existen).", userId);
                    return true; // Ninguno de los roles válidos estaba asignado o existía.
                }

                var identityResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error al remover uno o más roles del usuario '{UserId}': {Errors}. Roles intentados: {Roles}",
                        userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)), string.Join(", ", rolesToRemove));
                    return false;
                }

                _logger.LogInformation("Roles '{Roles}' removidos correctamente del usuario '{UserId}'.", string.Join(", ", rolesToRemove), userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al remover roles del usuario '{UserId}'. Roles intentados: {Roles}", userId, string.Join(", ", validRoleNames));
                return false;
            }
        }

        public async Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<string> roleNames)
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para actualizar roles.", userId);
                return false;
            }

            // Tratar null roleNames como una lista vacía, luego filtrar y obtener distintos.
            var desiredRoles = (roleNames ?? Enumerable.Empty<string>())
                               .Where(rn => !string.IsNullOrWhiteSpace(rn))
                               .Distinct()
                               .ToList();
            try
            {
                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró el usuario con ID '{UserId}' o está eliminado, no se pueden actualizar roles.", userId);
                    return false;
                }

                var currentRoles = (await _userManager.GetRolesAsync(user)).ToList();

                var rolesToRemove = currentRoles.Except(desiredRoles).ToList();
                var rolesToAddCandidate = desiredRoles.Except(currentRoles).ToList();

                var rolesToAddValidated = new List<string>();
                if (rolesToAddCandidate.Any())
                {
                    foreach (var roleName in rolesToAddCandidate)
                    {
                        if (await _roleManager.RoleExistsAsync(roleName))
                        {
                            rolesToAddValidated.Add(roleName);
                        }
                        else
                        {
                            _logger.LogWarning("El rol '{RoleName}' no existe y no se puede asignar al usuario '{UserId}' durante la actualización de roles.", roleName, userId);
                        }
                    }
                }

                bool success = true;

                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        _logger.LogError("Error al remover roles '{RolesToRemove}' del usuario '{UserId}' durante la actualización: {Errors}",
                            string.Join(", ", rolesToRemove), userId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        success = false; // Marcar como fallo, pero intentar agregar si es posible/necesario
                    }
                    else
                    {
                        _logger.LogInformation("Roles '{RolesToRemove}' removidos del usuario '{UserId}' durante la actualización.", string.Join(", ", rolesToRemove), userId);
                    }
                }

                if (success && rolesToAddValidated.Any()) // Solo intentar agregar si la eliminación fue exitosa (o no necesaria) y hay roles válidos para agregar
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAddValidated);
                    if (!addResult.Succeeded)
                    {
                        _logger.LogError("Error al agregar roles '{RolesToAdd}' al usuario '{UserId}' durante la actualización: {Errors}",
                            string.Join(", ", rolesToAddValidated), userId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                        success = false;
                    }
                    else
                    {
                        _logger.LogInformation("Roles '{RolesToAdd}' agregados al usuario '{UserId}' durante la actualización.", string.Join(", ", rolesToAddValidated), userId);
                    }
                }
                else if (!rolesToAddValidated.Any() && rolesToAddCandidate.Any())
                { // Hubo roles para agregar pero ninguno era válido
                    _logger.LogInformation("No se agregaron nuevos roles al usuario '{UserId}' durante la actualización, ya que los roles candidatos no existían.", userId);
                }

                if (success && !rolesToRemove.Any() && !rolesToAddValidated.Any())
                {
                    _logger.LogInformation("Los roles del usuario '{UserId}' no requirieron cambios o los roles deseados ya estaban asignados.", userId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar los roles del usuario '{UserId}'. Roles deseados: {Roles}", userId, string.Join(", ", desiredRoles));
                return false;
            }
        }

        public async Task<string?> GetPasswordResetTokenAsync(string userIdOrEmail)
        {
            if (string.IsNullOrWhiteSpace(userIdOrEmail))
            {
                _logger.LogWarning("El ID de usuario o email no puede ser nulo o vacío para generar el token de reseteo de contraseña.");
                return null;
            }

            ApplicationUser? user = null;
            try
            {
                if (Guid.TryParse(userIdOrEmail, out var userIdGuid))
                {
                    user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                }

                if (user == null) // Si no es un Guid válido o no se encontró por ID, intentar por email
                {
                    user = await _userManager.FindByEmailAsync(userIdOrEmail);
                }

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo para '{UserIdOrEmail}' al generar token de reseteo de contraseña.", userIdOrEmail);
                    return null; // No revelar si el usuario existe o no por seguridad
                }

                // Podríamos añadir una comprobación aquí si el email está confirmado, si es un requisito
                // if (!user.EmailConfirmed) 
                // {
                //     _logger.LogWarning("Intento de generar token de reseteo para usuario '{UserIdOrEmail}' con email no confirmado.", userIdOrEmail);
                //     return null; 
                // }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                _logger.LogInformation("Token de reseteo de contraseña generado para el usuario '{UserId}'.", user.Id);
                return token;
            }
            catch (Exception ex)
            {
                var userIdForLog = user?.Id.ToString() ?? userIdOrEmail;
                _logger.LogError(ex, "Error inesperado al generar el token de reseteo de contraseña para '{UserIdForLog}'.", userIdForLog);
                return null;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto)
        {
            if (resetPasswordDto == null)
            {
                _logger.LogWarning("El DTO para restablecer contraseña no puede ser nulo.");
                return false;
            }

            // Las DataAnnotations en ResetPasswordDTO deberían cubrir estas validaciones,
            // pero una comprobación adicional aquí puede ser útil.
            if (string.IsNullOrWhiteSpace(resetPasswordDto.Email) ||
                string.IsNullOrWhiteSpace(resetPasswordDto.Token) ||
                string.IsNullOrWhiteSpace(resetPasswordDto.Password))
            {
                _logger.LogWarning("Email, token o nueva contraseña no proporcionados en ResetPasswordDTO.");
                return false;
            }

            ApplicationUser? user = null;
            try
            {
                user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

                if (user == null || user.IsDeleted)
                {
                    // No revelar si el usuario existe o no. Registrar genéricamente.
                    _logger.LogWarning("Intento de restablecer contraseña para un usuario no encontrado o inactivo con email '{Email}'.", resetPasswordDto.Email);
                    return false;
                }

                var identityResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation("Contraseña restablecida exitosamente para el usuario '{UserId}'.", user.Id);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al restablecer la contraseña para el usuario '{UserId}': {Errors}", user.Id, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                var userIdForLog = user?.Id.ToString() ?? resetPasswordDto.Email;
                _logger.LogError(ex, "Error inesperado al restablecer la contraseña para '{UserIdForLog}'.", userIdForLog);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO changePasswordDto)
        {
            if (changePasswordDto == null)
            {
                _logger.LogWarning("El DTO para cambiar contraseña no puede ser nulo para el usuario '{UserId}'.", userId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("El ID de usuario no puede ser nulo o vacío para cambiar la contraseña.");
                return false;
            }

            // Las DataAnnotations en ChangePasswordDTO deberían cubrir estas validaciones,
            // pero una comprobación adicional aquí puede ser útil.
            if (string.IsNullOrWhiteSpace(changePasswordDto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(changePasswordDto.NewPassword))
            {
                _logger.LogWarning("La contraseña actual o la nueva contraseña no fueron proporcionadas en ChangePasswordDTO para el usuario '{UserId}'.", userId);
                return false;
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para cambiar la contraseña.", userId);
                return false;
            }

            ApplicationUser? user = null;
            try
            {
                user = await _userManager.FindByIdAsync(userIdGuid.ToString());

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo con ID '{UserId}' para cambiar la contraseña.", userId);
                    return false;
                }

                var identityResult = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation("Contraseña cambiada exitosamente para el usuario '{UserId}'.", userId);
                    // Considerar invalidar tokens de sesión aquí si es necesario (ej. actualizando SecurityStamp)
                    // await _userManager.UpdateSecurityStampAsync(user);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al cambiar la contraseña para el usuario '{UserId}': {Errors}", userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cambiar la contraseña para el usuario '{UserId}'.", userId);
                return false;
            }
        }

        public async Task<string?> ResendConfirmationEmailAsync(string userIdOrEmail)
        {
            if (string.IsNullOrWhiteSpace(userIdOrEmail))
            {
                _logger.LogWarning("El ID de usuario o email no puede ser nulo o vacío para reenviar el email de confirmación.");
                return null;
            }

            ApplicationUser? user = null;
            try
            {
                if (Guid.TryParse(userIdOrEmail, out var userIdGuid))
                {
                    user = await _userManager.FindByIdAsync(userIdGuid.ToString());
                }

                if (user == null) // Si no es un Guid válido o no se encontró por ID, intentar por email
                {
                    user = await _userManager.FindByEmailAsync(userIdOrEmail);
                }

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo para '{UserIdOrEmail}' al intentar reenviar email de confirmación.", userIdOrEmail);
                    return null; // No revelar si el usuario existe o no
                }

                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("El email para el usuario '{UserIdOrEmail}' ya está confirmado. No se reenviará el email de confirmación.", userIdOrEmail);
                    return null; // O un mensaje específico si la interfaz lo permitiera
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _logger.LogInformation("Nuevo token de confirmación de email generado para el usuario '{UserId}'.", user.Id);
                // Aquí se esperaría que otro servicio tome este token y envíe el email.
                return token;
            }
            catch (Exception ex)
            {
                var userIdForLog = user?.Id.ToString() ?? userIdOrEmail;
                _logger.LogError(ex, "Error inesperado al generar y reenviar el token de confirmación de email para '{UserIdForLog}'.", userIdForLog);
                return null;
            }
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("El ID de usuario o el token no pueden ser nulos o vacíos para confirmar el email.");
                return false;
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para confirmar el email.", userId);
                return false;
            }

            ApplicationUser? user = null;
            try
            {
                user = await _userManager.FindByIdAsync(userIdGuid.ToString());

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo con ID '{UserId}' para confirmar el email.", userId);
                    return false;
                }

                var identityResult = await _userManager.ConfirmEmailAsync(user, token);

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation("Email confirmado exitosamente para el usuario '{UserId}'.", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al confirmar el email para el usuario '{UserId}': {Errors}", userId, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al confirmar el email para el usuario '{UserId}'.", userId);
                return false;
            }
        }

        // Methods from the original IUserService that were not in the initial ApplicationUserService skeleton
        public async Task<bool> UpdateUserProfileAsync(string userId, GestMantIA.Shared.Identity.DTOs.UpdateProfileDTO profile)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("El ID de usuario no puede ser nulo o vacío para actualizar el perfil.");
                return false;
            }

            if (profile == null)
            {
                _logger.LogWarning("El DTO de perfil no puede ser nulo para el usuario '{UserId}'.", userId);
                return false;
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para actualizar el perfil.", userId);
                return false;
            }

            ApplicationUser? user = null;
            try
            {
                user = await _userManager.FindByIdAsync(userIdGuid.ToString());

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró un usuario activo con ID '{UserId}' para actualizar el perfil.", userId);
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
                                 // Por ahora, consideramos que no hacer nada es un 'éxito' si no hay errores.
                }

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
        public async Task<bool> LockUserAsync(string userId, TimeSpan? duration = null, string? reason = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("El ID de usuario no puede ser nulo o vacío para bloquear al usuario.");
                return false;
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para bloquear al usuario.", userId);
                return false;
            }

            ApplicationUser? user = null;
            try
            {
                user = await _userManager.FindByIdAsync(userIdGuid.ToString());

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
                    // Usar el tiempo de bloqueo por defecto configurado en IdentityOptions
                    lockoutEndDto = DateTimeOffset.UtcNow.Add(_userManager.Options.Lockout.DefaultLockoutTimeSpan);
                }

                var identityResult = await _userManager.SetLockoutEndDateAsync(user, lockoutEndDto);

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation("Usuario '{UserId}' bloqueado exitosamente hasta {LockoutEnd}. Razón: {Reason}",
                        userId, lockoutEndDto, string.IsNullOrWhiteSpace(reason) ? "No especificada" : reason);
                    // Aquí se podría registrar la razón del bloqueo en un log de auditoría más detallado si existiera.
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
        public async Task<bool> UnlockUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("El ID de usuario no puede ser nulo o vacío para desbloquear al usuario.");
                return false;
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para desbloquear al usuario.", userId);
                return false;
            }

            ApplicationUser? user = null;
            try
            {
                user = await _userManager.FindByIdAsync(userIdGuid.ToString());

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

                // Para desbloquear, se establece la fecha de finalización del bloqueo a un momento actual o pasado.
                // Establecerlo a DateTimeOffset.UtcNow efectivamente lo desbloquea.
                // También se podría usar null para resetear el LockoutEnd y el AccessFailedCount.
                // userManager.ResetAccessFailedCountAsync(user) también es una opción si solo se quiere resetear el contador.
                var identityResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation("Usuario '{UserId}' desbloqueado exitosamente.", userId);
                    // Adicionalmente, resetear el contador de accesos fallidos podría ser una buena práctica aquí.
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
        public async Task<bool> IsUserLockedOutAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("El ID de usuario no puede ser nulo o vacío para verificar si está bloqueado.");
                return false;
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para verificar si está bloqueado.", userId);
                return false;
            }

            ApplicationUser? user = null;
            try
            {
                user = await _userManager.FindByIdAsync(userIdGuid.ToString());

                if (user == null || user.IsDeleted) // Un usuario no existente o eliminado no está 'bloqueado'
                {
                    _logger.LogInformation("No se encontró un usuario activo con ID '{UserId}' para verificar si está bloqueado.", userId);
                    return false;
                }

                return await _userManager.IsLockedOutAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al verificar si el usuario '{UserId}' está bloqueado.", userId);
                return false; // En caso de error, asumir que no está bloqueado o manejar según política
            }
        }
        public async Task<UserLockoutInfo?> GetUserLockoutInfoAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("El ID de usuario no puede ser nulo o vacío para obtener información de bloqueo.");
                return null;
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para obtener información de bloqueo.", userId);
                return null;
            }

            ApplicationUser? user = null;
            try
            {
                user = await _userManager.FindByIdAsync(userIdGuid.ToString());

                if (user == null || user.IsDeleted)
                {
                    _logger.LogInformation("No se encontró un usuario activo con ID '{UserId}' para obtener información de bloqueo.", userId);
                    return null;
                }

                var isLockedOut = await _userManager.IsLockedOutAsync(user);
                var lockoutEndDateOffset = await _userManager.GetLockoutEndDateAsync(user);

                // Razón y LockoutStart no son directamente gestionados por UserManager de forma estándar.
                // Se podrían añadir a ApplicationUser o a un sistema de auditoría si fuera necesario.
                // Por ahora, se dejan como null o valores por defecto.
                return new UserLockoutInfo
                {
                    UserId = user.Id.ToString(),
                    UserName = user.UserName ?? string.Empty,
                    IsLockedOut = isLockedOut,
                    LockoutEnd = lockoutEndDateOffset?.UtcDateTime,
                    IsPermanent = lockoutEndDateOffset == DateTimeOffset.MaxValue, // Convención común para bloqueo permanente
                    Reason = null, // TODO: Implementar si se añade lógica de auditoría para razones de bloqueo
                    LockoutStart = null // TODO: Implementar si se añade lógica de auditoría para inicio de bloqueo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener la información de bloqueo para el usuario '{UserId}'.", userId);
                return null;
            }
        }

        public async Task<bool> AssignRolesToUserAsync(string userId, IEnumerable<string> roleNames)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("El ID de usuario no puede ser nulo o vacío para asignar roles.");
                return false;
            }

            if (roleNames == null || !roleNames.Any())
            {
                _logger.LogWarning("La lista de nombres de roles no puede ser nula o vacía para el usuario '{UserId}'.", userId);
                return false; // O true si se considera que no hacer nada es un éxito en este caso
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para asignar roles.", userId);
                return false;
            }

            var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("No se encontró un usuario activo con ID '{UserId}' para asignar roles.", userId);
                return false;
            }

            try
            {
                // Filtrar roles que realmente existen para evitar errores
                var existingRoleNames = new List<string>();
                foreach (var roleName in roleNames.Distinct())
                {
                    if (await _roleManager.RoleExistsAsync(roleName))
                    {
                        existingRoleNames.Add(roleName);
                    }
                    else
                    {
                        _logger.LogWarning("El rol '{RoleName}' no existe y no se asignará al usuario '{UserId}'.", roleName, userId);
                    }
                }

                if (!existingRoleNames.Any())
                {
                    _logger.LogInformation("No se encontraron roles válidos para asignar al usuario '{UserId}'.", userId);
                    return true; // O false dependiendo de la semántica deseada
                }

                var result = await _userManager.AddToRolesAsync(user, existingRoleNames);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Roles asignados exitosamente al usuario '{UserId}'. Roles: {Roles}", userId, string.Join(", ", existingRoleNames));
                    return true;
                }
                else
                {
                    _logger.LogError("Error al asignar roles al usuario '{UserId}'. Errores: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al asignar roles al usuario '{UserId}'.", userId);
                return false;
            }
        }

        public async Task<bool> RemoveRolesFromUserAsync(string userId, IEnumerable<string> roleNames)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("El ID de usuario no puede ser nulo o vacío para remover roles.");
                return false;
            }

            if (roleNames == null || !roleNames.Any())
            {
                _logger.LogWarning("La lista de nombres de roles no puede ser nula o vacía para el usuario '{UserId}'.", userId);
                return true; // Considerar éxito si no hay roles que remover
            }

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                _logger.LogWarning("El ID de usuario '{UserId}' no es un GUID válido para remover roles.", userId);
                return false;
            }

            var user = await _userManager.FindByIdAsync(userIdGuid.ToString());
            if (user == null || user.IsDeleted) // No se pueden remover roles de un usuario no existente o eliminado
            {
                _logger.LogWarning("No se encontró un usuario activo con ID '{UserId}' para remover roles.", userId);
                return false;
            }

            try
            {
                // Solo intentar remover roles que el usuario realmente tiene o que existen, para evitar errores innecesarios.
                // UserManager.RemoveFromRolesAsync es idempotente para roles que el usuario no tiene.
                var result = await _userManager.RemoveFromRolesAsync(user, roleNames.Distinct());
                if (result.Succeeded)
                {
                    _logger.LogInformation("Roles removidos exitosamente del usuario '{UserId}'. Roles: {Roles}", userId, string.Join(", ", roleNames.Distinct()));
                    return true;
                }
                else
                {
                    _logger.LogError("Error al remover roles del usuario '{UserId}'. Errores: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al remover roles del usuario '{UserId}'.", userId);
                return false;
            }
        }

        // El bloque de placeholders anterior ha sido eliminado ya que sus funcionalidades
        // están cubiertas por los métodos implementados de IUserService o representan
        // un patrón de diseño anterior (uso de Result<T> y parámetros de auditoría explícitos
        // que se manejarán de otra forma).
    }
}
