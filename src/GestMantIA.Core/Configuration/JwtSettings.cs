namespace GestMantIA.Core.Configuration;

/// <summary>
/// Configuración para JWT (JSON Web Tokens)
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Clave secreta para firmar los tokens JWT
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Emisor del token
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Audiencia del token
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Tiempo de expiración del token en días
    /// </summary>
    public int ExpireDays { get; set; } = 7;
    
    /// <summary>
    /// Tiempo de expiración del token de refresco en días
    /// </summary>
    public int RefreshTokenExpireDays { get; set; } = 30;
}
