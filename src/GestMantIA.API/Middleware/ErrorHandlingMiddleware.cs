using System.Net;
using System.Text.Json;

namespace GestMantIA.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error no controlado en el pipeline.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Considerar no exponer exception.Message directamente en producción por seguridad.
            var responsePayload = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Ocurrió un error interno en el servidor. Por favor, intente más tarde.",
                // DetailedError = exception.Message // Descomentar para desarrollo si es necesario
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(responsePayload, options);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
