using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GestMantIA.Core.Entities;

/// <summary>
/// Representa un permiso en el sistema que puede ser asignado a roles.
/// Los permisos definen acciones específicas que pueden realizarse en el sistema.
/// </summary>
public class Permiso : BaseEntity
{
    /// <summary>
    /// Nombre único del permiso (ej: "Usuarios.Crear", "Roles.Eliminar")
    /// </summary>
    public string Nombre { get; set; } = null!;
    
    /// <summary>
    /// Descripción detallada del permiso
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Módulo al que pertenece el permiso (ej: "Usuarios", "Roles", "Equipos")
    /// </summary>
    public string Modulo { get; set; } = null!;
    
    // Relaciones
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    
    // Propiedades de navegación
    [NotMapped]
    public ICollection<Rol> Roles => RolPermisos.Select(rp => rp.Rol).ToList();
}
