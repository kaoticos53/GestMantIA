using GestMantIA.Core.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Identity
{
    /// <summary>
    /// Configuración de Entity Framework para la entidad RefreshToken.
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            // Nombre de la tabla
            builder.ToTable("RefreshTokens", "identity");

            // Clave primaria
            builder.HasKey(rt => rt.Id);

            // Configuración del ID
            builder.Property(rt => rt.Id)
                .ValueGeneratedOnAdd();

            // Configuración de propiedades
            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(rt => rt.Expires)
                .IsRequired();

            builder.Property(rt => rt.CreatedByIp)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(rt => rt.RevokedByIp)
                .HasMaxLength(50);

            builder.Property(rt => rt.ReplacedByToken)
                .HasMaxLength(500);

            // Configuración de índices
            builder.HasIndex(rt => rt.Token)
                .IsUnique();

            // Índice para la relación con User
            builder.HasIndex(rt => rt.UserId);

            builder.HasIndex(rt => rt.CreatedAt);

            // Configuración de relaciones
            builder.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Configuración del tipo de columna para UserId
            builder.Property(rt => rt.UserId)
                .HasColumnType("uuid")
                .IsRequired();

            // Configuración de fechas
            builder.Property(rt => rt.CreatedAt)
                .IsRequired();

            builder.Property(rt => rt.Expires)
                .IsRequired();

            // Configuración de propiedades booleanas
            builder.Property(rt => rt.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            //builder.Property(rt => rt.IsExpired)
            //    .IsRequired()
            //    .HasDefaultValue(false);

            //builder.Property(rt => rt.IsActive)
            //    .IsRequired()
            //    .HasDefaultValue(true);
        }
    }
}
