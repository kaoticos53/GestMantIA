using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GestMantIA.Core.Interfaces
{
    /// <summary>
    /// Interfaz genérica para el patrón Repository que define operaciones CRUD básicas.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad que manejará el repositorio</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Obtiene una entidad por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la entidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>La entidad encontrada o null si no existe</returns>
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las entidades.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Una lista de todas las entidades</returns>
        Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene entidades que cumplen con un predicado.
        /// </summary>
        /// <param name="predicate">Predicado para filtrar las entidades</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Una lista de entidades que cumplen con el predicado</returns>
        Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega una nueva entidad al repositorio.
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>La entidad agregada</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una entidad existente.
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina una entidad.
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cuenta el número de entidades que cumplen con un predicado.
        /// </summary>
        /// <param name="predicate">Predicado para filtrar las entidades</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>El número de entidades que cumplen con el predicado</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe al menos una entidad que cumple con un predicado.
        /// </summary>
        /// <param name="predicate">Predicado para verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe al menos una entidad que cumple con el predicado, de lo contrario false</returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
