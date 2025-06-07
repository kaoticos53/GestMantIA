using Blazored.LocalStorage;
using GestMantIA.Web.Models; 
using GestMantIA.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;
using GestMantIA.Web.HttpClients; 
using System; 

namespace GestMantIA.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGestMantIAApiClient _apiClient; 
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;
        private readonly string _authTokenStorageKey = "authToken";
        private readonly string _refreshTokenStorageKey = "refreshToken";
        private readonly string _userStorageKey = "user"; 

        public AuthService(
            IGestMantIAApiClient apiClient, 
            AuthenticationStateProvider authStateProvider,
            ILocalStorageService localStorage)
        {
            _apiClient = apiClient;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        public async Task<AuthResponse> LoginAsync(LoginModel loginModel)
        {
            try
            {
                var authResult = await _apiClient.LoginAsync(new LoginRequest
                {
                    UsernameOrEmail = loginModel.Email,
                    Password = loginModel.Password
                });

                if (authResult != null && authResult.Succeeded && !string.IsNullOrEmpty(authResult.AccessToken))
                {
                    await _localStorage.SetItemAsync(_authTokenStorageKey, authResult.AccessToken);

                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(authResult.AccessToken);
                    return new AuthResponse
                    {
                        Success = true,
                        Token = authResult.AccessToken,
                        RefreshToken = authResult.RefreshToken,
                        Message = "Inicio de sesión exitoso."
                    };
                }
                else
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = authResult?.Message ?? "No se recibió un token de autenticación válido del servidor."
                    };
                }
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"API Exception durante el login: {ex.StatusCode} - {ex.Response}");
                string errorMessage = "Error de autenticación. Verifique sus credenciales.";
                if (ex.StatusCode >= 500)
                {
                    errorMessage = "Error interno del servidor. Inténtelo más tarde.";
                }
                return new AuthResponse { Success = false, Message = string.IsNullOrEmpty(ex.Response) ? errorMessage : ex.Response };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción genérica durante el login: {ex.ToString()}");
                return new AuthResponse { Success = false, Message = "No se pudo conectar con el servidor o ocurrió un error inesperado." };
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterModel registerModel)
        {
            // Cuerpo del método RegisterAsync comentado temporalmente.
            Console.WriteLine("RegisterAsync temporalmente deshabilitado debido a la ausencia de swaggerClient.");
            return await Task.FromResult(new AuthResponse { Success = false, Message = "Servicio de registro no disponible temporalmente." });
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(_authTokenStorageKey);
            await _localStorage.RemoveItemAsync(_refreshTokenStorageKey);
            await _localStorage.RemoveItemAsync(_userStorageKey);

            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>(_authTokenStorageKey) ?? string.Empty;
        }

        public async Task RefreshTokenAsync()
        {
            // Cuerpo del método RefreshTokenAsync comentado temporalmente.
            Console.WriteLine("RefreshTokenAsync temporalmente deshabilitado debido a la ausencia de swaggerClient.");
            await Task.CompletedTask;
        }
    }
}
