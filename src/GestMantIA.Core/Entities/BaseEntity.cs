using System;

namespace GestMantIA.Core.Entities;

/// <summary>
/// Clase base para todas las entidades del dominio.
/// Incluye propiedades comunes como Id, FechaCreación y FechaActualización.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha de la última actualización del registro
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }
    
    /// <summary>
    /// Indica si el registro está marcado como eliminado (borrado lógico)
    /// </summary>
    public bool EstaEliminado { get; set; }
}
