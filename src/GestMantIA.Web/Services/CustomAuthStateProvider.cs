using System.Security.Claims;
using System.Text.Json;
using System.Linq;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System;

namespace GestMantIA.Web.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private const string AuthTokenKey = "authToken";

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>(AuthTokenKey);

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }

        // Modificado para aceptar el token directamente
        public void NotifyUserAuthentication(string token)
        {
            ClaimsPrincipal user;
            if (string.IsNullOrEmpty(token))
            {
                // Esto no debería ocurrir si AuthService lo llama después de un login exitoso,
                // pero es bueno tener un fallback a anónimo.
                user = new ClaimsPrincipal(new ClaimsIdentity());
            }
            else
            {
                var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                user = new ClaimsPrincipal(identity);
            }
            
            var authState = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];

            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            Console.WriteLine("JWT Payload KeyValuePairs:");
            foreach (var kvp in keyValuePairs ?? new Dictionary<string, object>())
            {
                Console.WriteLine($"  Key: {kvp.Key}, Value: {kvp.Value}");
            }

            if (keyValuePairs != null)
            {
                keyValuePairs.TryGetValue("role", out var roles); // Cambiado de ClaimTypes.Role a "role"
                if (roles != null)
                {
                    if (roles is JsonElement rolesElement && rolesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleValue = role.ToString();
                            claims.Add(new Claim(ClaimTypes.Role, roleValue));
                            Console.WriteLine($"Added role from array: {roleValue}");
                        }
                    }
                    else
                    {
                        var roleValue = roles.ToString() ?? string.Empty;
                        claims.Add(new Claim(ClaimTypes.Role, roleValue));
                        Console.WriteLine($"Added single role: {roleValue}");
                    }
                }
                else
                {
                    Console.WriteLine("Role claim ('role') not found or is null."); // Cambiado de ClaimTypes.Role a "role"
                }

                if (keyValuePairs.TryGetValue("sub", out var sub))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.ToString() ?? string.Empty));
                }
                if (keyValuePairs.TryGetValue(ClaimTypes.Name, out var name))
                {
                    claims.Add(new Claim(ClaimTypes.Name, name.ToString() ?? string.Empty));
                }
                else if (keyValuePairs.TryGetValue("name", out var nameAlt))
                {
                    claims.Add(new Claim(ClaimTypes.Name, nameAlt.ToString() ?? string.Empty));
                }

                if (keyValuePairs.TryGetValue(ClaimTypes.Email, out var email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, email.ToString() ?? string.Empty));
                }
                else if (keyValuePairs.TryGetValue("email", out var emailAlt))
                {
                    claims.Add(new Claim(ClaimTypes.Email, emailAlt.ToString() ?? string.Empty));
                }

                // Añadir otros claims que te interesen del payload
                // Ejemplo: claims personalizados
                // if (keyValuePairs.TryGetValue("custom_claim_name", out var customClaimValue))
                // {
                //     claims.Add(new Claim("custom_claim_name", customClaimValue.ToString() ?? string.Empty));
                // }
            }

            Console.WriteLine("Final claims parsed:");
            foreach (var claim in claims)
            {
                Console.WriteLine($"  Type: {claim.Type}, Value: {claim.Value}");
            }
            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
