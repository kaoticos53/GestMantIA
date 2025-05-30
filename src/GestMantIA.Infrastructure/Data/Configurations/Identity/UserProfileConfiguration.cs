using GestMantIA.Core.Entities.Identity; // Para UserProfile
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Identity
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            // Nombre de la tabla
            builder.ToTable("UserProfiles", "identity"); // Esquema "identity"

            // Clave primaria (heredada de BaseEntity)
            builder.HasKey(up => up.Id);

            // Propiedades
            builder.Property(up => up.UserId)
                .IsRequired();

            builder.Property(up => up.FirstName)
                .HasMaxLength(100);

            builder.Property(up => up.LastName)
                .HasMaxLength(100);

            builder.Property(up => up.AvatarUrl)
                .HasMaxLength(500);

            builder.Property(up => up.Bio)
                .HasMaxLength(1000);

            builder.Property(up => up.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(up => up.StreetAddress).HasMaxLength(255);
            builder.Property(up => up.City).HasMaxLength(100);
            builder.Property(up => up.StateProvince).HasMaxLength(100);
            builder.Property(up => up.PostalCode).HasMaxLength(20);
            builder.Property(up => up.Country).HasMaxLength(100);

            // Relaciones
            builder.HasOne(up => up.User)
                .WithOne() // Asumiendo que ApplicationUser no tiene una propiedad de navegación directa a UserProfile
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade); // O Restrict, según la política de borrado

            // Índices
            builder.HasIndex(up => up.UserId)
                .IsUnique(); // Un usuario solo puede tener un perfil
        }
    }
}
