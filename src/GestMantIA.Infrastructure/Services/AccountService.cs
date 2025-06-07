using GestMantIA.Application; // Para Result
using GestMantIA.Application.Interfaces; // Para IAccountService
using GestMantIA.Core.Identity.Entities; // Para ApplicationUser
using GestMantIA.Core.Interfaces; // Para IEmailService
using GestMantIA.Shared.Identity.DTOs; // Para ForgotPasswordRequest, ResetPasswordRequest
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities; // Para WebEncoders
using Microsoft.Extensions.Logging; // Para ILogger
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web; // Para HttpUtility

namespace GestMantIA.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe por seguridad.
                // Registrar internamente si es necesario.
                _logger.LogInformation("Solicitud de restablecimiento de contraseña para correo no existente: {Email}", request.Email);
                return Result.Success(); // Devolver éxito para no dar pistas.
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Codificar el token para que sea seguro en URL
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            user.PasswordResetToken = encodedToken; // Guardar el token codificado
            user.PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(2); // Expiración en 2 horas
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                _logger.LogError("Error al guardar el token de restablecimiento para el usuario {UserId}", user.Id);
                return Result.Failure("Ocurrió un error al intentar iniciar el restablecimiento de contraseña.");
            }
            
            // Construir el enlace de restablecimiento
            // Asegurarse de que origin no tenga una barra al final para evitar dobles barras.
            var callbackUrl = $"{origin.TrimEnd('/')}/account/reset-password?token={HttpUtility.UrlEncode(encodedToken)}&email={HttpUtility.UrlEncode(user.Email)}";

            var emailBody = $"<p>Por favor, restablezca su contraseña haciendo clic <a href='{callbackUrl}'>aquí</a>.</p>" +
                            $"<p>Si no solicitó esto, ignore este correo electrónico.</p>" +
                            $"<p>Este enlace expirará en 2 horas.</p>";

            try
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    _logger.LogWarning("No se puede enviar correo de restablecimiento: el correo del usuario es nulo o vacío");
                    return Result.Failure("No se puede enviar el correo de restablecimiento: dirección de correo no válida");
                }

                await _emailService.SendEmailAsync(user.Email, "Restablecer Contraseña - GestMantIA", emailBody);
                _logger.LogInformation("Correo de restablecimiento de contraseña enviado a {Email}", user.Email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de restablecimiento de contraseña a {Email}", user.Email);
                // No fallar la operación completa si solo el email falla, pero registrarlo.
                // Considerar si se debe devolver un error específico o si el usuario debe reintentar.
                // Por ahora, se devuelve éxito, pero se registra el fallo del email.
                // Opcionalmente, se podría revertir el guardado del token si el envío de email es crítico.
                return Result.Failure("Ocurrió un error al enviar el correo de restablecimiento. Por favor, inténtelo de nuevo más tarde.");
            }
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe.
                _logger.LogWarning("Intento de restablecimiento de contraseña para correo no existente: {Email}", request.Email);
                return Result.Failure("La solicitud de restablecimiento de contraseña no es válida o ha expirado.");
            }

            // Validar el token almacenado y su expiración
            if (user.PasswordResetToken != request.Token || // Comparar token codificado
                user.PasswordResetTokenExpiration == null ||
                user.PasswordResetTokenExpiration.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("Token de restablecimiento inválido o expirado para el usuario {UserId}", user.Id);
                return Result.Failure("El enlace de restablecimiento de contraseña no es válido o ha expirado.");
            }

            // Decodificar el token para Identity
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(request.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var resetPassResult = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
            if (resetPassResult.Succeeded)
            {
                user.PasswordResetToken = null; // Limpiar el token después de un uso exitoso
                user.PasswordResetTokenExpiration = null;
                await _userManager.UpdateAsync(user); // Guardar los cambios (token limpiado)
                _logger.LogInformation("Contraseña restablecida exitosamente para el usuario {UserId}", user.Id);
                return Result.Success();
            }

            _logger.LogWarning("Error al restablecer contraseña para el usuario {UserId}. Errores: {Errors}", user.Id, string.Join(", ", resetPassResult.Errors.Select(e => e.Description)));
            return Result.Failure(resetPassResult.Errors.Select(e => e.Description));
        }
    }
}
