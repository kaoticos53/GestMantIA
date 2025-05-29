using GestMantIA.Core.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Security
{
    public class SecurityLogConfiguration : IEntityTypeConfiguration<SecurityLog>
    {
        public void Configure(EntityTypeBuilder<SecurityLog> builder)
        {
            builder.ToTable("SecurityLogs", "security");
            // La clave primaria Id se hereda de BaseEntity y se configura automáticamente.
            
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.EventType);
            builder.HasIndex(e => e.Timestamp);
            
            builder.Property(e => e.AdditionalData).HasColumnType("jsonb");

            // La relación con ApplicationUser ya está definida en ApplicationUserConfiguration
            // builder.HasOne(e => e.User)
            //     .WithMany() // ApplicationUser ya tiene HasMany<SecurityLog>()
            //     .HasForeignKey(e => e.UserId)
            //     .OnDelete(DeleteBehavior.Cascade); // Coincide con ApplicationUserConfiguration
        }
    }
}
