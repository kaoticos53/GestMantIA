using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs.Requests;

/// <summary>
/// DTO para la creación de un nuevo usuario.
/// </summary>
public record CreateUserDTO
{
    /// <summary>
    /// Nombre de usuario (debe ser único).
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public required string UserName { get; init; }

    /// <summary>
    /// Correo electrónico del usuario (debe ser único).
    /// </summary>
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres.")]
    public required string Email { get; init; }

    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    public required string Password { get; init; }

    /// <summary>
    /// Confirmación de la contraseña.
    /// </summary>
    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "La contraseña y la confirmación no coinciden.")]
    public required string ConfirmPassword { get; init; }

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
    /// Lista de nombres de roles a asignar al usuario.
    /// </summary>
    public ICollection<string> Roles { get; init; } = new List<string>();

    /// <summary>
    /// Indica si se requiere que el usuario confirme su correo electrónico después del registro.
    /// Por defecto es true.
    /// </summary>
    public bool RequireEmailConfirmation { get; init; } = true;
}
