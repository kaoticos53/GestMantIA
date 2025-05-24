using System;
using System.IO;
using System.Threading.Tasks;
using GestMantIA.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GestMantIA.Infrastructure.Services.Email
{
    /// <summary>
    /// Implementación de IEmailService para desarrollo que registra los correos en lugar de enviarlos.
    /// </summary>
    public class DevelopmentEmailService : IEmailService
    {
        private readonly ILogger<DevelopmentEmailService> _logger;
        private readonly string _emailsDirectory;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DevelopmentEmailService"/>
        /// </summary>
        /// <param name="logger">Logger para registrar información.</param>
        public DevelopmentEmailService(ILogger<DevelopmentEmailService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Directorio para guardar los correos simulados
            _emailsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TempEmails");
            
            // Asegurarse de que el directorio existe
            if (!Directory.Exists(_emailsDirectory))
            {
                Directory.CreateDirectory(_emailsDirectory);
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendEmailAsync(string to, string subject, string message, bool isHtml = true)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileName = $"email_{to}_{timestamp}.html";
                var filePath = Path.Combine(_emailsDirectory, fileName);

                var emailContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>{subject}</title>
                    <meta charset=""utf-8"" />
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ margin-top: 20px; padding: 10px; text-align: center; font-size: 0.8em; color: #6c757d; }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h2>{subject}</h2>
                        </div>
                        <div class=""content"">
                            {message}
                        </div>
                        <div class=""footer"">
                            <p>Este es un correo de prueba. En un entorno de producción, este correo se habría enviado a {to}.</p>
                            <p>Hora de envío: {DateTime.Now}</p>
                        </div>
                    </div>
                </body>
                </html>";

                await File.WriteAllTextAsync(filePath, emailContent);
                
                _logger.LogInformation("Correo simulado guardado en: {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el correo simulado para {To}", to);
                return false;
            }
        }
    }
}
