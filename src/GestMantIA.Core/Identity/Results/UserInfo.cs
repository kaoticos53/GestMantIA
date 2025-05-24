using System;
using System.Collections.Generic;

namespace GestMantIA.Core.Identity.Results
{
    /// <summary>
    /// Información básica del usuario para incluir en la respuesta de autenticación.
    /// </summary>
    public class UserInfo
    {
        public UserInfo()
        {
            Id = string.Empty;
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Roles = new List<string>();
        }

        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Dirección de correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Indica si el correo electrónico del usuario ha sido verificado.
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Lista de roles asignados al usuario.
        /// </summary>
        public ICollection<string> Roles { get; set; }
    }
}
