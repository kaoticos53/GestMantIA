namespace GestMantIA.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de envío de correos electrónicos.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un correo electrónico de forma asíncrona.
        /// </summary>
        /// <param name="to">Dirección de correo electrónico del destinatario.</param>
        /// <param name="subject">Asunto del correo electrónico.</param>
        /// <param name="message">Contenido del mensaje en formato HTML.</param>
        /// <param name="isHtml">Indica si el mensaje está en formato HTML. Por defecto es true.</param>
        /// <returns>True si el correo se envió correctamente; de lo contrario, false.</returns>
        Task<bool> SendEmailAsync(string to, string subject, string message, bool isHtml = true);
    }
}
