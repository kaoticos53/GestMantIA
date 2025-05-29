using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestMantIA.Infrastructure.Data.Configurations.Identity
{
    public class IdentityUserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
        {
            builder.ToTable("UserTokens", "identity");
            // Las claves primarias (UserId, LoginProvider, Name) son configuradas por IdentityDbContext.
            // Las relaciones también son manejadas por IdentityDbContext y ApplicationUserConfiguration.
        }
    }
}
