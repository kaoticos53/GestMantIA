using GestMantIA.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Configurations;

/// <summary>
/// Configuración de la entidad UsuarioRol para Entity Framework Core.
/// </summary>
public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        // Configuración de la tabla
        builder.ToTable("UsuarioRoles");

        // Configuración de la clave primaria compuesta
        builder.HasKey(ur => new { ur.UsuarioId, ur.RolId });

        // Configuración de relaciones
        builder.HasOne(ur => ur.Usuario)
            .WithMany(u => u.UsuarioRoles)
            .HasForeignKey(ur => ur.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Rol)
            .WithMany(r => r.UsuarioRoles)
            .HasForeignKey(ur => ur.RolId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configuración de propiedades
        builder.Property(ur => ur.FechaAsignacion)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(ur => ur.AsignadoPor)
            .HasMaxLength(100);
            
        // Configuración de valores por defecto
        builder.Property(ur => ur.EstaEliminado)
            .HasDefaultValue(false);
    }
}
