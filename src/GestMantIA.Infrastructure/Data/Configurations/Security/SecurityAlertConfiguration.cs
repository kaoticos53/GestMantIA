using GestMantIA.Core.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Security
{
    public class SecurityAlertConfiguration : IEntityTypeConfiguration<SecurityAlert>
    {
        public void Configure(EntityTypeBuilder<SecurityAlert> builder)
        {
            builder.ToTable("SecurityAlerts", "security");
            // La clave primaria Id se hereda de BaseEntity.

            builder.HasIndex(e => e.Severity);
            builder.HasIndex(e => e.IsResolved);
            builder.HasIndex(e => e.CreatedAt);
            builder.HasIndex(e => e.ResolvedById); // Índice para la FK a ApplicationUser

            // Configuración de la relación con ApplicationUser (ResolvedBy)
            // Asumiendo que SecurityAlert tiene una propiedad `ResolvedBy` de tipo ApplicationUser
            // y una FK `ResolvedById` de tipo Guid?
            // Si ApplicationUser no tiene una colección de SecurityAlerts resueltas por él,
            // la relación es unidireccional desde SecurityAlert.
            builder.HasOne(e => e.ResolvedBy)
                .WithMany() // Si ApplicationUser no tiene una colección de SecurityAlerts resueltas
                .HasForeignKey(e => e.ResolvedById)
                .OnDelete(DeleteBehavior.SetNull); // Para no eliminar la alerta si se elimina el usuario que la resolvió
        }
    }
}
