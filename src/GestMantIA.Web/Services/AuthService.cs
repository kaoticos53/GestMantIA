using System.Net.Http.Json;
using Blazored.LocalStorage;
using GestMantIA.Web.Models;
using GestMantIA.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestMantIA.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;
        private readonly string _authTokenStorageKey = "authToken";
        private readonly string _refreshTokenStorageKey = "refreshToken";
        private readonly string _userStorageKey = "user";

        public AuthService(
            HttpClient httpClient,
            AuthenticationStateProvider authStateProvider,
            ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        public async Task<AuthResponse> LoginAsync(LoginModel loginModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginModel);
            var loginResult = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (loginResult?.Success == true)
            {
                await _localStorage.SetItemAsync(_authTokenStorageKey, loginResult.Token);
                await _localStorage.SetItemAsync(_refreshTokenStorageKey, loginResult.RefreshToken);
                await _localStorage.SetItemAsync(_userStorageKey, loginResult);

                ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(loginResult.Email);
            }

            return loginResult ?? new AuthResponse { Success = false, Message = "Error en el inicio de sesión" };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterModel registerModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerModel);
            var registerResult = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (registerResult?.Success == true)
            {
                await _localStorage.SetItemAsync(_authTokenStorageKey, registerResult.Token);
                await _localStorage.SetItemAsync(_refreshTokenStorageKey, registerResult.RefreshToken);
                await _localStorage.SetItemAsync(_userStorageKey, registerResult);

                ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(registerResult.Email);
            }

            return registerResult ?? new AuthResponse { Success = false, Message = "Error en el registro" };
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(_authTokenStorageKey);
            await _localStorage.RemoveItemAsync(_refreshTokenStorageKey);
            await _localStorage.RemoveItemAsync(_userStorageKey);

            ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
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
            var refreshToken = await _localStorage.GetItemAsync<string>(_refreshTokenStorageKey);
            if (string.IsNullOrEmpty(refreshToken))
            {
                await LogoutAsync();
                return;
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", new { token = refreshToken });
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                if (result?.Success == true)
                {
                    await _localStorage.SetItemAsync(_authTokenStorageKey, result.Token);
                    await _localStorage.SetItemAsync(_refreshTokenStorageKey, result.RefreshToken);
                }
                else
                {
                    await LogoutAsync();
                }
            }
            catch
            {
                await LogoutAsync();
            }
        }
    }
}
