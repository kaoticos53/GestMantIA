using System.Collections.Concurrent;
using GestMantIA.Core.Interfaces;
using GestMantIA.Infrastructure.Features.UserManagement.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GestMantIA.Infrastructure.Data
{
    /// <summary>
    /// Implementación del patrón Unit of Work para manejar transacciones y repositorios.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed;
        private IDbContextTransaction? _transaction;
        private readonly ConcurrentDictionary<Type, object> _repositories;
        private GestMantIA.Core.Identity.Interfaces.IUserRepository _userRepository = null!;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UnitOfWork"/>.
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        /// <inheritdoc />
        public IRepository<T> Repository<T>() where T : class
        {
            return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_context));
        }

        public GestMantIA.Core.Identity.Interfaces.IUserRepository UserRepository =>
            _userRepository ??= new UserRepository(_context);

        /// <inheritdoc />
        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No hay una transacción activa para confirmar.");
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No hay una transacción activa para revertir.");
            }

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        /// <summary>
        /// Libera los recursos no administrados que usa el objeto y, de forma opcional, libera los recursos administrados.
        /// </summary>
        /// <param name="disposing">True para liberar tanto recursos administrados como no administrados; false para liberar únicamente recursos no administrados.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
