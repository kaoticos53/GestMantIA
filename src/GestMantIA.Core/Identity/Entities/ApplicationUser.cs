using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase que representa a un usuario en el sistema.
    /// </summary>
    public class ApplicationUser : IdentityUser<string>
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ApplicationUser"/>
        /// </summary>
        public ApplicationUser()
        {
            Username = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            PhoneNumber = string.Empty;
            LockoutReason = string.Empty;
            FullName = string.Empty;
            UserRoles = new HashSet<ApplicationUserRole>();
            RefreshTokens = new HashSet<RefreshToken>();
        }

        /// <summary>
        /// Nombre de usuario para iniciar sesión.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Hash de la contraseña del usuario.
        /// </summary>
        [JsonIgnore]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Indica si el correo electrónico ha sido confirmado.
        /// </summary>
        public bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// Indica si el número de teléfono ha sido confirmado.
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; } = false;

        /// <summary>
        /// Indica si la autenticación de dos factores está habilitada para este usuario.
        /// </summary>
        public bool TwoFactorEnabled { get; set; } = false;

        /// <summary>
        /// Fecha de bloqueo del usuario, si está bloqueado.
        /// </summary>
        public DateTimeOffset? LockoutEnd { get; set; }
        
        /// <summary>
        /// Indica si el usuario puede ser bloqueado.
        /// </summary>
        public bool LockoutEnabled { get; set; } = true;

        /// <summary>
        /// Indica si el usuario está activo en el sistema.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Razón del bloqueo del usuario (si está bloqueado).
        /// </summary>
        public string LockoutReason { get; set; }
        
        /// <summary>
        /// Fecha en que se produjo el bloqueo del usuario.
        /// </summary>
        public DateTime? LockoutDate { get; set; }
        
        /// <summary>
        /// Número de intentos fallidos de inicio de sesión.
        /// </summary>
        public int AccessFailedCount { get; set; } = 0;

        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        [StringLength(100)]
        public string FullName { get; set; }

        /// <summary>
        /// Relación con los roles del usuario.
        /// </summary>
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        /// <summary>
        /// Tokens de actualización asociados al usuario.
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Convierte el usuario a una lista de claims para el token JWT.
        /// </summary>
        /// <returns>Lista de claims del usuario.</returns>
        public virtual IEnumerable<Claim> ToClaims()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Id.ToString()),
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.Email, Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Agregar roles como claims
            if (UserRoles != null)
            {
                foreach (var role in UserRoles.Select(ur => ur.Role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            return claims;
        }
    }
}
