using GestMantIA.Shared.Identity.DTOs;

namespace GestMantIA.Application.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
        Task UpdateUserProfileAsync(Guid UserId, UserProfileDto userProfileDto);
        Task<UserProfileDto> CreateUserProfileAsync(UserProfileDto userProfileDto);
    }
}
