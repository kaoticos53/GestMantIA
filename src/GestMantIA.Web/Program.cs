using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GestMantIA.Web;
using GestMantIA.Web.Services;
using GestMantIA.Web.Services.Interfaces;
using Blazored.LocalStorage;
using Blazored.Toast;
// using Microsoft.AspNetCore.Components.WebAssembly.Authentication; // Comentado si no se usa directamente o si CustomAuthStateProvider lo maneja todo
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;
using Microsoft.Extensions.Configuration;
using MudBlazor.Services;
using MudBlazor;
using System.Text.Json;
// using GestMantIA.Web.Models; // Si no se usa directamente aquí
// using GestMantIA.Core.Identity.Interfaces; // Se eliminará si no hay dependencias directas válidas aquí
// using GestMantIA.Infrastructure.Services; // Eliminado
// using GestMantIA.Infrastructure.Services.Auth; // Eliminado
// using GestMantIA.Infrastructure; // Eliminado
// using GestMantIA.Core.Identity.Services; // Eliminado

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configuración de la aplicación
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? builder.HostEnvironment.BaseAddress;

// Configuración de MudBlazor
builder.Services.AddMudServices(config =>
{
    // Configuración de notificaciones
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
    
    // Configuración del tema
    config.SnackbarConfiguration.BackgroundBlurred = true;
    config.SnackbarConfiguration.MaxDisplayedSnackbars = 5;
});

// Configuración de servicios de autenticación
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Configuración de HttpClient
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Configuración de servicios personalizados
builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddScoped<Core.Identity.Interfaces.IUserService, Infrastructure.Services.UserService>(); // Eliminado
// builder.Services.AddScoped<Core.Identity.Interfaces.ITokenService, Infrastructure.Services.Auth.JwtTokenService>(); // Eliminado
builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddScoped<IThemeService, ThemeService>();

// Configuración de almacenamiento local
builder.Services.AddBlazoredLocalStorage(config =>
{
    config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
    config.JsonSerializerOptions.IgnoreNullValues = false;
    config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    config.JsonSerializerOptions.WriteIndented = false;
});

// Configuración de componentes de la aplicación
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuración de mensajes toast
builder.Services.AddBlazoredToast();

// Construir la aplicación
var host = builder.Build();

// Inicializar servicios necesarios
var layoutService = host.Services.GetRequiredService<ILayoutService>();
await layoutService.InitializeAsync();

// Inicializar el tema
var themeService = host.Services.GetRequiredService<IThemeService>();
await themeService.InitializeAsync();

// Ejecutar la aplicación
await host.RunAsync();
