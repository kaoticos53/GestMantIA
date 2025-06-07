using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs;

/// <summary>
/// Data Transfer Object para el resumen de un usuario, usado en listas.
/// </summary>
public record UserSummaryDto
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
    /// Indica si el usuario está actualmente bloqueado.
    /// </summary>
    public bool IsLockedOut { get; init; }
}
