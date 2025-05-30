using GestMantIA.Core.Identity.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GestMantIA.Infrastructure.Services.Auth
{
    public class CustomSignInManager : SignInManager<ApplicationUser>
    {
        public CustomSignInManager(UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        protected override async Task<SignInResult?> PreSignInCheck(ApplicationUser user)
        {
            var result = await base.PreSignInCheck(user);
            if (result == null || !result.Succeeded)
            {
                return result;
            }

            if (user.IsDeleted)
            {
                Logger.LogWarning(0, "User {userId} is deleted and cannot sign in.", await UserManager.GetUserIdAsync(user));
                return SignInResult.Failed; // O un resultado más específico si se quiere diferenciar
            }

            if (!user.IsActive)
            {
                Logger.LogWarning(1, "User {userId} is not active and cannot sign in.", await UserManager.GetUserIdAsync(user));
                return SignInResult.NotAllowed; // Identity usa NotAllowed para cuentas no confirmadas, etc.
            }

            return SignInResult.Success;
        }

        // Opcionalmente, se podría sobrescribir CanSignInAsync si se prefiere una lógica más granular
        // public override async Task<bool> CanSignInAsync(ApplicationUser user)
        // {
        //     if (user.IsDeleted) return false;
        //     if (!user.IsActive) return false;
        //     return await base.CanSignInAsync(user);
        // }
    }
}
