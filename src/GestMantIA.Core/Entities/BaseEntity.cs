using GestMantIA.Core.Interfaces; // Para IAuditableEntity

namespace GestMantIA.Core.Entities
{
    public abstract class BaseEntity : IAuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Propiedades de IAuditableEntity
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
