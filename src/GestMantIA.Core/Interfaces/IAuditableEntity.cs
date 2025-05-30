namespace GestMantIA.Core.Interfaces
{
    /// <summary>
    /// Define las propiedades de auditoría para las entidades.
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Fecha de creación del registro.
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de la última modificación del registro.
        /// </summary>
        DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Indica si el registro está marcado como eliminado (borrado lógico).
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Fecha en que se marcó como eliminado.
        /// </summary>
        DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Identificador del usuario que marcó como eliminado.
        /// </summary>
        Guid? DeletedBy { get; set; }
    }
}
