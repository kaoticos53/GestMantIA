using GestMantIA.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Configurations;

/// <summary>
/// Configuración de la entidad Rol para Entity Framework Core.
/// </summary>
public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        // Configuración de la tabla
        builder.ToTable("Roles");

        // Configuración de la clave primaria
        builder.HasKey(r => r.Id);

        // Configuración de propiedades
        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Descripcion)
            .HasMaxLength(500);
            
        builder.Property(r => r.EsRolPorDefecto)
            .HasDefaultValue(false);
            
        // Configuración de índices
        builder.HasIndex(r => r.Nombre)
            .IsUnique();
            
        // Configuración de valores por defecto
        builder.Property(r => r.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(r => r.EstaEliminado)
            .HasDefaultValue(false);
    }
}
