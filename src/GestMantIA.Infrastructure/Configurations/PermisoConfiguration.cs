using GestMantIA.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Configurations;

/// <summary>
/// Configuración de la entidad Permiso para Entity Framework Core.
/// </summary>
public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        // Configuración de la tabla
        builder.ToTable("Permisos");

        // Configuración de la clave primaria
        builder.HasKey(p => p.Id);

        // Configuración de propiedades
        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(500);
            
        builder.Property(p => p.Modulo)
            .IsRequired()
            .HasMaxLength(50);
            
        // Configuración de índices
        builder.HasIndex(p => p.Nombre)
            .IsUnique();
            
        // Configuración de valores por defecto
        builder.Property(p => p.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(p => p.EstaEliminado)
            .HasDefaultValue(false);
    }
}
