using GestMantIA.Core.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Security
{
    public class SecurityNotificationConfiguration : IEntityTypeConfiguration<SecurityNotification>
    {
        public void Configure(EntityTypeBuilder<SecurityNotification> builder)
        {
            builder.ToTable("SecurityNotifications", "security");
            // La clave primaria Id se hereda de BaseEntity.

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.NotificationType);
            builder.HasIndex(e => e.IsRead);
            builder.HasIndex(e => e.CreatedAt);

            // La relación con ApplicationUser ya está definida en ApplicationUserConfiguration
            // builder.HasOne(e => e.User)
            //     .WithMany() // ApplicationUser ya tiene HasMany<SecurityNotification>()
            //     .HasForeignKey(e => e.UserId)
            //     .OnDelete(DeleteBehavior.Cascade); // Coincide con ApplicationUserConfiguration
        }
    }
}
