namespace GestMantIA.Core.Entities;

/// <summary>
/// Entidad de unión para la relación muchos a muchos entre Usuario y Rol.
/// Permite agregar metadatos adicionales a la relación si es necesario en el futuro.
/// </summary>
public class UsuarioRol : BaseEntity
{
    // Claves foráneas
    public Guid UsuarioId { get; set; }
    public Guid RolId { get; set; }
    
    // Propiedades de navegación
    public Usuario Usuario { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
    
    // Metadatos adicionales de la relación
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public string? AsignadoPor { get; set; }
}
