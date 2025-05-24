using System;
using System.Threading;
using System.Threading.Tasks;

namespace GestMantIA.Core.Interfaces
{
    /// <summary>
    /// Interfaz que define el patrón Unit of Work para manejar transacciones y repositorios.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Obtiene un repositorio para el tipo de entidad especificado.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad</typeparam>
        /// <returns>Una instancia del repositorio para el tipo de entidad</returns>
        IRepository<T> Repository<T>() where T : class;

        /// <summary>
        /// Guarda todos los cambios realizados en el contexto de la base de datos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>El número de entidades escritas en la base de datos</returns>
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Inicia una nueva transacción.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Una tarea que representa la operación asíncrona</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirma la transacción actual.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Una tarea que representa la operación asíncrona</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Revierte la transacción actual.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Una tarea que representa la operación asíncrona</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
