using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using MudBlazor;

namespace GestMantIA.Web.Services
{
    public interface IThemeService
    {
        bool IsDarkMode { get; }
        MudBlazor.MudTheme CurrentTheme { get; }
        event System.Action OnThemeChanged;
        Task ToggleThemeAsync();
        Task SetThemeAsync(bool isDarkMode);
        Task InitializeAsync();
    }

    public class ThemeService : IThemeService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;
        private readonly MudBlazor.MudTheme _theme;
        private bool _isDarkMode;
        private const string ThemeKey = "theme";

        public event System.Action OnThemeChanged = delegate { };

        public bool IsDarkMode => _isDarkMode;
        public MudBlazor.MudTheme CurrentTheme => _theme;

        public ThemeService(
            ILocalStorageService localStorage,
            IJSRuntime jsRuntime)
        {
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
            _theme = new MudBlazor.MudTheme();
        }

        public async Task InitializeAsync()
        {
            _isDarkMode = await _localStorage.GetItemAsync<bool>("darkMode");
            await ApplyThemeAsync();
        }

        public async Task ToggleThemeAsync()
        {
            _isDarkMode = !_isDarkMode;
            await ApplyThemeAsync();
            await _localStorage.SetItemAsync("darkMode", _isDarkMode);
        }

        public async Task SetThemeAsync(bool isDarkMode)
        {
            if (_isDarkMode != isDarkMode)
            {
                _isDarkMode = isDarkMode;
                await ApplyThemeAsync();
                await _localStorage.SetItemAsync("darkMode", _isDarkMode);
            }
        }

        private async Task ApplyThemeAsync()
        {
            if (_isDarkMode)
            {
                _theme.PaletteDark.Primary = "#ff6b35".ToSwatch();
                _theme.PaletteDark.Secondary = "#2d3436".ToSwatch();
                _theme.PaletteDark.Background = "#1a1a1a".ToSwatch();
                _theme.PaletteDark.AppbarBackground = "#1e1e1e".ToSwatch();
                _theme.PaletteDark.DrawerBackground = "#1e1e1e".ToSwatch();
                _theme.PaletteDark.Surface = "#2d2d2d".ToSwatch();
                _theme.PaletteDark.TextPrimary = "#f5f6fa".ToSwatch();
                _theme.PaletteDark.TextSecondary = "#b2bec3".ToSwatch();
                _theme.PaletteDark.TextDisabled = "#636e72".ToSwatch();
                _theme.PaletteDark.ActionDefault = "#f5f6fa".ToSwatch();
                _theme.PaletteDark.ActionDisabled = "#636e72".ToSwatch();
            }
            else
            {
                _theme.PaletteLight.Primary = "#ff6b35".ToSwatch();
                _theme.PaletteLight.Secondary = "#2d3436".ToSwatch();
                _theme.PaletteLight.Background = "#f5f6fa".ToSwatch();
                _theme.PaletteLight.AppbarBackground = "#2d3436".ToSwatch();
                _theme.PaletteLight.DrawerBackground = "#2d3436".ToSwatch();
                _theme.PaletteLight.Surface = "#ffffff".ToSwatch();
                _theme.PaletteLight.TextPrimary = "#2d3436".ToSwatch();
                _theme.PaletteLight.TextSecondary = "#636e72".ToSwatch();
                _theme.PaletteLight.TextDisabled = "#b2bec3".ToSwatch();
                _theme.PaletteLight.ActionDefault = "#2d3436".ToSwatch();
                _theme.PaletteLight.ActionDisabled = "#b2bec3".ToSwatch();
            }

            await _jsRuntime.InvokeVoidAsync("mudThemeManager.applyTheme", _isDarkMode ? "dark" : "light");
            OnThemeChanged.Invoke();
        }
    }

    public static class StringExtensions
    {
        public static string ToSwatch(this string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor)) return string.Empty;
            return hexColor.StartsWith("#") ? hexColor : $"#{hexColor}";
        }
    }
}
