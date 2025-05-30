using GestMantIA.Core.Entities.Identity; // Para UserProfile
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestMantIA.Infrastructure.Data
{
    /// <summary>
    /// Contexto de la base de datos principal de la aplicación.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
        IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ApplicationDbContext"/>
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto.</param>

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ApplicationDbContext"/>
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para las entidades de Identity
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<ApplicationPermission> Permissions { get; set; } = null!;
        public DbSet<ApplicationRolePermission> RolePermissions { get; set; } = null!;

        // DbSets para las entidades de seguridad
        public DbSet<SecurityLog> SecurityLogs { get; set; } = null!;
        public DbSet<SecurityNotification> SecurityNotifications { get; set; } = null!;
        public DbSet<SecurityAlert> SecurityAlerts { get; set; } = null!;

        // DbSet para UserProfile
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;

        // Configuraciones específicas para las entidades de negocio

        #region Auditoría

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ProcessAuditEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ProcessAuditEntities();
            return base.SaveChanges();
        }

        private void ProcessAuditEntities()
        {
            var entries = ChangeTracker.Entries<IAuditableEntity>();
            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = null; // Asegurar que UpdatedAt sea null al crear
                        entry.Entity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        if (entry.Entity.IsDeleted && entry.OriginalValues.GetValue<bool>(nameof(IAuditableEntity.IsDeleted)) == false)
                        {
                            entry.Entity.DeletedAt = DateTime.UtcNow;
                            // Aquí podríamos intentar establecer DeletedBy si tuviéramos acceso al ID del usuario actual.
                            // Por ahora, se deja en null o se manejará en el servicio.
                        }
                        else if (!entry.Entity.IsDeleted && entry.OriginalValues.GetValue<bool>(nameof(IAuditableEntity.IsDeleted)) == true)
                        {
                            // Si se está "des-eliminando" una entidad, limpiar DeletedAt y DeletedBy
                            entry.Entity.DeletedAt = null;
                            entry.Entity.DeletedBy = null;
                        }
                        break;
                        // No es necesario manejar EntityState.Deleted para el borrado lógico automático aquí,
                        // ya que el borrado lógico se maneja cambiando IsDeleted a true y luego guardando (lo que es una Modificación).
                }
            }
        }

        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Aplicar todas las configuraciones de IEntityTypeConfiguration<T> en este ensamblado
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configurar datos iniciales si es necesario
            // ConfigurarDatosIniciales(builder);
        }


        #region Implementación de IUnitOfWork

        /// <inheritdoc />
        public IRepository<T> Repository<T>() where T : class
        {
            throw new NotImplementedException("El repositorio genérico Repository<T> no está implementado en la nueva arquitectura vertical slice.");
        }

        /// <inheritdoc />
        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await SaveChangesAsync(cancellationToken);
        }


        #endregion

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
        }


    }
}
