namespace GestMantIA.Core.Identity.Results
{
    /// <summary>
    /// Resultado de una operación de registro de usuario.
    /// </summary>
    public class RegisterResult : AuthResult
    {
        public RegisterResult()
        {
            UserId = Guid.Empty;
            Succeeded = false;
            Message = "Operación completada";
            Errors = new List<string>();
        }

        /// <summary>
        /// Identificador del usuario recién registrado.
        /// </summary>
        public Guid UserId { get; set; } // Fixed property name to match usage

        /// <summary>
        /// Indica si se requiere confirmación de correo electrónico.
        /// </summary>
        public bool RequiresEmailConfirmation { get; set; }

        /// <summary>
        /// Crea un resultado de registro exitoso.
        /// </summary>
        /// <param name="userId">Identificador del usuario registrado.</param>
        /// <param name="requiresEmailConfirmation">Indica si se requiere confirmación de correo electrónico.</param>
        /// <param name="message">Mensaje descriptivo opcional.</param>
        /// <returns>Instancia de RegisterResult con los datos del registro.</returns>
        public static RegisterResult Success(Guid userId, bool requiresEmailConfirmation = false, string? message = null)
        {
            if (userId == Guid.Empty) // Fixed validation for Guid.Empty
            {
                throw new ArgumentException("El ID de usuario no puede estar vacío.", nameof(userId));
            }

            var result = AuthResult.Success<RegisterResult>(message ?? "Registro exitoso");
            result.UserId = userId;
            result.RequiresEmailConfirmation = requiresEmailConfirmation;
            return result;
        }

        /// <summary>
        /// Crea un resultado de registro fallido.
        /// </summary>
        /// <param name="errors">Lista de errores.</param>
        /// <param name="message">Mensaje descriptivo opcional.</param>
        /// <returns>Instancia de RegisterResult con Succeeded en false.</returns>
        public static RegisterResult Failure(IEnumerable<string>? errors, string? message = null)
        {
            return AuthResult.Failure<RegisterResult>(errors, message ?? "Error en el registro");
        }
    }
}
