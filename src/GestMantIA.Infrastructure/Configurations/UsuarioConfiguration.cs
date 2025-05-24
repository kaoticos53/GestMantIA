using GestMantIA.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Configurations;

/// <summary>
/// Configuración de la entidad Usuario para Entity Framework Core.
/// </summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        // Configuración de la tabla
        builder.ToTable("Usuarios");

        // Configuración de la clave primaria
        builder.HasKey(u => u.Id);

        // Configuración de propiedades
        builder.Property(u => u.NombreUsuario)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasColumnType("bytea");

        builder.Property(u => u.PasswordSalt)
            .IsRequired()
            .HasColumnType("bytea");

        builder.Property(u => u.Nombres)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Apellidos)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Telefono)
            .HasMaxLength(20);

            
        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(255);
            
        builder.Property(u => u.EmailConfirmado)
            .HasDefaultValue(false);
            
        builder.Property(u => u.IntentosFallidos)
            .HasDefaultValue(0);
            
        builder.Property(u => u.BloqueadoHasta)
            .HasDefaultValue(false);
            
        // Configuración de índices
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        builder.HasIndex(u => u.NombreUsuario)
            .IsUnique();
            
        // Configuración de valores por defecto
        builder.Property(u => u.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(u => u.EstaEliminado)
            .HasDefaultValue(false);
    }
}
