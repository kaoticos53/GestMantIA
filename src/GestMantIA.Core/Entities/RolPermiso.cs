namespace GestMantIA.Core.Entities;

/// <summary>
/// Entidad de unión para la relación muchos a muchos entre Rol y Permiso.
/// Permite agregar metadatos adicionales a la relación si es necesario en el futuro.
/// </summary>
public class RolPermiso : BaseEntity
{
    // Claves foráneas
    public Guid RolId { get; set; }
    public Guid PermisoId { get; set; }
    
    // Propiedades de navegación
    public Rol Rol { get; set; } = null!;
    public Permiso Permiso { get; set; } = null!;
    
    // Metadatos adicionales de la relación
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public string? AsignadoPor { get; set; }
}
