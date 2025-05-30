using GestMantIA.Core.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Identity
{
    /// <summary>
    /// Configuración de Entity Framework para la entidad ApplicationPermission.
    /// </summary>
    public class ApplicationPermissionConfiguration : IEntityTypeConfiguration<ApplicationPermission>
    {
        public void Configure(EntityTypeBuilder<ApplicationPermission> builder)
        {
            // Nombre de la tabla
            builder.ToTable("Permissions", "identity");

            // Configuración de propiedades
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(p => p.Name)
                .IsUnique();

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(50);

            // Configuración de ClaimType y ClaimValue
            builder.Property(p => p.ClaimType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.ClaimValue)
                .IsRequired()
                .HasMaxLength(100);

            // Configuración de eliminación en cascada
            builder.HasMany(p => p.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
