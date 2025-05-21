using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GestMantIA.Core.Entities;

/// <summary>
/// Representa un rol en el sistema que agrupa permisos.
/// Los usuarios pueden tener uno o más roles que definen sus permisos en el sistema.
/// </summary>
public class Rol : BaseEntity
{
    /// <summary>
    /// Nombre único del rol (ej: "Administrador", "Técnico", "Consulta")
    /// </summary>
    public string Nombre { get; set; } = null!;
    
    /// <summary>
    /// Descripción del rol y sus funciones
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Indica si este es el rol por defecto para nuevos usuarios
    /// </summary>
    public bool EsRolPorDefecto { get; set; }
    
    // Relaciones
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    
    // Propiedades de navegación
    [NotMapped]
    public ICollection<Usuario> Usuarios => UsuarioRoles.Select(ur => ur.Usuario).ToList();
    [NotMapped]
    public ICollection<Permiso> Permisos => RolPermisos.Select(rp => rp.Permiso).ToList();
}
