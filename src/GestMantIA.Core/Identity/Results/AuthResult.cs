namespace GestMantIA.Core.Identity.Results
{
    /// <summary>
    /// Clase base para los resultados de autenticación.
    /// </summary>
    public abstract class AuthResult
    {
        protected AuthResult()
        {
            Succeeded = false;
            Message = "Operación completada";
            Errors = new List<string>();
        }
        /// <summary>
        /// Indica si la operación se completó con éxito.
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// Mensaje descriptivo sobre el resultado de la operación.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Lista de errores que pudieron ocurrir durante la operación.
        /// </summary>
        public IEnumerable<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Crea un resultado exitoso.
        /// </summary>
        /// <param name="message">Mensaje descriptivo opcional.</param>
        /// <returns>Instancia de AuthResult con Succeeded en true.</returns>
        public static T Success<T>(string? message = null) where T : AuthResult, new()
        {
            var result = new T
            {
                Succeeded = true,
                Message = message ?? "Operación completada con éxito"
            };

            // Asegurarse de que la lista de errores esté inicializada
            if (result.Errors == null)
            {
                result.Errors = new List<string>();
            }

            return result;
        }

        /// <summary>
        /// Crea un resultado fallido.
        /// </summary>
        /// <param name="errors">Lista de errores.</param>
        /// <param name="message">Mensaje descriptivo opcional.</param>
        /// <returns>Instancia de AuthResult con Succeeded en false.</returns>
        public static T Failure<T>(IEnumerable<string>? errors, string? message = null) where T : AuthResult, new()
        {
            var result = new T
            {
                Succeeded = false,
                Message = message ?? "Error en la operación"
            };

            // Inicializar la lista de errores si es nula
            if (result.Errors == null)
            {
                result.Errors = new List<string>();
            }

            // Agregar los errores si existen
            if (errors != null)
            {
                foreach (var error in errors)
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        result.Errors = result.Errors.Append(error);
                    }
                }
            }

            return result;
        }
    }
}
