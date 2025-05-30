using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs
{
    /// <summary>
    /// DTO para la gestión de roles.
    /// </summary>
    public class RoleDTO
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="RoleDTO"/>
        /// </summary>
        public RoleDTO()
        {
            Id = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            Permissions = new List<string>();
        }

        /// <summary>
        /// Identificador único del rol.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nombre del rol.
        /// </summary>
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede tener más de 50 caracteres")]
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol.
        /// </summary>
        [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
        public string Description { get; set; }

        /// <summary>
        /// Lista de permisos asignados al rol.
        /// </summary>
        public ICollection<string> Permissions { get; set; } = new List<string>();

        /// <summary>
        /// Fecha de creación del rol.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de la última actualización del rol.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO para la creación de un nuevo rol.
    /// </summary>
    public class CreateRoleDTO
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="CreateRoleDTO"/>
        /// </summary>
        public CreateRoleDTO()
        {
            Name = string.Empty;
            Description = string.Empty;
            Permissions = new List<string>();
        }

        /// <summary>
        /// Nombre del rol.
        /// </summary>
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede tener más de 50 caracteres")]
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol.
        /// </summary>
        [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
        public string Description { get; set; }

        /// <summary>
        /// Lista de permisos asignados al rol.
        /// </summary>
        public ICollection<string> Permissions { get; set; }
    }

    /// <summary>
    /// DTO para la actualización de un rol existente.
    /// </summary>
    public class UpdateRoleDTO
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UpdateRoleDTO"/>
        /// </summary>
        public UpdateRoleDTO()
        {
            Id = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            Permissions = new List<string>();
        }

        /// <summary>
        /// Identificador único del rol.
        /// </summary>
        [Required(ErrorMessage = "El ID del rol es obligatorio")]
        public string Id { get; set; }

        /// <summary>
        /// Nombre del rol.
        /// </summary>
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede tener más de 50 caracteres")]
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol.
        /// </summary>
        [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
        public string Description { get; set; }

        /// <summary>
        /// Lista de permisos asignados al rol.
        /// </summary>
        public ICollection<string> Permissions { get; set; }
    }
}
