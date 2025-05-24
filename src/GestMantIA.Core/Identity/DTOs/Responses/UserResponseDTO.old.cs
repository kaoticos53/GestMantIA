using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Core.Identity.DTOs
{
    /// <summary>
    /// DTO para la respuesta de operaciones de usuario.
    /// </summary>
    public class UserResponseDTO
    {
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
        /// Nombre completo del usuario (nombre + apellidos).
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Indica si el número de teléfono está confirmado.
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Indica si el usuario está activo en el sistema.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indica si el usuario está bloqueado.
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Fecha hasta la que el usuario está bloqueado (si aplica).
        /// </summary>
        public DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Razón del bloqueo (si está bloqueado).
        /// </summary>
        public string LockoutReason { get; set; }

        /// <summary>
        /// Indica si el usuario tiene habilitada la autenticación de dos factores.
        /// </summary>
        public bool TwoFactorEnabled { get; set; }
        /// <summary>
        /// Fecha de creación del usuario.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Fecha de la última modificación del usuario.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        /// <summary>
        /// Lista de roles asignados al usuario.
        /// </summary>
        public ICollection<string> Roles { get; set; } = new List<string>();
    }
}
