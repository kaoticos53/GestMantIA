using AutoMapper;
using GestMantIA.Application.Interfaces;
using GestMantIA.Core.Repositories;
using GestMantIA.Shared.Identity.DTOs;
using Microsoft.Extensions.Logging;

namespace GestMantIA.Application.Features.UserManagement.Services
{
    public class ApplicationUserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ApplicationUserProfileService> _logger;

        public ApplicationUserProfileService(
            IUserProfileRepository userProfileRepository,
            IMapper mapper,
            ILogger<ApplicationUserProfileService> logger)
        {
            _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<UserProfileDto> CreateUserProfileAsync(UserProfileDto userProfileDto)
        {
            _logger.LogInformation("Creando perfil de usuario para el usuario con ID: {UserId}", userProfileDto.UserId);
            // TODO: Implementar lógica para crear el perfil de usuario
            throw new NotImplementedException();
        }

        public Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            _logger.LogInformation("Obteniendo perfil de usuario para el ID: {UserId}", userId);
            // TODO: Implementar lógica para obtener el perfil de usuario
            throw new NotImplementedException();
        }

        public Task UpdateUserProfileAsync(string userId, UserProfileDto userProfileDto)
        {
            _logger.LogInformation("Actualizando perfil de usuario para el ID: {UserId}", userId);
            // TODO: Implementar lógica para actualizar el perfil de usuario
            throw new NotImplementedException();
        }
    }
}
