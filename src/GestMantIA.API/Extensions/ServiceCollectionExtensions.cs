using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GestMantIA.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomValidation(this IServiceCollection services)
        {
            // Configuración de validación personalizada
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Configuración de FluentValidation
            services.AddFluentValidationAutoValidation(config =>
            {
                // Deshabilitar la validación automática de MVC
                config.DisableDataAnnotationsValidation = true;
                
                // Configuración adicional de validación
                config.ImplicitlyValidateChildProperties = true;
                config.ImplicitlyValidateRootCollectionElements = true;
            });

            // Registrar todos los validadores del ensamblado actual
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
