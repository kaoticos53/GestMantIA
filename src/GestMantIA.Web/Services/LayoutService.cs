using System.Threading.Tasks;
using Blazored.LocalStorage;
using GestMantIA.Web.Models;
using GestMantIA.Web.Services.Interfaces;

namespace GestMantIA.Web.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly ILocalStorageService _localStorage;
        private const string LayoutKey = "layoutSettings";
        private string _currentTitle = "GestMantIA";

        /// <summary>
        /// Obtiene el título actual de la página.
        /// </summary>
        public string CurrentTitle => _currentTitle;

        /// <summary>
        /// Evento que se dispara cuando cambia el título de la página.
        /// </summary>
        public event Action? OnTitleChanged;

        public LayoutService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task InitializeAsync()
        {
            // Cargar configuración de diseño del almacenamiento local
            var layoutSettings = await _localStorage.GetItemAsync<LayoutSettings>(LayoutKey);
            
            // Si no hay configuración guardada, usar valores por defecto
            if (layoutSettings == null)
            {
                layoutSettings = new LayoutSettings
                {
                    IsDarkMode = true,
                    IsSidebarCollapsed = false,
                    PrimaryColor = "#ff6b35", // Naranja corporativo
                    SecondaryColor = "#2d3436", // Gris oscuro
                    FontFamily = "'Poppins', sans-serif"
                };
                
                await _localStorage.SetItemAsync(LayoutKey, layoutSettings);
            }

            // Aplicar la configuración de diseño
            await ApplyLayoutSettings(layoutSettings);
        }

        public async Task UpdateLayoutAsync()
        {
            // Obtener la configuración actual
            var layoutSettings = await _localStorage.GetItemAsync<LayoutSettings>(LayoutKey) ?? new LayoutSettings();
            
            // Guardar la configuración actualizada
            await _localStorage.SetItemAsync(LayoutKey, layoutSettings);
            
            // Aplicar la configuración de diseño
            await ApplyLayoutSettings(layoutSettings);
        }

        private async Task ApplyLayoutSettings(LayoutSettings settings)
        {
            // Aquí se aplicarían los estilos al layout principal
            // Esto podría incluir actualizar variables CSS personalizadas
            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task SetTitleAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                _currentTitle = "GestMantIA";
            }
            else
            {
                _currentTitle = $"{title} | GestMantIA";
            }
            
            // Notificar a los suscriptores que el título ha cambiado
            OnTitleChanged?.Invoke();
            
            // Actualizar el título del documento
            await Task.CompletedTask;
        }
    }

    public class LayoutSettings
    {
        public bool IsDarkMode { get; set; } = true;
        public bool IsSidebarCollapsed { get; set; } = false;
        public string PrimaryColor { get; set; } = "#ff6b35";
        public string SecondaryColor { get; set; } = "#2d3436";
        public string FontFamily { get; set; } = "'Poppins', sans-serif";
        // Agregar más configuraciones según sea necesario
    }
}
