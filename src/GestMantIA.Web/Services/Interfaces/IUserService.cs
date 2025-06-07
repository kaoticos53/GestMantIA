using System.Collections.Generic;
using System.Threading.Tasks;
using GestMantIA.Core.Shared;
using GestMantIA.Web.Models.Users;

namespace GestMantIA.Web.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResult<UserListModel>> GetAllUsersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool activeOnly = true);
        Task<UserListModel> GetUserByIdAsync(Guid userId);
        Task<ApiResponse> CreateUserAsync(CreateUserModel model);
        Task<ApiResponse> UpdateUserAsync(Guid userId, UpdateUserModel model);
        Task<ApiResponse> DeleteUserAsync(Guid userId);
        Task<ApiResponse> AssignRolesAsync(Guid userId, IEnumerable<string> roles);
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
