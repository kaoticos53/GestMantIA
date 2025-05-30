using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any; // For OpenApiString
using Microsoft.OpenApi.Models;

namespace GestMantIA.API.Extensions
{
    public static class PresentationServiceExtensions
    {
        public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add services to the container for Controllers
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    // Personalizar la respuesta de validación para mantener consistencia
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var result = new BadRequestObjectResult(context.ModelState);
                        result.ContentTypes.Add("application/problem+json");
                        return result;
                    };
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Mantener el mismo formato de propiedades
                    options.JsonSerializerOptions.WriteIndented = true;
                })
                .AddApplicationPart(Assembly.GetExecutingAssembly()) // Asegurar que todos los controladores del ensamblado actual se descubran
                .AddControllersAsServices(); // Registrar controladores como servicios para DI

            // Configuración de CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:3000",  // React
                            "http://localhost:4200",  // Angular
                            "http://localhost:5000",  // .NET
                            "https://localhost:5001", // .NET con HTTPS
                            "http://localhost:6080",  // Puerto de desarrollo HTTP
                            "https://localhost:6001"  // Puerto de desarrollo HTTPS
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()
                          .WithExposedHeaders("Token-Expired");
                });
            });

            // Configuración de Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "GestMantIA API",
                    Version = "v1",
                    Description = "API para el sistema de Gestión de Mantenimiento Inteligente Asistido.",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Soporte GestMantIA",
                        Email = "soporte@gestmantia.com",
                        Url = new Uri("https://gestmantia.com/contact")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Licencia de Uso",
                        Url = new Uri("https://example.com/license")
                    }
                });

                // Añadir seguridad JWT a Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Encabezado de autorización JWT usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http, // Usar el tipo Http para JWT Bearer
                    Scheme = "bearer", // ¡Importante: debe ser en minúsculas!
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "bearer", // Debe coincidir con el scheme definido arriba
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new string[]{}
                    }
                }); // Esto permite que Swagger envíe el JWT automáticamente en los endpoints protegidos

                // Incluir comentarios XML para documentación de Swagger
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Filtro para descripciones de tags (controladores)
                // options.DocumentFilter<TagDescriptionsDocumentFilter>(); // Comentado temporalmente: Archivo TagDescriptionsDocumentFilter.cs no encontrado

                // Personalización para mostrar enums como strings
                options.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string", Example = new OpenApiString("00:00:00") });
            });

            // Configuración de HealthChecks
            services.AddHealthChecks()
                .AddDbContextCheck<Infrastructure.Data.ApplicationDbContext>("Database"); // Asumiendo que ApplicationDbContext está en Infrastructure.Data

            // API Versioning (si se añade en el futuro)
            // services.AddApiVersioning(options =>
            // {
            //    options.DefaultApiVersion = new ApiVersion(1, 0);
            //    options.AssumeDefaultVersionWhenUnspecified = true;
            //    options.ReportApiVersions = true;
            // });

            return services;
        }
    }
}
