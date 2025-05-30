namespace GestMantIA.Core.Identity.Results
{
    /// <summary>
    /// Resultado de una operación de verificación de correo electrónico.
    /// </summary>
    public class VerifyEmailResult : AuthResult
    {
        public VerifyEmailResult()
        {
            UserId = string.Empty;
            Succeeded = false;
            Message = "Operación completada";
            Errors = new List<string>();
            EmailVerified = false;
        }
        /// <summary>
        /// Identificador del usuario verificado.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Indica si el correo electrónico fue verificado correctamente.
        /// </summary>
        public bool EmailVerified { get; set; }

        /// <summary>
        /// Crea un resultado de verificación de correo electrónico exitoso.
        /// </summary>
        /// <param name="userId">Identificador del usuario verificado.</param>
        /// <param name="message">Mensaje descriptivo opcional.</param>
        /// <returns>Instancia de VerifyEmailResult con EmailVerified en true.</returns>
        public static VerifyEmailResult Success(string userId, string? message = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("El ID de usuario no puede estar vacío.", nameof(userId));
            }

            var result = AuthResult.Success<VerifyEmailResult>(message ?? "Correo electrónico verificado exitosamente");
            result.UserId = userId;
            result.EmailVerified = true;
            return result;
        }

        /// <summary>
        /// Crea un resultado de verificación de correo electrónico fallido.
        /// </summary>
        /// <param name="errors">Lista de errores.</param>
        /// <param name="message">Mensaje descriptivo opcional.</param>
        /// <returns>Instancia de VerifyEmailResult con Succeeded en false.</returns>
        public static VerifyEmailResult Failure(IEnumerable<string>? errors, string? message = null)
        {
            var result = AuthResult.Failure<VerifyEmailResult>(errors, message ?? "Error al verificar el correo electrónico");
            result.EmailVerified = false;
            return result;
        }

        // No es necesario el método obsoleto ya que es redundante con el método anterior
        // Se puede llamar directamente al método con parámetros opcionales
    }
}
