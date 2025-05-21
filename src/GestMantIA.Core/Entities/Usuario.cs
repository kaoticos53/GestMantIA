using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GestMantIA.Core.Entities;

/// <summary>
/// Representa un usuario del sistema con sus datos de autenticación y perfil.
/// </summary>
public class Usuario : BaseEntity
{
    // Datos de autenticación
    public string NombreUsuario { get; set; } = null!;
    public string Email { get; set; } = null!;
    public byte[] PasswordHash { get; set; } = null!;
    public byte[] PasswordSalt { get; set; } = null!;
    public string? RefreshToken { get; set; }
    public DateTime? TokenCreado { get; set; }
    public DateTime? TokenExpira { get; set; }
    
    // Datos del perfil
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string? Telefono { get; set; }
    public string? AvatarUrl { get; set; }
    public bool EmailConfirmado { get; set; }
    public DateTime? FechaUltimoAcceso { get; set; }
    public int IntentosFallidos { get; set; }
    public bool BloqueadoHasta { get; set; }
    
    // Relaciones
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    
    // Propiedades de navegación
    [NotMapped]
    public ICollection<Rol> Roles => UsuarioRoles.Select(ur => ur.Rol).ToList();
    
    // Propiedades calculadas
    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}
