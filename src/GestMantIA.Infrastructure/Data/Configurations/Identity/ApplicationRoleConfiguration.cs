using GestMantIA.Core.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Identity
{
    /// <summary>
    /// Configuración de Entity Framework para la entidad ApplicationRole.
    /// </summary>
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            // Nombre de la tabla
            builder.ToTable("Roles", "identity");

            // Clave primaria
            builder.HasKey(r => r.Id);

            // Configuración de propiedades
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(r => r.Name)
                .IsUnique();

            builder.Property(r => r.NormalizedName)
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(r => r.Description)
                .HasMaxLength(200);

            // Configuración de índices
            builder.HasIndex(r => r.NormalizedName)
                .IsUnique();

            // Configuración de eliminación en cascada
            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con IdentityRoleClaim<Guid>
            builder.HasMany<Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>>()
                .WithOne()
                .HasForeignKey(rc => rc.RoleId)
                .IsRequired();

            // Configuración del tipo de columna para el ID
            builder.Property(r => r.Id)
                .HasColumnType("uuid");
        }
    }
}
