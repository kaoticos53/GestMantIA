using GestMantIA.Application.Features.UserManagement.Services;
using GestMantIA.Application.Interfaces;
using GestMantIA.Core.Identity.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GestMantIA.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            // El parámetro 'configuration' no se usa actualmente, pero se mantiene para futuras necesidades.
            // Registrar servicios de la capa de aplicación
            services.AddScoped<IUserService, ApplicationUserService>();
            services.AddScoped<IUserProfileService, ApplicationUserProfileService>();

            // Aquí se podrían registrar otros servicios de aplicación, MediatR, AutoMapper, FluentValidation, etc.
            // Ejemplo con AutoMapper (si se usa en esta capa para mapeos específicos de aplicación):
            // services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Ejemplo con MediatR (si se usa para CQRS en la capa de aplicación):
            // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Ejemplo con FluentValidation (si se usa para validaciones en la capa de aplicación):
            // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
