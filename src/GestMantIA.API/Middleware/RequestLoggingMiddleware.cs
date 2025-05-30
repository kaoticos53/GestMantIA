namespace GestMantIA.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Loguear información de la solicitud
            _logger.LogInformation(
                "Solicitud Entrante: {Method} {Path} desde {RemoteIpAddress} | User-Agent: {UserAgent}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress,
                context.Request.Headers["User-Agent"].FirstOrDefault() ?? "N/A");

            // Copiar el stream original de la respuesta para poder restaurarlo después de leerlo
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Loguear información de la respuesta
            _logger.LogInformation(
                "Respuesta Saliente: {StatusCode} | Content-Type: {ContentType} | Content-Length: {ContentLength}",
                context.Response.StatusCode,
                context.Response.ContentType ?? "N/A",
                context.Response.ContentLength ?? -1);

            // Copiar el contenido del MemoryStream al stream original de la respuesta
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}
