using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs.Requests;

/// <summary>
/// DTO para la actualización de un usuario existente.
/// </summary>
public record UpdateUserDTO
{
    /// <summary>
    /// Identificador único del usuario a actualizar.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Nuevo nombre de usuario. Si se proporciona, debe ser válido.
    /// </summary>
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Si se proporciona, el nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public string? UserName { get; init; }

    /// <summary>
    /// Nueva dirección de correo electrónico del usuario. Si se proporciona, debe ser válida.
    /// </summary>
    [EmailAddress(ErrorMessage = "Si se proporciona, el formato del correo electrónico no es válido.")]
    [StringLength(100, ErrorMessage = "Si se proporciona, el correo electrónico no puede exceder los 100 caracteres.")]
    public string? Email { get; init; }

    /// <summary>
    /// Nombre del usuario (opcional).
    /// </summary>
    [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
    public string? FirstName { get; init; }

    /// <summary>
    /// Apellidos del usuario (opcional).
    /// </summary>
    [StringLength(100, ErrorMessage = "Los apellidos no pueden tener más de 100 caracteres.")]
    public string? LastName { get; init; }

    /// <summary>
    /// Número de teléfono del usuario (opcional).
    /// </summary>
    [Phone(ErrorMessage = "El formato del número de teléfono no es válido.")]
    [StringLength(20, ErrorMessage = "El número de teléfono no puede exceder los 20 caracteres.")]
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Lista completa de nombres de roles a asignar al usuario.
    /// El backend interpretará esto como el estado deseado de los roles para el usuario.
    /// Si es null, los roles no se modifican. Si es una lista vacía, se eliminan todos los roles.
    /// </summary>
    public ICollection<string>? Roles { get; init; }

    /// <summary>
    /// Opcional: Permite establecer explícitamente el estado de actividad del usuario.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Opcional: Permite establecer explícitamente el estado de confirmación del correo electrónico.
    /// </summary>
    public bool? EmailConfirmed { get; init; }

    /// <summary>
    /// Opcional: Permite establecer explícitamente el estado de confirmación del número de teléfono.
    /// </summary>
    public bool? PhoneNumberConfirmed { get; init; }

    /// <summary>
    /// Opcional: Permite habilitar o deshabilitar la autenticación de dos factores.
    /// </summary>
    public bool? TwoFactorEnabled { get; init; }
    
    /// <summary>
    /// Opcional: Permite establecer explícitamente el estado de bloqueo del usuario.
    /// true para bloquear, false para desbloquear. Si es null, no se modifica el estado de bloqueo.
    /// </summary>
    public bool? SetLockoutStatus { get; init; }
}
