using GestMantIA.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting; // For IWebHostEnvironment and IsDevelopment()
using Microsoft.AspNetCore.Diagnostics.HealthChecks; // For HealthCheckOptions
using Microsoft.AspNetCore.Http; // For PathString

namespace GestMantIA.API.Extensions
{
    public static class PipelineConfigurationExtensions
    {
        public static WebApplication ConfigureApiPipeline(this WebApplication app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => 
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GestMantIA API v1");
                    c.RoutePrefix = string.Empty; // Servir Swagger UI en la raíz de la API
                });
            }
            else
            {
                // Middleware de manejo de errores global para producción
                app.UseMiddleware<ErrorHandlingMiddleware>();
                app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            }

            // Middleware para registrar todas las solicitudes entrantes
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseHttpsRedirection();

            app.UseStaticFiles(); // Habilitar el servicio de archivos estáticos

            app.UseRouting();

            // Configurar CORS - debe ir después de UseRouting y antes de UseAuthorization/UseEndpoints
            app.UseCors(); // Asume que la política por defecto ya está configurada en AddPresentationServices

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true, // Incluir todos los health checks
                ResponseWriter = async (context, report) =>
                {
                    var result = System.Text.Json.JsonSerializer.Serialize(
                        new { 
                            status = report.Status.ToString(), 
                            checks = report.Entries.Select(e => new { 
                                name = e.Key, 
                                status = e.Value.Status.ToString(), 
                                description = e.Value.Description 
                            }),
                            totalDuration = report.TotalDuration.TotalMilliseconds
                        });
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(result);
                }
            });

            return app;
        }
    }
}
