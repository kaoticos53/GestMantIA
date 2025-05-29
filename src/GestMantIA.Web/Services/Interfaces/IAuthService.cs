using System.Threading.Tasks;
using GestMantIA.Web.Models;

namespace GestMantIA.Web.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginModel loginModel);
        Task<AuthResponse> RegisterAsync(RegisterModel registerModel);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetTokenAsync();
        Task RefreshTokenAsync();
    }
}
