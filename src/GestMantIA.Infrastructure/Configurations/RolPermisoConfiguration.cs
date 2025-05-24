using GestMantIA.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Configurations;

/// <summary>
/// Configuración de la entidad RolPermiso para Entity Framework Core.
/// </summary>
public class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        // Configuración de la tabla
        builder.ToTable("RolPermisos");

        // Configuración de la clave primaria compuesta
        builder.HasKey(rp => new { rp.RolId, rp.PermisoId });

        // Configuración de relaciones
        builder.HasOne(rp => rp.Rol)
            .WithMany(r => r.RolPermisos)
            .HasForeignKey(rp => rp.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permiso)
            .WithMany(p => p.RolPermisos)
            .HasForeignKey(rp => rp.PermisoId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configuración de propiedades
        builder.Property(rp => rp.FechaAsignacion)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(rp => rp.AsignadoPor)
            .HasMaxLength(100);
            
        // Configuración de valores por defecto
        builder.Property(rp => rp.EstaEliminado)
            .HasDefaultValue(false);
    }
}
