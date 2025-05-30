namespace GestMantIA.Web.Services.Interfaces
{
    public interface ILayoutService
    {
        /// <summary>
        /// Inicializa el servicio de diseño.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Actualiza la configuración de diseño.
        /// </summary>
        Task UpdateLayoutAsync();

        /// <summary>
        /// Establece el título de la página actual.
        /// </summary>
        /// <param name="title">Título de la página.</param>
        Task SetTitleAsync(string title);
    }
}
