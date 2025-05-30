using GestMantIA.Core.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Identity
{
    /// <summary>
    /// Configuración de Entity Framework para la entidad ApplicationUser.
    /// </summary>
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Nombre de la tabla
            builder.ToTable("Users", "identity");

            // Clave primaria
            builder.HasKey(u => u.Id);

            // Configuración de propiedades
            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(u => u.UserName)
                .IsUnique();

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(u => u.FullName)
                .HasMaxLength(100);

            // Configuración de índices adicionales (los de UserName y Email ya están definidos como únicos junto a sus propiedades)
            builder.HasIndex(u => u.NormalizedUserName)
                .IsUnique();

            builder.HasIndex(u => u.NormalizedEmail)
                .IsUnique();

            builder.HasIndex(u => u.PhoneNumber); // Índice para búsquedas por número de teléfono

            // Configuración de eliminación en cascada
            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones con otras entidades de Identity (heredadas)
            builder.HasMany<Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>>()
                .WithOne()
                .HasForeignKey(ut => ut.UserId)
                .IsRequired();

            builder.HasMany<Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>>()
                .WithOne()
                .HasForeignKey(ul => ul.UserId)
                .IsRequired();

            builder.HasMany<Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>>()
                .WithOne()
                .HasForeignKey(uc => uc.UserId)
                .IsRequired();

            // Relaciones con entidades de Seguridad
            builder.HasMany<SecurityLog>()
                .WithOne(sl => sl.User)
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany<SecurityNotification>()
                .WithOne(sn => sn.User)
                .HasForeignKey(sn => sn.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
