using GestMantIA.Core.Interfaces;

namespace GestMantIA.Core.Identity.Entities
{
    /// <summary>
    /// Clase base para todas las entidades del dominio.
    /// </summary>
    public abstract class BaseEntity : IAuditableEntity
    {
        /// <summary>
        /// Identificador único de la entidad.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Fecha de creación del registro.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de la última modificación del registro.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }


        /// <summary>
        /// Indica si el registro está marcado como eliminado (borrado lógico).
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Fecha en que se marcó como eliminado.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Identificador del usuario que marcó como eliminado.
        /// </summary>
        public Guid? DeletedBy { get; set; }
    }
}
