namespace GestMantIA.Core.Identity.Results
{
    /// <summary>
    /// Representa el resultado de una operación genérica.
    /// </summary>
    public class OperationResult
    {
        private readonly List<string> _errors = new List<string>();

        /// <summary>
        /// Indica si la operación fue exitosa.
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// Indica si la operación fue exitosa (propiedad de solo lectura para compatibilidad).
        /// </summary>
        public bool Success => Succeeded;

        /// <summary>
        /// Mensaje descriptivo del resultado de la operación.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Lista de errores ocurridos durante la operación.
        /// </summary>
        public IReadOnlyCollection<string> Errors => _errors.AsReadOnly();

        /// <summary>
        /// Crea un resultado de operación exitoso.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        /// <returns>Instancia de <see cref="OperationResult"/> con éxito.</returns>
        public static OperationResult CreateSuccess(string? message = null)
        {
            return new OperationResult
            {
                Succeeded = true,
                Message = message ?? "Operación completada con éxito"
            };
        }

        /// <summary>
        /// Crea un resultado de operación fallido (obsoleto, usar CreateFailure en su lugar).
        /// </summary>
        [Obsolete("Usar el método CreateFailure en su lugar.")]
        public static OperationResult Failed(string message)
        {
            var result = new OperationResult
            {
                Succeeded = false,
                Message = message ?? "La operación ha fallado."
            };
            
            if (message != null)
            {
                result._errors.Add(message);
            }
            
            return result;
        }

        /// <summary>
        /// Crea un resultado de operación fallido con múltiples errores.
        /// </summary>
        /// <param name="errors">Lista de mensajes de error.</param>
        /// <param name="message">Mensaje descriptivo del error.</param>
        /// <returns>Instancia de <see cref="OperationResult"/> con errores.</returns>
        public static OperationResult CreateFailure(IEnumerable<string> errors, string? message = null)
        {
            var result = new OperationResult
            {
                Succeeded = false,
                Message = message ?? "La operación ha fallado con uno o más errores."
            };
            result._errors.AddRange(errors);
            return result;
        }

        /// <summary>
        /// Crea un resultado de operación fallido con múltiples errores (obsoleto, usar CreateFailure en su lugar).
        /// </summary>
        [Obsolete("Usar el método CreateFailure en su lugar.")]
        public static OperationResult Failed(IEnumerable<string> errors, string? message = null) =>
            CreateFailure(errors, message);

        /// <summary>
        /// Agrega un error al resultado.
        /// </summary>
        /// <param name="error">Mensaje de error a agregar.</param>
        public void AddError(string error)
        {
            Succeeded = false;
            _errors.Add(error);

            // Actualizar el mensaje para incluir el último error
            if (string.IsNullOrEmpty(Message) || Message == "Operación completada con éxito")
            {
                Message = error;
            }
            else if (!Message.Contains(error))
            {
                Message += "; " + error;
            }
        }
    }
}
