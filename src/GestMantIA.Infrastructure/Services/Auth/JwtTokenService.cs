using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsPrincipalFactory;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="JwtTokenService"/>
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación.</param>
        /// <param name="context">Contexto de la base de datos.</param>
        /// <param name="userManager">Administrador de usuarios.</param>
        public JwtTokenService(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsPrincipalFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
        }

        /// <inheritdoc />
        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetSecretKey());

            // Obtener claims del usuario a través de CustomUserClaimsPrincipalFactory
            var userPrincipal = await _claimsPrincipalFactory.CreateAsync(user);
            var claims = new List<Claim>(userPrincipal.Claims);

            // Añadir claims específicos del token JWT
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            // El claim Iat (Issued At) es añadido automáticamente por JwtSecurityTokenHandler si no se especifica.
            // Si se desea control explícito:
            // claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            // Agregar los roles como claims individuales para compatibilidad con ASP.NET Core
            // Importante: No agregar los roles como un array, sino como múltiples claims "role" (uno por cada rol)
            //var userRoles = await _userManager.GetRolesAsync(user);
            //foreach (var role in userRoles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role)); // Cada rol se agrega como claim independiente
            //}

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // Usar los claims obtenidos y los específicos del JWT
                Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = GetIssuer(),
                Audience = GetAudience()
            };

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
                CreatedAt = DateTime.UtcNow,
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
        public Guid? GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var claims = GetClaimsFromToken(token);
            var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return userId;
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
