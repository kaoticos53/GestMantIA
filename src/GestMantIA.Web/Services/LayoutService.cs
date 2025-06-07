using Blazored.LocalStorage;
using GestMantIA.Web.Services.Interfaces;
using System;

namespace GestMantIA.Web.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly ILocalStorageService _localStorage;
        private const string LayoutKey = "layoutSettings";
        private LayoutSettings _layoutSettings = new(); 
        private string _currentTitle = "GestMantIA";

        public string CurrentTitle => _currentTitle;
        public event Action? OnTitleChanged;
        public event Action? MajorUpdateOccured; 

        public bool IsDarkMode => _layoutSettings.IsDarkMode;
        public bool IsDrawerOpen => _layoutSettings.IsDrawerOpen; 

        public LayoutService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task InitializeAsync()
        {
            var storedSettings = await _localStorage.GetItemAsync<LayoutSettings>(LayoutKey);
            if (storedSettings == null)
            {
                _layoutSettings = new LayoutSettings(); 
                await _localStorage.SetItemAsync(LayoutKey, _layoutSettings);
            }
            else
            {
                _layoutSettings = storedSettings;
            }
        }

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
            OnTitleChanged?.Invoke();
            await Task.CompletedTask; 
        }

        public async Task ToggleDarkModeAsync()
        {
            _layoutSettings.IsDarkMode = !_layoutSettings.IsDarkMode;
            await _localStorage.SetItemAsync(LayoutKey, _layoutSettings);
            MajorUpdateOccured?.Invoke();
        }

        public async Task ToggleDrawerAsync()
        {
            _layoutSettings.IsDrawerOpen = !_layoutSettings.IsDrawerOpen;
            await _localStorage.SetItemAsync(LayoutKey, _layoutSettings);
            MajorUpdateOccured?.Invoke();
        }

        public async Task SetDrawerOpenAsync(bool open)
        {
            if (_layoutSettings.IsDrawerOpen != open)
            {
                _layoutSettings.IsDrawerOpen = open;
                await _localStorage.SetItemAsync(LayoutKey, _layoutSettings);
                MajorUpdateOccured?.Invoke();
            }
        }
    }

    public class LayoutSettings
    {
        public bool IsDarkMode { get; set; } = true;
        public bool IsDrawerOpen { get; set; } = false; 
        public string PrimaryColor { get; set; } = "#ff6b35";
        public string SecondaryColor { get; set; } = "#2d3436";
        public string FontFamily { get; set; } = "'Poppins', sans-serif";
    }
}
