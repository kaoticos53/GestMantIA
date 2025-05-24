using GestMantIA.Core.Identity.Entities;
using GestMantIA.Infrastructure.Data.Configurations.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestMantIA.Core.Identity.Interfaces;

namespace GestMantIA.Infrastructure.Data
{
    /// <summary>
    /// Contexto de base de datos para la autenticación y autorización.
    /// Hereda de IdentityDbContext para integrar con ASP.NET Core Identity.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para las entidades de identidad
        public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
        public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
        public DbSet<ApplicationPermission> ApplicationPermissions => Set<ApplicationPermission>();
        public DbSet<ApplicationUserRole> ApplicationUserRoles => Set<ApplicationUserRole>();
        public DbSet<ApplicationRolePermission> ApplicationRolePermissions => Set<ApplicationRolePermission>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        
        // DbSets para seguridad
        public DbSet<SecurityLog> SecurityLogs => Set<SecurityLog>();
        public DbSet<SecurityNotification> SecurityNotifications => Set<SecurityNotification>();
        public DbSet<SecurityAlert> SecurityAlerts => Set<SecurityAlert>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configuraciones personalizadas
            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationRoleConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationPermissionConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationRolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

            // Configurar el esquema por defecto para las tablas de Identity
            modelBuilder.HasDefaultSchema("identity");

            // Personalizar los nombres de las tablas de Identity
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("Users");
                
                // Relaciones con las entidades de seguridad
                b.HasMany<SecurityLog>()
                    .WithOne(x => x.User)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                b.HasMany<SecurityNotification>()
                    .WithOne(x => x.User)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                b.ToTable("Roles");
            });
            
            // Configuración de índices para las entidades de seguridad
            modelBuilder.Entity<SecurityLog>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.Timestamp);
                entity.Property(e => e.AdditionalData).HasColumnType("jsonb");
            });
            
            modelBuilder.Entity<SecurityNotification>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.NotificationType);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);
            });
            
            modelBuilder.Entity<SecurityAlert>(entity =>
            {
                entity.HasIndex(e => e.Severity);
                entity.HasIndex(e => e.IsResolved);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ResolvedById);
            });

            modelBuilder.Entity<ApplicationUserRole>(b =>
            {
                b.ToTable("UserRoles");
            });

            modelBuilder.Entity<ApplicationPermission>(b =>
            {
                b.ToTable("Permissions");
            });

            modelBuilder.Entity<ApplicationRolePermission>(b =>
            {
                b.ToTable("RolePermissions");
            });

            modelBuilder.Entity<RefreshToken>(b =>
            {
                b.ToTable("RefreshTokens");
            });
        }
    }
}
