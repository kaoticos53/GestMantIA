using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Identity
{
    public class IdentityRoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
        {
            builder.ToTable("RoleClaims", "identity");
            // La clave primaria Id es configurada por IdentityDbContext de forma predeterminada.
            // Las relaciones también son manejadas por IdentityDbContext y ApplicationRoleConfiguration.
        }
    }
}
