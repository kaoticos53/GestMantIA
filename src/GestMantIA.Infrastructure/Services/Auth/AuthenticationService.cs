using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestMantIA.Core.Identity.DTOs;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Identity.Results;
using GestMantIA.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GestMantIA.Core.Interfaces;

namespace GestMantIA.Infrastructure.Services.Auth
{
    /// <summary>
    /// Implementación del servicio de autenticación de usuarios.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AuthenticationService"/>
        /// </summary>
        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ILogger<AuthenticationService> logger,
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        /// <inheritdoc />
        public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request, string ipAddress)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(ipAddress))
                throw new ArgumentNullException(nameof(ipAddress));

            _logger.LogInformation("Intento de autenticación para el usuario: {Username}", request.UsernameOrEmail);

            // Buscar al usuario por nombre de usuario o correo electrónico
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.NormalizedUserName == request.UsernameOrEmail.ToUpper() || 
                                       u.NormalizedEmail == request.UsernameOrEmail.ToUpper());

            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado: {Username}", request.UsernameOrEmail);
                return AuthenticationResult.Failure(new[] { "Usuario o contraseña incorrectos" }, "Error de autenticación");
            }

            // Verificar si el usuario está bloqueado
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Cuenta bloqueada temporalmente para el usuario: {UserId}", user.Id);
                return AuthenticationResult.Failure(
                    new[] { "Su cuenta ha sido bloqueada temporalmente. Por favor, inténtelo de nuevo más tarde." },
                    "Cuenta bloqueada");
            }

            // Intentar iniciar sesión
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Cuenta bloqueada temporalmente para el usuario: {UserId}", user.Id);
                    return AuthenticationResult.Failure(
                        new[] { "Su cuenta ha sido bloqueada temporalmente debido a múltiples intentos fallidos." },
                        "Cuenta bloqueada temporalmente");
                }

                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("Inicio de sesión no permitido para el usuario: {UserId}", user.Id);
                    return AuthenticationResult.Failure(
                        new[] { "Debe confirmar su dirección de correo electrónico antes de iniciar sesión." },
                        "Correo electrónico no verificado");
                }

                _logger.LogWarning("Contraseña incorrecta para el usuario: {UserId}", user.Id);
                return AuthenticationResult.Failure(
                    new[] { "Usuario o contraseña incorrectos" },
                    "Error de autenticación");
            }

            // Generar tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, ipAddress);

            _logger.LogInformation("Autenticación exitosa para el usuario: {UserId}", user.Id);

            return AuthenticationResult.Success(
                accessToken,
                refreshToken.Token,
                refreshToken.Expires,
                new UserInfo
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = (await _userManager.GetRolesAsync(user)).ToList()
                },
                "Autenticación exitosa");
        }

        // Resto de los métodos de la interfaz...
        public async Task<AuthenticationResult> RefreshTokenAsync(string token, string ipAddress)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("El token no puede estar vacío.", nameof(token));

            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null)
                return AuthenticationResult.Failure(new[] { "Token de actualización no válido" }, "Error al renovar el token");

            // Validar el token
            if (refreshToken.IsExpired)
            {
                _logger.LogWarning("Intento de usar un token de actualización expirado: {TokenId}", refreshToken.Id);
                return AuthenticationResult.Failure(new[] { "Token de actualización expirado" }, "Sesión expirada");
            }


            if (refreshToken.IsRevoked)
            {
                _logger.LogWarning("Intento de usar un token de actualización revocado: {TokenId}", refreshToken.Id);
                await RevokeDescendantRefreshTokens(refreshToken, ipAddress, $"Intento de reutilización de token ancestro: {refreshToken.Token}");
                return AuthenticationResult.Failure(new[] { "Token de actualización revocado" }, "Sesión inválida");
            }

            // Rotar el token
            var newRefreshToken = await RotateRefreshToken(refreshToken, ipAddress);
            await _context.SaveChangesAsync();

            // Generar nuevo token de acceso
            var accessToken = _tokenService.GenerateAccessToken(refreshToken.User);

            _logger.LogInformation("Token renovado para el usuario: {UserId}", refreshToken.UserId);

            return AuthenticationResult.Success(
                accessToken,
                newRefreshToken.Token,
                newRefreshToken.Expires,
                new UserInfo
                {
                    Id = refreshToken.User.Id.ToString(),
                    Username = refreshToken.User.Username,
                    Email = refreshToken.User.Email,
                    FullName = refreshToken.User.FullName,
                    Roles = (await _userManager.GetRolesAsync(refreshToken.User)).ToList()
                },
                "Token renovado exitosamente");
        }

        public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("El token no puede estar vacío.", nameof(token));

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null)
                return false;

            if (!refreshToken.IsActive)
                return false;

            // Revocar el token y todos los tokens descendientes
            await RevokeRefreshToken(refreshToken, ipAddress, "Token revocado por el usuario");
            await _context.SaveChangesAsync();

            _logger.LogInformation("Token revocado: {TokenId}", refreshToken.Id);
            return true;
        }

        public async Task<RegisterResult> RegisterAsync(RegisterRequest request, string origin)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger.LogInformation("Intento de registro para el correo: {Email}", request.Email);

            // Verificar si el correo ya está en uso
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                _logger.LogWarning("Intento de registro con correo ya existente: {Email}", request.Email);
                return RegisterResult.Failure(new[] { "El correo electrónico ya está en uso" }, "Error en el registro");
            }

            // Verificar si el nombre de usuario ya está en uso
            if (await _userManager.FindByNameAsync(request.Username) != null)
            {
                _logger.LogWarning("Intento de registro con nombre de usuario ya existente: {Username}", request.Username);
                return RegisterResult.Failure(new[] { "El nombre de usuario ya está en uso" }, "Error en el registro");
            }

            // Crear el nuevo usuario
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = false // El correo debe ser confirmado
            };

            // Crear el usuario
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Error al crear el usuario: {Errors}", string.Join(", ", errors));
                return RegisterResult.Failure(errors, "Error al crear el usuario");
            }

            // Asignar rol de usuario por defecto
            await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("Usuario registrado exitosamente: {UserId}", user.Id);

            // Generar token de verificación de correo
            var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Enviar correo de verificación (implementación simulada)
            await SendVerificationEmail(user, verificationToken, origin);

            return RegisterResult.Success(
                user.Id.ToString(),
                requiresEmailConfirmation: true,
                "Registro exitoso. Por favor revise su correo electrónico para verificar su cuenta.");
        }

        public async Task<VerifyEmailResult> VerifyEmailAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("El token no puede estar vacío.", nameof(token));

            // El token debe estar en formato: userId|verificationToken
            var parts = token.Split('|');
            if (parts.Length != 2)
            {
                _logger.LogWarning("Formato de token de verificación inválido");
                return VerifyEmailResult.Failure(new[] { "Token de verificación inválido" }, "Error al verificar el correo");
            }

            var userId = parts[0];
            var verificationToken = parts[1];

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para la verificación de correo: {UserId}", userId);
                return VerifyEmailResult.Failure(new[] { "Usuario no encontrado" }, "Error al verificar el correo");
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("El correo ya ha sido verificado anteriormente: {UserId}", user.Id);
                return VerifyEmailResult.Success(user.Id.ToString(), "El correo electrónico ya ha sido verificado");
            }

            var result = await _userManager.ConfirmEmailAsync(user, verificationToken);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Error al verificar el correo: {Errors}", string.Join(", ", errors));
                return VerifyEmailResult.Failure(errors, "Error al verificar el correo");
            }

            _logger.LogInformation("Correo verificado exitosamente: {UserId}", user.Id);
            return VerifyEmailResult.Success(user.Id.ToString(), "Correo electrónico verificado exitosamente");
        }

        #region Métodos auxiliares privados

        private async Task<RefreshToken> RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(refreshToken.User, ipAddress);
            
            // Revocar el token actual
            await RevokeRefreshToken(
                refreshToken, 
                ipAddress, 
                "Reemplazado por nuevo token", 
                newRefreshToken.Token);

            return newRefreshToken;
        }

        private async Task RevokeDescendantRefreshTokens(RefreshToken refreshToken, string ipAddress, string reason)
        {
            // Verificar si el token tiene un token de reemplazo
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(x => x.Token == refreshToken.ReplacedByToken);

                if (childToken != null && childToken.IsActive)
                {
                    await RevokeRefreshToken(childToken, ipAddress, reason);
                }
                else if (childToken != null && !childToken.IsActive)
                {
                    await RevokeDescendantRefreshTokens(childToken, ipAddress, reason);
                }
            }
        }


        private async Task RevokeRefreshToken(RefreshToken token, string ipAddress, string? reason = null, string? replacedByToken = null)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
                
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
            
            await _context.SaveChangesAsync();
        }

        private async Task SendVerificationEmail(ApplicationUser user, string verificationToken, string origin)
        {
            try
            {
                // En una implementación real, aquí se enviaría un correo electrónico
                // con un enlace que incluya el token de verificación
                var verificationUrl = $"{origin}/api/auth/verify-email?token={Uri.EscapeDataString($"{user.Id}|{verificationToken}")}";
                
                _logger.LogInformation("URL de verificación para {Email}: {VerificationUrl}", user.Email, verificationUrl);
                
                // Implementación simulada del envío de correo
                var emailSent = await _emailService.SendEmailAsync(
                    to: user.Email,
                    subject: "Verifica tu correo electrónico",
                    message: $"Por favor, haz clic en el siguiente enlace para verificar tu dirección de correo electrónico: <a href='{verificationUrl}'>Verificar correo</a>");

                if (!emailSent)
                {
                    _logger.LogError("No se pudo enviar el correo de verificación a {Email}", user.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el correo de verificación a {Email}", user.Email);
                // No lanzamos la excepción para no interrumpir el flujo de registro
            }
        }

        #endregion

        #region Password Reset

        /// <inheritdoc />
        public async Task<OperationResult> ForgotPasswordAsync(string email, string origin)
        {
            if (string.IsNullOrWhiteSpace(email))
                return OperationResult.Failed("El correo electrónico es requerido");

            _logger.LogInformation("Solicitud de restablecimiento de contraseña para {Email}", email);

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                
                // Siempre devolvemos éxito para no revelar si el correo existe o no
                if (user == null)
                {
                    _logger.LogInformation("No se encontró ninguna cuenta con el correo {Email}", email);
                    return OperationResult.Succeeded("Si el correo electrónico existe en nuestro sistema, se ha enviado un enlace para restablecer la contraseña.");
                }

                // Generar token de restablecimiento
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // Codificar el token para URL
                var encodedToken = Uri.EscapeDataString($"{user.Id}|{resetToken}");
                
                // Crear URL de restablecimiento
                var resetUrl = $"{origin}/reset-password?token={encodedToken}";
                
                // Enviar correo electrónico con el enlace de restablecimiento
                await SendPasswordResetEmailAsync(user, resetUrl);

                _logger.LogInformation("Se ha enviado un correo de restablecimiento a {Email}", email);
                
                return OperationResult.Succeeded("Si el correo electrónico existe en nuestro sistema, se ha enviado un enlace para restablecer la contraseña.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud de restablecimiento de contraseña para {Email}", email);
                return OperationResult.Failed("Ocurrió un error al procesar la solicitud de restablecimiento de contraseña. Por favor, inténtelo de nuevo más tarde.");
            }
        }

        /// <inheritdoc />
        public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (request == null)
                return OperationResult.Failed("La solicitud no puede estar vacía");

            _logger.LogInformation("Intento de restablecimiento de contraseña para el token: {Token}", request.Token);

            try
            {
                // Decodificar el token
                var tokenParts = Uri.UnescapeDataString(request.Token).Split('|');
                if (tokenParts.Length != 2)
                {
                    _logger.LogWarning("Formato de token inválido");
                    return OperationResult.Failed("El enlace de restablecimiento no es válido o ha expirado.");
                }

                var userId = tokenParts[0];
                var resetToken = tokenParts[1];

                // Buscar al usuario
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado para el ID: {UserId}", userId);
                    return OperationResult.Failed("El enlace de restablecimiento no es válido o ha expirado.");
                }

                // Restablecer la contraseña
                var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
                
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Error al restablecer la contraseña: {Errors}", string.Join("; ", errors));
                    return OperationResult.Failed(errors);
                }

                _logger.LogInformation("Contraseña restablecida exitosamente para el usuario: {UserId}", user.Id);
                
                // Opcional: Enviar notificación de cambio de contraseña
                await SendPasswordChangedNotificationAsync(user);
                
                return OperationResult.Succeeded("La contraseña se ha restablecido correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer la contraseña");
                return OperationResult.Failed("Ocurrió un error al restablecer la contraseña. Por favor, inténtelo de nuevo.");
            }
        }

        private async Task SendPasswordResetEmailAsync(ApplicationUser user, string resetUrl)
        {
            try
            {
                // En una implementación real, aquí se enviaría un correo electrónico
                // con el enlace de restablecimiento de contraseña
                _logger.LogInformation("URL de restablecimiento de contraseña para {Email}: {ResetUrl}", user.Email, resetUrl);
                
                // Implementación simulada del envío de correo
                var emailSent = await _emailService.SendEmailAsync(
                    to: user.Email,
                    subject: "Restablecer su contraseña",
                    message: $"Para restablecer su contraseña, haga clic en el siguiente enlace: <a href='{resetUrl}'>Restablecer contraseña</a>");

                if (!emailSent)
                {
                    _logger.LogError("No se pudo enviar el correo de restablecimiento a {Email}", user.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el correo de restablecimiento a {Email}", user.Email);
                // No lanzamos la excepción para no interrumpir el flujo
            }
        }

        private async Task SendPasswordChangedNotificationAsync(ApplicationUser user)
        {
            try
            {
                // En una implementación real, aquí se enviaría una notificación
                // informando que la contraseña ha sido cambiada
                _logger.LogInformation("Notificación de cambio de contraseña para {Email}", user.Email);
                
                // Implementación simulada del envío de notificación
                var emailSent = await _emailService.SendEmailAsync(
                    to: user.Email,
                    subject: "Contraseña actualizada",
                    message: "Su contraseña ha sido actualizada correctamente. Si no realizó este cambio, póngase en contacto con el soporte técnico de inmediato.");

                if (!emailSent)
                {
                    _logger.LogError("No se pudo enviar la notificación de cambio de contraseña a {Email}", user.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar la notificación de cambio de contraseña a {Email}", user.Email);
                // No lanzamos la excepción para no interrumpir el flujo
            }
        }

        #region Two-Factor Authentication

        /// <inheritdoc />
        public async Task<TwoFactorSetupResult> GenerateTwoFactorSetupAsync(string userId, string email)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new TwoFactorSetupResult("Usuario no encontrado.");

                // Generar una clave secreta
                await _userManager.ResetAuthenticatorKeyAsync(user);
                var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                
                if (string.IsNullOrEmpty(unformattedKey))
                {
                    _logger.LogError("No se pudo generar la clave de autenticación para el usuario {UserId}", userId);
                    return new TwoFactorSetupResult("No se pudo generar la clave de autenticación.");
                }

                var sharedKey = FormatKey(unformattedKey);

                // Generar código QR
                var authenticatorUri = GenerateQrCode(email, unformattedKey);
                
                return new TwoFactorSetupResult(sharedKey, authenticatorUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar la configuración de autenticación de dos factores para el usuario {UserId}", userId);
                return new TwoFactorSetupResult("Ocurrió un error al generar la configuración de autenticación de dos factores.");
            }
        }

        /// <inheritdoc />
        public async Task<OperationResult> EnableTwoFactorAsync(string userId, string code)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return OperationResult.Failed("Usuario no encontrado.");

                // Verificar el código
                var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                    user, 
                    _userManager.Options.Tokens.AuthenticatorTokenProvider, 
                    code);

                if (!is2faTokenValid)
                    return OperationResult.Failed("Código de verificación inválido.");

                // Habilitar 2FA
                var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Error al habilitar 2FA para el usuario {UserId}: {Errors}", userId, errors);
                    return OperationResult.Failed("Error al habilitar la autenticación de dos factores.");
                }

                _logger.LogInformation("Autenticación de dos factores habilitada para el usuario {UserId}", userId);
                return OperationResult.Succeeded("Autenticación de dos factores habilitada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al habilitar la autenticación de dos factores para el usuario {UserId}", userId);
                return OperationResult.Failed("Ocurrió un error al habilitar la autenticación de dos factores.");
            }
        }

        /// <inheritdoc />
        public async Task<OperationResult> DisableTwoFactorAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return OperationResult.Failed("Usuario no encontrado.");

                var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Error al deshabilitar 2FA para el usuario {UserId}: {Errors}", userId, errors);
                    return OperationResult.Failed("Error al deshabilitar la autenticación de dos factores.");
                }

                // Limpiar la clave de autenticación
                await _userManager.ResetAuthenticatorKeyAsync(user);
                
                _logger.LogInformation("Autenticación de dos factores deshabilitada para el usuario {UserId}", userId);
                return OperationResult.Succeeded("Autenticación de dos factores deshabilitada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deshabilitar la autenticación de dos factores para el usuario {UserId}", userId);
                return OperationResult.Failed("Ocurrió un error al deshabilitar la autenticación de dos factores.");
            }
        }

        /// <inheritdoc />
        public async Task<bool> VerifyTwoFactorTokenAsync(string userId, string code)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                return await _userManager.VerifyTwoFactorTokenAsync(
                    user, 
                    _userManager.Options.Tokens.AuthenticatorTokenProvider, 
                    code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el token 2FA para el usuario {UserId}", userId);
                return false;
            }
        }

        private string GenerateQrCode(string email, string unformattedKey)
        {
            try
            {
                var encodedIssuer = Uri.EscapeDataString("GestMantIA");
                var encodedAccount = Uri.EscapeDataString(email);
                var encodedSecret = Uri.EscapeDataString(unformattedKey);
                
                return string.Format(
                    AuthenticatorUriFormat,
                    encodedIssuer,
                    encodedAccount,
                    encodedSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el código QR para el correo {Email}", email);
                return string.Empty;
            }
        }

        private string FormatKey(string unformattedKey)
        {
            if (string.IsNullOrEmpty(unformattedKey))
                return string.Empty;
                
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        #endregion

        #endregion
    }
}
