using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs;

/// <summary>
/// Data Transfer Object para la creación de un nuevo usuario.
/// </summary>
public record CreateUserRequest
{
    /// <summary>
    /// Nombre de usuario deseado.
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public required string UserName { get; init; }

    /// <summary>
    /// Dirección de correo electrónico del nuevo usuario.
    /// </summary>
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres.")]
    public required string Email { get; init; }

    /// <summary>
    /// Contraseña para el nuevo usuario.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    // Nota: Las reglas de complejidad de contraseña (mayúsculas, minúsculas, números, símbolos)
    // se validan típicamente en el servidor (ASP.NET Core Identity options).
    public required string Password { get; init; }

    /// <summary>
    /// Confirmación de la contraseña.
    /// </summary>
    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public required string ConfirmPassword { get; init; }

    /// <summary>
    /// Lista de nombres de roles a asignar al nuevo usuario.
    /// </summary>
    public ICollection<string> Roles { get; init; } = new List<string>();
}
