using GestMantIA.Core.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Identity
{
    /// <summary>
    /// Configuración de Entity Framework para la entidad ApplicationRolePermission.
    /// </summary>
    public class ApplicationRolePermissionConfiguration : IEntityTypeConfiguration<ApplicationRolePermission>
    {
        public void Configure(EntityTypeBuilder<ApplicationRolePermission> builder)
        {
            // Nombre de la tabla
            builder.ToTable("RolePermissions", "identity");

            // Clave primaria compuesta
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            
            // Índice para la relación con Role
            builder.HasIndex(rp => rp.RoleId);
            
            // Índice para la relación con Permission
            builder.HasIndex(rp => rp.PermissionId);

            // Configuración de relaciones
            builder.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configuración de tipos de columna
            builder.Property(rp => rp.RoleId)
                .HasColumnType("uuid")
                .IsRequired();
                
            builder.Property(rp => rp.PermissionId)
                .HasColumnType("uuid")
                .IsRequired();
        }
    }
}
