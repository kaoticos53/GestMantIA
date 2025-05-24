using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GestMantIA.Infrastructure.Services.Auth
{
    /// <summary>
    /// Implementación del servicio para la generación y validación de tokens JWT.
    /// </summary>
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="JwtTokenService"/>
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación.</param>
        /// <param name="context">Contexto de la base de datos.</param>
        public JwtTokenService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public string GenerateAccessToken(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetSecretKey());
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                }),
                Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = GetIssuer(),
                Audience = GetAudience()
            };

            // Agregar roles como claims
            // Nota: Esto es un ejemplo básico. En una aplicación real, deberías cargar los roles del usuario.
            // var userRoles = await _userManager.GetRolesAsync(user);
            // foreach (var role in userRoles)
            // {
            //     tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            // }


            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <inheritdoc />
        public async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, string ipAddress)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("La dirección IP no puede estar vacía.", nameof(ipAddress));

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = GenerateRefreshToken(),
                Expires = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays()),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            // Guardar el token en la base de datos
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        /// <inheritdoc />
        public IEnumerable<Claim> GetClaimsFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("El token no puede estar vacío.", nameof(token));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetSecretKey());

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = GetIssuer(),
                    ValidateAudience = true,
                    ValidAudience = GetAudience(),
                    ValidateLifetime = false // No validar la expiración aquí
                }, out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken.Claims;
            }
            catch (Exception)
            {
                // En producción, considera registrar la excepción
                return new List<Claim>();
            }
        }

        /// <inheritdoc />
        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetSecretKey());

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = GetIssuer(),
                    ValidateAudience = true,
                    ValidAudience = GetAudience(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return true;
            }
            catch (Exception)
            {
                // En producción, considera registrar la excepción
                return false;
            }
        }

        /// <inheritdoc />
        public string GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var claims = GetClaimsFromToken(token);
            return claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        #region Métodos auxiliares privados

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GetSecretKey()
        {
            var key = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("No se ha configurado la clave secreta JWT.");

            return key;
        }

        private string GetIssuer()
        {
            return _configuration["JwtSettings:Issuer"] ?? "GestMantIA.API";
        }

        private string GetAudience()
        {
            return _configuration["JwtSettings:Audience"] ?? "GestMantIA.Clients";
        }

        private int GetTokenExpirationMinutes()
        {
            if (int.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out int minutes))
                return minutes;

            return 60; // Valor por defecto: 60 minutos
        }

        private int GetRefreshTokenExpirationDays()
        {
            if (int.TryParse(_configuration["JwtSettings:RefreshTokenExpirationDays"], out int days))
                return days;

            return 7; // Valor por defecto: 7 días
        }

        #endregion
    }
}
