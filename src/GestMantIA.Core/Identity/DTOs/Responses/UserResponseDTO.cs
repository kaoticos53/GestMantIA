using System;
using System.Collections.Generic;

namespace GestMantIA.Core.Identity.DTOs.Responses
{
    /// <summary>
    /// DTO para la respuesta de operaciones de usuario.
    /// </summary>
    public class UserResponseDTO
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserResponseDTO"/>
        /// </summary>
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserResponseDTO"/>
        /// </summary>
        public UserResponseDTO()
        {
            Id = string.Empty;
            Username = string.Empty;
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
            LockoutReason = string.Empty;
            Roles = new List<string>();
            Claims = new Dictionary<string, string>();
        }

        /// <summary>
        /// ID único del usuario.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nombre de usuario para el inicio de sesión.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Indica si el correo electrónico está confirmado.
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Apellidos del usuario.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Indica si el número de teléfono está confirmado.
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Indica si el usuario está activo
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indica si la autenticación en dos factores está habilitada
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// Fecha de creación del usuario
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de la última actualización del usuario
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Lista de roles del usuario
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Lista de claims del usuario.
        /// </summary>
        public IDictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Indica si el usuario está bloqueado
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Fecha de fin del bloqueo (si está bloqueado)
        /// </summary>
        public DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Razón del bloqueo (si está bloqueado)
        /// </summary>
        public string? LockoutReason { get; set; }
    }
}
