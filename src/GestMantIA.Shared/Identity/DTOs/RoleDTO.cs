using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs;

/// <summary>
/// Data Transfer Object para representar un rol del sistema.
/// </summary>
public record RoleDto
{
    /// <summary>
    /// Identificador único del rol.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Nombre del rol.
    /// </summary>
    [Required(ErrorMessage = "El nombre del rol es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre del rol no puede tener más de 50 caracteres")]
    public required string Name { get; init; }

    /// <summary>
    /// Descripción del rol.
    /// </summary>
    [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
    public string? Description { get; init; }

    /// <summary>
    /// Lista de permisos asignados al rol.
    /// (Nota: En el futuro, esto podría evolucionar a un DTO más específico para permisos).
    /// </summary>
    public ICollection<string> Permissions { get; init; } = new List<string>();

    /// <summary>
    /// Fecha de creación del rol. Generalmente establecida por el servidor.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Fecha de la última actualización del rol. Generalmente establecida por el servidor.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
