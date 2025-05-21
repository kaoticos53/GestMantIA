using System.Text;
using GestMantIA.Core.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GestMantIA.API.Extensions;

/// <summary>
/// Métodos de extensión para configurar la autenticación JWT
/// </summary>
public static class JwtServiceCollectionExtensions
{
    /// <summary>
    /// Agrega y configura la autenticación JWT a la colección de servicios
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <returns>IServiceCollection para encadenamiento</returns>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configurar JwtSettings desde appsettings.json
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        if (string.IsNullOrEmpty(jwtSettings?.Key) || 
            string.IsNullOrEmpty(jwtSettings.Issuer) || 
            string.IsNullOrEmpty(jwtSettings.Audience))
        {
            throw new InvalidOperationException("La configuración de JWT no es válida. Asegúrese de configurar Jwt:Key, Jwt:Issuer y Jwt:Audience en appsettings.json");
        }

        // Configurar autenticación JWT
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = !configuration.GetValue<bool>("AllowInsecureHttp");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero // Elimina el tiempo de gracia del token
            };
        });

        // Configurar autorización
        services.AddAuthorization(options =>
        {
            // Políticas de autorización personalizadas pueden ir aquí
            // Ejemplo:
            // options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
        });

        return services;
    }
}
