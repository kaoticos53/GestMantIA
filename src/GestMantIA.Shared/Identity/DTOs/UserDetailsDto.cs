using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs;

/// <summary>
/// Data Transfer Object para los detalles de un usuario.
/// </summary>
public record UserDetailsDto
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
    public bool IsEmailConfirmed { get; init; }

    /// <summary>
    /// Indica si el usuario está actualmente bloqueado.
    /// </summary>
    public bool IsLockedOut { get; init; }

    /// <summary>
    /// Fecha y hora hasta la cual el usuario está bloqueado (si aplica).
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; init; }

    /// <summary>
    /// Lista de nombres de roles asignados al usuario.
    /// </summary>
    public ICollection<string> Roles { get; init; } = new List<string>();
}
