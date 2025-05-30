using AutoMapper;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Shared;
using GestMantIA.Shared.Identity.DTOs;
using GestMantIA.Shared.Identity.DTOs.Requests;
using GestMantIA.Shared.Identity.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GestMantIA.Application.Features.UserManagement.Services
{
    public class UserService : IUserService, IDisposable
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool _disposed = false;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper,
            ILogger<UserService> logger,
            IUserRepository userRepository,
            IOptions<IdentityOptions> identityOptions,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityOptions = identityOptions ?? throw new ArgumentNullException(nameof(identityOptions));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("No se encontró el usuario con ID {UserId} o está marcado como eliminado.", userId);
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

        // Stubs para métodos de IUserService movidos a Application.UserService o no implementados aquí

        public Task<bool> UnlockUserAsync(string userId)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(UnlockUserAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<bool> IsUserLockedOutAsync(string userId)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(IsUserLockedOutAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<UserLockoutInfo?> GetUserLockoutInfoAsync(string userId)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(GetUserLockoutInfoAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<UserResponseDTO> CreateUserAsync(CreateUserDTO createUserDto, IEnumerable<string>? roleNames = null)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(CreateUserAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<UserResponseDTO> UpdateUserAsync(string userId, UpdateUserDTO userDto)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(UpdateUserAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<bool> DeleteUserAsync(string userId, bool hardDelete = false)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(DeleteUserAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<UserResponseDTO?> GetUserByIdAsync(string userId)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(GetUserByIdAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<PagedResult<UserResponseDTO>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool activeOnly = false)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(GetAllUsersAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<bool> AssignRolesToUserAsync(string userId, IEnumerable<string> roleNames)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(AssignRolesToUserAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<bool> RemoveRolesFromUserAsync(string userId, IEnumerable<string> roleNames)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(RemoveRolesFromUserAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<string?> GetPasswordResetTokenAsync(string userIdOrEmail)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(GetPasswordResetTokenAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<bool> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(ResetPasswordAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO changePasswordDto)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(ChangePasswordAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(ConfirmEmailAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
        }

        public Task<string?> ResendConfirmationEmailAsync(string userIdOrEmail)
        {
            _logger.LogWarning("{MethodName} no está implementado en Infrastructure.UserService y debería ser manejado por Application.UserService.", nameof(ResendConfirmationEmailAsync));
            throw new NotImplementedException("Este método se ha movido a Application.UserService o no está implementado aquí.");
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
                var query = _userManager.Users.Where(u => !u.IsDeleted).AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(u =>
                        u.UserName != null && u.UserName.ToLower().Contains(searchTermLower) ||
                        u.Email != null && u.Email.ToLower().Contains(searchTermLower) ||
                        u.FirstName != null && u.FirstName.ToLower().Contains(searchTermLower) ||
                        u.LastName != null && u.LastName.ToLower().Contains(searchTermLower));
                }


                var allUsers = await _userRepository.GetAllAsync();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    allUsers = allUsers.Where(u =>
                        (u.UserName != null && u.UserName.ToLower().Contains(searchTermLower)) ||
                        (u.Email != null && u.Email.ToLower().Contains(searchTermLower)) ||
                        (u.FirstName != null && u.FirstName.ToLower().Contains(searchTermLower)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(searchTermLower))
                    ).ToList();
                }

                var totalCount = allUsers.Count();
                var users = allUsers
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

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
