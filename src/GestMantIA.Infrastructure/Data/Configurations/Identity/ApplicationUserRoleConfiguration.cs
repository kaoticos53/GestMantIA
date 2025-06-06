using GestMantIA.Core.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Identity
{
    /// <summary>
    /// Configuración de Entity Framework para la entidad ApplicationUserRole.
    /// </summary>
    public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
        {
            // Nombre de la tabla
            builder.ToTable("UserRoles", "identity");

            // Clave primaria compuesta
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            // Configuración de relaciones
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de tipos de columna
            builder.Property(ur => ur.UserId)
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(ur => ur.RoleId)
                .HasColumnType("uuid")
                .IsRequired();
        }
    }
}
