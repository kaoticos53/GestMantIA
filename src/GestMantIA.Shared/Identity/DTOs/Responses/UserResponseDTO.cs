using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs.Responses; // Mantendré el namespace por ahora

/// <summary>
/// Data Transfer Object para la respuesta de operaciones de usuario, incluyendo detalles.
/// </summary>
public record UserResponseDTO
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Nombre de usuario.
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    public required string UserName { get; init; }

    /// <summary>
    /// Dirección de correo electrónico del usuario.
    /// </summary>
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    public required string Email { get; init; }

    /// <summary>
    /// Indica si el correo electrónico del usuario ha sido confirmado.
    /// </summary>
    public bool EmailConfirmed { get; init; }

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Apellidos del usuario.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Número de teléfono del usuario.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Indica si el número de teléfono está confirmado.
    /// </summary>
    public bool PhoneNumberConfirmed { get; init; }

    /// <summary>
    /// Fecha de registro del usuario.
    /// </summary>
    public DateTime DateRegistered { get; init; }
    
    /// <summary>
    /// Indica si el usuario está activo.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si la autenticación de dos factores está habilitada para el usuario.
    /// </summary>
    public bool TwoFactorEnabled { get; init; }

    /// <summary>
    /// Indica si el usuario está actualmente bloqueado.
    /// </summary>
    public bool IsLockedOut { get; init; }

    /// <summary>
    /// Fecha y hora hasta la cual el usuario está bloqueado (si aplica).
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; init; }


    /// <summary>
    /// Fecha de la última actualización del usuario.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Lista de nombres de roles asignados al usuario.
    /// </summary>
    public ICollection<string> Roles { get; init; } = new List<string>();

    /// <summary>
    /// Lista de claims del usuario.
    /// </summary>
    public IDictionary<string, string> Claims { get; init; } = new Dictionary<string, string>();
}
