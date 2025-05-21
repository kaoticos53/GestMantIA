using GestMantIA.API.Extensions;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configuración de OpenAPI (Swagger)
builder.Services.AddOpenApi();

// Configuración de validación personalizada
builder.Services.AddCustomValidation();

// Configuración de autenticación JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(
                "http://localhost:5001",
                "https://localhost:5002")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Necesario para enviar cookies de autenticación
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint de salud para health checks
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();
