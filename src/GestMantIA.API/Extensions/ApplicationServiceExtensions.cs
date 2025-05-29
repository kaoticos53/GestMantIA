using AutoMapper;
using GestMantIA.Infrastructure.Mappings; // For MappingProfile
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// using MediatR;
// using FluentValidation;
// using System.Reflection;

namespace GestMantIA.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de AutoMapper
            var mapperConfig = MappingProfile.ConfigureAutoMapper(); 
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            // Configuración de MediatR (si se usa en el futuro)
            // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // FluentValidation ya se configura a través de AddCustomValidation() en Program.cs

            // Otros servicios de aplicación específicos de la API

            return services;
        }
    }
}
