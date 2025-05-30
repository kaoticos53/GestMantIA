namespace GestMantIA.Core.Shared
{
    /// <summary>
    /// Clase genérica para manejar resultados paginados
    /// </summary>
    /// <typeparam name="T">Tipo de los elementos en la lista paginada</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="PagedResult{T}"/>.
        /// </summary>
        public PagedResult()
        {
            Items = new List<T>();
            PageNumber = 1;
            PageSize = 10;
            TotalCount = 0;
        }

        /// <summary>
        /// Lista de elementos en la página actual
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Número de la página actual (comenzando en 1)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Tamaño de la página (número de elementos por página)
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Número total de elementos en todas las páginas
        /// </summary>
        public int TotalCount { get; set; }


        /// <summary>
        /// Número total de páginas disponibles
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)System.Math.Ceiling(TotalCount / (double)PageSize) : 0;

        /// <summary>
        /// Indica si hay una página anterior
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Indica si hay una página siguiente
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
