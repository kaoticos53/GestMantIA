using GestMantIA.Shared.Identity.DTOs;

namespace GestMantIA.Application.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task UpdateUserProfileAsync(string userId, UserProfileDto userProfileDto);
        Task<UserProfileDto> CreateUserProfileAsync(UserProfileDto userProfileDto);
    }
}
