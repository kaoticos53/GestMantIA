using FluentValidation;
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
                // Deshabilitar la validación automática de MVC
                options.SuppressModelStateInvalidFilter = true;
            });

            // Registrar todos los validadores del ensamblado actual
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
