using System.Collections.Generic;

namespace GestMantIA.Web.Models.Shared
{
    /// <summary>
    /// Clase que representa un resultado paginado de elementos.
    /// </summary>
    /// <typeparam name="T">Tipo de los elementos en la lista paginada.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Lista de elementos en la página actual.
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Número total de elementos en todos las páginas.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Número de página actual (comenzando en 1).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Tamaño de la página (número de elementos por página).
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Número total de páginas.
        /// </summary>
        public int TotalPages => (int)System.Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Indica si hay una página anterior disponible.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Indica si hay una página siguiente disponible.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
