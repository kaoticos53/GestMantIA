using AutoMapper;
using GestMantIA.Application.Interfaces;
using GestMantIA.Core.Repositories;
using GestMantIA.Core.Entities;
using GestMantIA.Shared.Identity.DTOs;
using Microsoft.Extensions.Logging;
using GestMantIA.Core.Identity.Entities;
using Microsoft.AspNetCore.Identity;
// using GestMantIA.Application.Common.Exceptions;
using GestMantIA.Core.Interfaces;

namespace GestMantIA.Application.Features.UserManagement.Services
{
    public class ApplicationUserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper; 
        private readonly ILogger<ApplicationUserProfileService> _logger;

        public ApplicationUserProfileService(
            IUserProfileRepository userProfileRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<ApplicationUserProfileService> logger)
        {
            _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<UserProfileDto> CreateUserProfileAsync(UserProfileDto userProfileDto)
        {
            _logger.LogInformation("Creando perfil de usuario para el usuario con ID: {UserId}", userProfileDto.UserId);
            // TODO: Implementar lógica para crear el perfil de usuario
            throw new NotImplementedException();
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
        {
            _logger.LogInformation("Intentando obtener perfil para UserId: {UserId}", userId);
            try
            {
                var applicationUser = await _userManager.FindByIdAsync(userId.ToString());
                if (applicationUser == null)
                {
                    _logger.LogWarning("Usuario no encontrado con ID: {UserId}", userId);
                    return null; // O lanzar UserNotFoundException si se prefiere un manejo de error más estricto
                }

                // Obtener el UserProfile asociado
                // TODO: Optimizar esto. Idealmente, IUserProfileRepository debería tener un método como GetByUserIdAsync o FirstOrDefaultAsync(predicate).
                // Solución temporal para evitar error de compilación CS1061:
                var userProfiles = await _userProfileRepository.ListAsync(up => up.UserId == userId); // CancellationToken removido
                var userProfile = userProfiles.FirstOrDefault();

                var userRoles = await _userManager.GetRolesAsync(applicationUser);

                var userProfileDto = new UserProfileDto
                {
                    UserId = applicationUser.Id,
                    UserName = applicationUser.UserName ?? string.Empty,
                    Email = applicationUser.Email ?? string.Empty,
                    EmailConfirmed = applicationUser.EmailConfirmed,
                    PhoneNumber = userProfile?.PhoneNumber ?? applicationUser.PhoneNumber,
                    PhoneNumberConfirmed = applicationUser.PhoneNumberConfirmed,
                    TwoFactorEnabled = applicationUser.TwoFactorEnabled,
                    LockoutEnd = applicationUser.LockoutEnd,
                    LockoutEnabled = applicationUser.LockoutEnabled,
                    IsActive = applicationUser.IsActive,
                    Roles = userRoles?.ToList() ?? new List<string>(),

                    Id = userProfile?.Id ?? Guid.Empty,
                    FirstName = userProfile?.FirstName ?? applicationUser.FirstName ?? string.Empty,
                    LastName = userProfile?.LastName ?? applicationUser.LastName ?? string.Empty,
                    DateOfBirth = userProfile?.DateOfBirth,
                    AvatarUrl = userProfile?.AvatarUrl,
                    Bio = userProfile?.Bio,
                    StreetAddress = userProfile?.StreetAddress,
                    City = userProfile?.City,
                    StateProvince = userProfile?.StateProvince,
                    PostalCode = userProfile?.PostalCode,
                    Country = userProfile?.Country,

                    CreatedAt = userProfile?.CreatedAt ?? DateTime.UtcNow, 
                    UpdatedAt = userProfile?.UpdatedAt,
                    CreatedBy = userProfile?.CreatedBy,
                    UpdatedBy = userProfile?.UpdatedBy
                };

                _logger.LogInformation("Perfil de usuario para {UserId} obtenido exitosamente.", userId);
                return userProfileDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario para el ID: {UserId}", userId);
                // Considerar lanzar una excepción personalizada o devolver un Result<T> con error
                return null; 
            }
        }

        public Task UpdateUserProfileAsync(Guid userId, UserProfileDto userProfileDto)
        {
            _logger.LogInformation("Actualizando perfil de usuario para el ID: {UserId}", userId);
            // TODO: Implementar lógica para actualizar el perfil de usuario
            throw new NotImplementedException();
        }
    }
}
