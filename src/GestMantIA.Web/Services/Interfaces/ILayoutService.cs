namespace GestMantIA.Web.Services.Interfaces
{
    public interface ILayoutService
    {
        /// <summary>
        /// Inicializa el servicio de diseño.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Establece el título de la página actual.
        /// </summary>
        /// <param name="title">Título de la página.</param>
        Task SetTitleAsync(string title);

        /// <summary>
        /// Indica si el modo oscuro está activado.
        /// </summary>
        bool IsDarkMode { get; }

        /// <summary>
        /// Indica si el drawer está abierto.
        /// </summary>
        bool IsDrawerOpen { get; }

        /// <summary>
        /// Se dispara cuando ocurre una actualización importante.
        /// </summary>
        event Action? MajorUpdateOccured;

        /// <summary>
        /// Se dispara cuando cambia el título de la página.
        /// </summary>
        event Action? OnTitleChanged;

        /// <summary>
        /// Alterna entre el modo oscuro y el modo claro.
        /// </summary>
        Task ToggleDarkModeAsync();

        /// <summary>
        /// Alterna el estado del drawer.
        /// </summary>
        Task ToggleDrawerAsync();

        /// <summary>
        /// Establece el estado del drawer.
        /// </summary>
        /// <param name="open">Indica si el drawer debe estar abierto.</param>
        Task SetDrawerOpenAsync(bool open);
    }
}
