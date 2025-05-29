using GestMantIA.Core.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

// Temporalmente en este namespace hasta resolver problemas de creación de carpetas
namespace GestMantIA.Infrastructure.Identity.Factories
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            
            // Añadir claims personalizados basados en propiedades de ApplicationUser
            if (!string.IsNullOrWhiteSpace(user.FirstName))
            {
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            }

            if (!string.IsNullOrWhiteSpace(user.LastName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
            }
            
            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                 identity.AddClaim(new Claim("full_name", user.FullName));
            }

            // El ClaimTypes.NameIdentifier (user.Id) ya es añadido por la clase base.
            // El ClaimTypes.Name (user.UserName) ya es añadido por la clase base.
            // El ClaimTypes.Email (user.Email) ya es añadido por la clase base.
            // Los roles (ClaimTypes.Role) ya son añadidos por la clase base.

            // Los claims Jti e Iat del método ToClaims() original son más para la generación
            // del token JWT en sí, no tanto para el ClaimsPrincipal del usuario.
            // Esos se deben añadir en el servicio que genera el token JWT.

            return identity;
        }
    }
}
