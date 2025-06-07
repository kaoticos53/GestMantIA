using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs;

/// <summary>
/// Data Transfer Object para la actualización de un usuario existente.
/// </summary>
public record UpdateUserRequest
{
    /// <summary>
    /// Identificador único del usuario a actualizar.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Nuevo nombre de usuario. Usar con precaución si es un identificador primario.
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public required string UserName { get; init; }

    /// <summary>
    /// Nueva dirección de correo electrónico del usuario.
    /// </summary>
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres.")]
    public required string Email { get; init; }

    /// <summary>
    /// Lista completa de nombres de roles a asignar al usuario.
    /// El backend interpretará esto como el estado deseado de los roles para el usuario.
    /// </summary>
    public ICollection<string> Roles { get; init; } = new List<string>();

    /// <summary>
    /// Opcional: Permite establecer explícitamente el estado de bloqueo del usuario.
    /// true para bloquear, false para desbloquear. Si es null, no se modifica el estado de bloqueo.
    /// </summary>
    public bool? SetLockoutStatus { get; init; }
}
