using System;
using System.Threading;
using System.Threading.Tasks;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Interfaces;
using GestMantIA.Infrastructure.Data.Configurations;
using GestMantIA.Infrastructure.Data.Configurations.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
        private IDbContextTransaction? _currentTransaction;

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
            var entries = ChangeTracker.Entries<BaseEntity>();
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
                        // No modificar IsDeleted aquí directamente, se asume que se maneja antes de llamar a SaveChanges
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

            // Configuración de índices para mejorar el rendimiento
            ConfigurarIndices(builder);
            
            // Configurar datos iniciales si es necesario
            // ConfigurarDatosIniciales(builder);
        }


        private static void ConfigurarIndices(ModelBuilder modelBuilder)
        {
            // Índices para ApplicationUser
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.NormalizedEmail)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.NormalizedUserName)
                .IsUnique();


            // Índices para ApplicationRole
            modelBuilder.Entity<ApplicationRole>()
                .HasIndex(r => r.NormalizedName)
                .IsUnique();


            // Índices para RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId);


            // Índices para SecurityLog
            modelBuilder.Entity<SecurityLog>()
                .HasIndex(sl => sl.UserId);


            modelBuilder.Entity<SecurityLog>()
                .HasIndex(sl => sl.Timestamp);


            // Índices para SecurityNotification
            modelBuilder.Entity<SecurityNotification>()
                .HasIndex(sn => sn.UserId);


            modelBuilder.Entity<SecurityNotification>()
                .HasIndex(sn => sn.CreatedAt);


            // Índices para SecurityAlert
            modelBuilder.Entity<SecurityAlert>()
                .HasIndex(sa => sa.CreatedAt);
        }

        #region Implementación de IUnitOfWork

        /// <inheritdoc />
        public IRepository<T> Repository<T>() where T : class
        {
            return new Data.Repositories.Repository<T>(this);
        }

        /// <inheritdoc />
        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);
                _currentTransaction?.Commit();
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction != null) // Ensure _currentTransaction is not null before accessing it
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        #endregion

        private async Task DisposeTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public override async ValueTask DisposeAsync()
        {
            await DisposeTransactionAsync();
            await base.DisposeAsync();
        }


    }
}
