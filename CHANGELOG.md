# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Corregido (Sesión Actual - Cascade AI)
- **FIX (Errores de Compilación de Telemetría en `GestMantIA.API`)**: Resueltos múltiples errores de compilación en `GestMantIA.API` relacionados con la integración de telemetría (App.Metrics e InfluxDB). Las correcciones incluyeron:
  - Ajuste de versiones y referencias de paquetes NuGet (`App.Metrics`, `App.Metrics.Formatters.InfluxDB`, `App.Metrics.Formatters.Json`, `App.Metrics.Formatters.Prometheus`, `App.Metrics.Reporting.InfluxDB`, `InfluxDB.Client`).
  - Actualización de la configuración de `InfluxDbOptions` en `MetricsConfiguration.cs` para usar la propiedad `Database` en lugar de `Bucket` y eliminar la configuración de `Org` (manejada por el cliente InfluxDB).
  - Corrección en la instanciación y uso de formateadores de métricas (Prometheus, JSON) en `MetricsConfiguration.cs`, utilizando constructores directos en lugar de `metrics.Formatter<T>()` donde era problemático.
  - Asegurar los `using` correctos y la inicialización adecuada de `InfluxDBClientOptions` dentro de la configuración del reporter de InfluxDB.
  - Verificación y ajuste de la configuración de `IMetricsOutputFormattingBuilder` en `Program.cs` y `MetricsConfiguration.cs`.

- **FIX (Errores CS0104 y CS0311 en `GestMantIA.Web`):** Resuelta la referencia ambigua de `IUserService` y el consecuente error de conversión de tipo genérico (`CS0311`) en `src/GestMantIA.Web/Program.cs`. Se calificaron completamente los tipos `GestMantIA.Web.Services.Interfaces.IUserService` y `GestMantIA.Web.Services.UserService` en el registro de servicios de `Program.cs`.

### Corregido (Sesión 2025-06-01)
- **FIX (Error CS0104 - Referencia Ambigua `IUserService`)**: Resuelta la ambigüedad de `IUserService` en `src/GestMantIA.Web/Pages/Admin/Usuarios/Create.razor` calificando completamente el tipo en la directiva `@inject` como `GestMantIA.Web.Services.Interfaces.IUserService`.
- **FIX (Error RZ1030 - TagHelper mal formado)**: Este error en `MudCheckBox` dentro de `src/GestMantIA.Web/Pages/Admin/Usuarios/Create.razor` se resolvió indirectamente al corregir el error de referencia ambigua `CS0104` en el mismo archivo.
- **FIX (Error CS0102)**: Eliminada la definición duplicada de la propiedad `RequireEmailConfirmation` en `src/GestMantIA.Shared/Identity/DTOs/Requests/CreateUserDTO.cs`.
- **FIX (Error RZ10001)**: Especificado el atributo `T="bool"` en el componente `MudCheckBox` en `src/GestMantIA.Web/Pages/Admin/Usuarios/Create.razor` para resolver error de inferencia de tipo.
- **FIX (Error CS0234 - IRoleService)**: Corregido error de 'IRoleService no encontrado' en `src/GestMantIA.Web/Pages/Admin/Usuarios/Create.razor` mediante:
    - Registro de `IRoleService` con su implementación en `src/GestMantIA.Web/Program.cs`.
    - Activación de referencias a `GestMantIA.Core.csproj` y `GestMantIA.Application.csproj` en `src/GestMantIA.Web/GestMantIA.Web.csproj`.
- **FIX (Error CS0234 - Common.Exceptions)**: Comentada la directiva `using GestMantIA.Application.Common.Exceptions;` en `src/GestMantIA.Application/Features/UserManagement/Services/ApplicationUserProfileService.cs` ya que el espacio de nombres o las excepciones referenciadas no existen actualmente y la directiva no era utilizada activamente en el archivo, resolviendo el error de compilación.

## [2025-06-01] - Cascade AI

### Corregido
- **FIX**: Resueltos ~35 errores de compilación CS1503 en `ApplicationUserServiceTests.cs` (y archivos de prueba relacionados) causados por el uso incorrecto de `Guid.Parse()` en variables que ya eran de tipo `Guid`. Se reemplazaron por asignaciones directas de las variables `Guid`.
- **FIX**: Corregido errores de compilación en `ApplicationUserServiceTests.cs` relacionados con la inicialización de `UserResponseDTO`. Se aseguraron todas las propiedades requeridas (`Id`, `UserName`, `Email`) y otras propiedades relevantes fueran inicializadas correctamente en las instancias de `UserResponseDTO` utilizadas en las configuraciones de mock de `IMapper` y en las aserciones de prueba, resolviendo así los fallos de compilación.
- **FIX**: Solucionados errores de compilación `CS1028` (Directiva de preprocesador inesperada) en `ApplicationUserServiceTests.cs` eliminando directivas `#endregion` huérfanas.

### Mejorado
- **Servicio de Perfil de Usuario (`ApplicationUserProfileService`)**:
  - Implementado el método `GetUserProfileAsync(Guid userId)` en `ApplicationUserProfileService.cs`.
  - El método ahora recupera y combina información del `ApplicationUser` (incluyendo roles) y la entidad `UserProfile` asociada.
  - Se inyectó `UserManager<ApplicationUser>` en el servicio para acceder a los datos de `ApplicationUser` y sus roles.
  - Se priorizan los datos específicos del perfil (ej. `FirstName`, `LastName`, `PhoneNumber`) desde la entidad `UserProfile` si está disponible.
- **Servicio de Gestión de Usuarios (`ApplicationUserService` y `IUserService`)**:
  - Se completó la implementación de `ApplicationUserService` en `GestMantIA.Application` para que cumpla con todos los métodos definidos en la interfaz `IUserService` (`GestMantIA.Core.Identity.Interfaces`).
  - Se unificó el uso de `Guid` como identificador principal para usuarios en la mayoría de los métodos, mejorando la consistencia.
  - Se eliminaron los métodos redundantes `AssignRolesToUserAsync` y `RemoveRolesFromUserAsync` de `IUserService` y `ApplicationUserService`, consolidando la lógica de asignación de roles.
  - Se implementaron los métodos faltantes para la gestión de contraseñas y confirmación de correo electrónico:
    - `GetPasswordResetTokenAsync(string userIdOrEmail)`
    - `ResetPasswordAsync(ResetPasswordDTO resetPasswordDto)`
    - `ChangePasswordAsync(string userId, ChangePasswordDTO changePasswordDto)`
    - `ConfirmEmailAsync(string userId, string token)`
    - `ResendConfirmationEmailAsync(string userIdOrEmail)`
  - Se añadió un método auxiliar privado `FindUserByIdOrEmailOrUsernameAsync` para facilitar la búsqueda de usuarios.
  - Se actualizó `IUserService` para reflejar los cambios, incluyendo la modificación de `GetUserRolesAsync` para que acepte `Guid userId`.

## [2025-05-31] - Cascade AI

### Corregido
- **Resolución Final de Errores Persistentes de Componentes MudBlazor en `GestMantIA.Web`**:
  - **`MudDialogInstance`**: Resuelto error de compilación `CS0246` ("El nombre del tipo o del espacio de nombres 'MudDialogInstance' no se encontró") en `ConfirmationDialog.razor` y `MinimalDialogTest.razor` utilizando la interfaz `IMudDialogInstance` en lugar de la clase concreta `MudDialogInstance` para el `CascadingParameter`. Se inicializaron propiedades con `default!` para `CS8618`.
  - **`MudChipSet` (`Pages/Admin/Usuarios/Edit.razor`)**: Se estabilizó la compilación (resolviendo `CS1662`) utilizando `SelectedValues` y `SelectedValuesChanged` con un manejador explícito `OnSelectedRolesChanged(IEnumerable<string> newValues)`, y especificando `T="string"`. La configuración final es:
    ```xml
    <MudChipSet SelectedValues="_selectedRoles" SelectedValuesChanged="OnSelectedRolesChanged" T="string">
    ```
    ```csharp
    // En @code
    private HashSet<string> _selectedRoles = new(StringComparer.OrdinalIgnoreCase);
    private void OnSelectedRolesChanged(IEnumerable<string> newValues)
    {
        _selectedRoles = new HashSet<string>(newValues ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
    }
    ```
  - **`MudCheckBox` (`Pages/Auth/Login.razor`)**: Se estabilizó la compilación (resolviendo `RZ10001`) utilizando `@bind-Checked` y especificando explícitamente `T="bool"`:
    ```xml
    <MudCheckBox T="bool" @bind-Checked="_loginModel.RememberMe" Label="Recordarme" Class="mb-4" />
    ```
- El proyecto `GestMantIA.Web` ahora compila sin los errores previamente identificados para `MudDialogInstance`, `MudChipSet` y `MudCheckBox`.
  *(Nota: Persisten errores MSB4018 no relacionados con estos componentes en `GestMantIA.Web` y `GestMantIA.API`.)*

## [2025-05-31] - Cascade AI

### Corregido
- **Resolución de Errores y Advertencias de Compilación en `GestMantIA.Web`**:
  - **`Login.razor`**: Resuelto error `RZ10001` (The type of component 'MudCheckBox' cannot be inferred) restaurando el atributo `T="bool"` en el componente `MudCheckBox`.
  - **`ResetPasswordPage.razor`**: Solucionado error `CS1061` ("ProblemDetails" no contiene una definición para "Extensions") modificando el manejo de excepciones `ApiException<HttpClients.ProblemDetails>`. La nueva lógica parsea directamente la respuesta JSON (`problemDetailsException.Response`) para extraer mensajes de error de campos comunes como "errors", "title" o "detail", en lugar de depender de la propiedad `Extensions` del tipo `ProblemDetails` generado por NSwag, que podría no tenerla.
  - **`AuthHeaderHandler.cs`**: Eliminada advertencia `CS8602` (Desreferencia de una referencia posiblemente NULL) añadiendo una comprobación de nulidad para `request.RequestUri` antes de acceder a `request.RequestUri.AbsolutePath`.
  - **`Pages/Admin/Usuarios/Index.razor`**: Eliminada advertencia `CS8602` (Desreferencia de una referencia posiblemente NULL) utilizando el operador condicional nulo (`?.`) y el operador de coalescencia nula (`??`) al acceder a `user.UserName.Trim()`.
- El proyecto `GestMantIA.Web` ahora compila limpiamente sin los errores y advertencias previamente identificados.

### Añadido
- **Página de Solicitud de Restablecimiento de Contraseña (`ForgotPasswordPage.razor`)**:
  - Implementada la interfaz de usuario en `/auth/forgot-password` para que los usuarios puedan solicitar un enlace de restablecimiento de contraseña.
  - La página incluye un campo para ingresar el correo electrónico y se comunica con el endpoint `/api/accounts/forgot-password` del backend.
## [2025-06-05] - Cascade AI

### Corregido
- **FIX**: Solucionado error de compilación `RZ10001` (El tipo de componente 'MudCheckBox' no se puede inferir) en `Pages/Admin/Usuarios/Create.razor`. El error se debía a que la propiedad `_createUserModel.RequireEmailConfirmation` a la que se enlazaba el `MudCheckBox` no existía en el DTO `CreateUserDTO`. Se añadió la propiedad `public bool RequireEmailConfirmation { get; init; }` a `CreateUserDTO.cs` (`GestMantIA.Shared/Identity/DTOs/Requests/CreateUserDTO.cs`) para resolver el problema.
- **FIX**: Restaurada la estructura del método de prueba `GetUserLockoutInfoAsync_UserNameIsNull_SetsEmptyStringInDTO` en `ApplicationUserServiceTests.cs`. Este método estaba severamente corrupto debido a ediciones previas, causando numerosos errores de sintaxis (CS1519, CS8124, CS0106). Esta corrección ha permitido que la solución compile correctamente a nivel general.

## [2025-06-04] - Cascade AI

### Mejorado
- **Pruebas Unitarias de `ApplicationUserService`**:
  - Revisados y actualizados exhaustivamente los tests unitarios en `ApplicationUserServiceTests.cs` (`GestMantIA.Application.UnitTests`).
  - Asegurada la compatibilidad con IDs de usuario de tipo `Guid` y la correcta conversión a `string` para todas las llamadas a `UserManager` (e.g., `FindByIdAsync(userId.ToString())`).
  - Eliminados tests obsoletos que no compilaban debido a refactorizaciones previas (e.g., `UpdateUserRolesAsync` con firma antigua).
  - Verificada la cobertura y correctitud de los tests para los siguientes métodos del servicio: `UpdateUserProfileAsync`, `LockUserAsync`, `UnlockUserAsync`, `IsUserLockedOutAsync`, `GetUserLockoutInfoAsync`, `GetPasswordResetTokenAsync`, `ResetPasswordAsync`, `ChangePasswordAsync`, `ConfirmEmailAsync`, `ResendConfirmationEmailAsync`, y `UpdateUserAsync`.
  - Confirmado el correcto mocking de `UserManager`, `ILogger`, y `IMapper`, así como el uso de `LoggerExtensions.VerifyLog` para la verificación de logs.

### Verificado
- **Integración de `UserManagementController`**:
  - Confirmado que `UserManagementController.cs` en `GestMantIA.API` utiliza correctamente `IUserService` (implementado por `ApplicationUserService`) para todas las operaciones de gestión de usuarios.
  - Verificada la consistencia en el uso de `Guid` como identificadores de usuario y los DTOs correspondientes.
  - Revisado el manejo de excepciones y logging en el controlador.

## [2025-06-03] - Cascade AI

### Añadido
- **Documentación**: Creada la guía de estilo de codificación C# (`CodingStandard.md`) para el proyecto GestMantIA, basada en las convenciones oficiales de Microsoft y las necesidades específicas del proyecto. Este documento cubre nomenclaturas, formato, y buenas prácticas de codificación.

## [2025-06-02] - Cascade AI

### Añadido
- **Pruebas Unitarias para `ApplicationUserService` en `GestMantIA.Application.UnitTests`**:
  - Añadidas pruebas unitarias exhaustivas para el método `GetAllUsersAsync`, cubriendo escenarios de filtrado (incluyendo `activeOnly`), paginación, búsqueda por término y obtención de roles.
  - Añadidas pruebas unitarias exhaustivas para el método `SearchUsersAsync`, cubriendo escenarios de búsqueda por término (nombre de usuario, email, nombre, apellido), paginación, manejo de roles y errores.
  - Añadidas pruebas unitarias exhaustivas para el método `CreateUserAsync`, cubriendo validaciones de DTO, duplicados de nombre de usuario/email, creación de usuario exitosa, asignación de roles (existentes y no existentes), fallos en la creación por parte de `UserManager`, manejo de `RequireEmailConfirmation` y errores genéricos.
  - Añadidas pruebas unitarias exhaustivas para el método `UpdateUserAsync` (basado en la implementación parcial disponible), cubriendo validaciones de DTO (nulidad, coincidencia de ID), usuario no encontrado/eliminado, actualizaciones de campos (incluyendo email/username con chequeo de duplicados), gestión de roles (añadir/eliminar/nulo/vacío), fallos en la actualización por parte de `UserManager` y errores genéricos.
- **Pruebas Unitarias para `ApplicationUserService.UpdateUserRolesAsync`**:
  - Añadidas pruebas unitarias exhaustivas para el método `UpdateUserRolesAsync`, cubriendo escenarios de usuario no encontrado, sin cambios en roles, adición/eliminación exitosa y fallida de roles, combinaciones de éxito/fallo, y manejo de excepciones.
- **Pruebas Unitarias para `ApplicationUserService.UpdateUserProfileAsync`**:
  - Añadidas pruebas unitarias para el método `UpdateUserProfileAsync`, cubriendo escenarios de usuario no encontrado, DTO sin cambios, actualizaciones exitosas de campos individuales y combinados del perfil (nombre, apellido, teléfono), fallos en la actualización por `UserManager`, y manejo de excepciones.
- **Pruebas Unitarias para `ApplicationUserService.LockUserAsync`**:
  - Añadidas pruebas unitarias para el método `LockUserAsync`, cubriendo escenarios de usuario no encontrado/eliminado, usuario ya bloqueado, bloqueo exitoso con duración específica/predeterminada y con/sin razón, fallos en `SetLockoutEndDateAsync` y manejo de excepciones.
- **Pruebas Unitarias para `ApplicationUserService.UnlockUserAsync`**:
  - Añadidas pruebas unitarias para el método `UnlockUserAsync`, cubriendo escenarios de usuario no encontrado/eliminado, usuario no bloqueado, desbloqueo exitoso (incluyendo reseteo de contador de accesos fallidos), fallos en `SetLockoutEndDateAsync`, y manejo de excepciones (incluyendo fallo en `ResetAccessFailedCountAsync` post-desbloqueo).
- **Pruebas Unitarias para `ApplicationUserService.IsUserLockedOutAsync`**:
  - Añadidas pruebas unitarias para el método `IsUserLockedOutAsync`, cubriendo escenarios de usuario no encontrado/eliminado, usuario bloqueado/no bloqueado, y manejo de excepciones.
- **Pruebas Unitarias para `ApplicationUserService.GetUserLockoutInfoAsync`**:
  - Añadidas pruebas unitarias para el método `GetUserLockoutInfoAsync`, cubriendo escenarios de usuario no encontrado/eliminado, información correcta para usuarios no bloqueados, bloqueados temporalmente y permanentemente, manejo de `UserName` nulo, y manejo de excepciones.
- **Pruebas Unitarias para `ApplicationUserService.GetPasswordResetTokenAsync`**:
  - Añadidas pruebas unitarias para el método `GetPasswordResetTokenAsync`, cubriendo escenarios de usuario no encontrado/eliminado, generación exitosa de token, y manejo de excepciones.
- **Pruebas Unitarias para `ApplicationUserService.ResetPasswordAsync`**:
  - Añadidas pruebas unitarias para el método `ResetPasswordAsync`, cubriendo DTO nulo, usuario no encontrado/eliminado, reseteo exitoso (con y sin desbloqueo), fallos, y manejo de excepciones.
- **Pruebas Unitarias para `ApplicationUserService.ChangePasswordAsync`**:
  - Añadidas pruebas unitarias para el método `ChangePasswordAsync`, cubriendo entradas inválidas, usuario no encontrado/eliminado, cambio exitoso, fallos y manejo de excepciones.
- **Pruebas Unitarias para `ApplicationUserService.ConfirmEmailAsync`**:
  - Añadidas pruebas unitarias para el método `ConfirmEmailAsync`, cubriendo entradas inválidas, usuario no encontrado, confirmación exitosa, fallos y manejo de excepciones.
- **Pruebas Unitarias para `ApplicationUserService.ResendConfirmationEmailAsync`**:
  - Añadidas pruebas unitarias para el método `ResendConfirmationEmailAsync`, cubriendo usuario no encontrado/eliminado, email ya confirmado, éxito, y manejo de excepciones.

## [2025-06-01] - Cascade AI

### Mejorado
- **Interfaz de Usuario (UI) en `GestMantIA.Web`**:
  - Mejorada la responsividad de las páginas de Login (`Login.razor`) y Registro (`Register.razor`):
    - Los diálogos ahora mantienen un ancho máximo adecuado en pantallas anchas, evitando el encogimiento excesivo.
    - Se ha corregido el desbordamiento en vistas móviles, asegurando que los formularios sean completamente visibles y utilizables.
    - Se ajustó el padding para una mejor presentación en dispositivos pequeños.
  - Configurado el menú de navegación lateral (`MainLayout.razor`) para un comportamiento responsivo:
    - El menú (drawer) ahora se oculta automáticamente en pantallas pequeñas (móviles) y se muestra de forma persistente en pantallas grandes (escritorio).
    - Se utiliza `DrawerVariant.Responsive` de MudBlazor para esta funcionalidad.

## [2024-07-16] - Cascade AI

### Corregido
- **Errores de Compilación en `ResetPasswordPage.razor`**:
  - Resueltos errores de compilación en el componente `ResetPasswordPage.razor` del proyecto `GestMantIA.Web` que impedían la funcionalidad de restablecimiento de contraseña en el frontend.
  - Corregida la directiva `@using` para `Microsoft.AspNetCore.WebUtilities`.
  - Asegurada la correcta deserialización de respuestas de error de la API, manejando `ProblemDetails` y el wrapper `Result`.
  - Implementadas notificaciones `Snackbar` de MudBlazor para feedback al usuario.
  - Garantizadas las actualizaciones de la UI mediante llamadas a `StateHasChanged()` en los bloques `catch` y `finally`.
- **Disponibilidad de Clases `Result` y `Result<T>`**:
  - Creado el archivo `Result.cs` en el proyecto `GestMantIA.Shared` (ruta `src/GestMantIA.Shared/Result.cs`) para proveer las clases wrapper `Result` y `Result<T>`.
  - Actualizada la directiva `@using` en `ResetPasswordPage.razor` de `@using GestMantIA.Shared.Wrapper` a `@using GestMantIA.Shared` para reflejar la nueva ubicación y espacio de nombres de las clases `Result`.

### Mejorado
- **CSS Simplificado**:
  - Eliminadas variables y estilos CSS de tema oscuro personalizado de `wwwroot/css/app.css` para simplificar la base de estilos y depender de los estilos por defecto de Blazor y MudBlazor.

## [2025-05-31] - Cascade AI

### Corregido
- **Generación de Cliente API (Swagger/NSwag)**:
  - Resuelto error de compilación `MSB4018` (relacionado con la tarea `DiscoverPrecompressedAssets`) en `GestMantIA.API.csproj`. La solución implicó deshabilitar la inclusión automática de activos web estáticos (`<EnableDefaultStaticWebAssetItems>false</EnableDefaultStaticWebAssetItems>`) para evitar conflictos durante el procesamiento de archivos en la carpeta `wwwroot`.
  - Solucionada la falta de generación del archivo `ApiClient.cs` en el proyecto `GestMantIA.Web` que impedía la compilación del frontend.

### Agregado
- **Generación de Cliente API (Swagger/NSwag)**:
  - Implementado un target MSBuild (`GenerateSwaggerFile`) en `GestMantIA.API.csproj` para generar automáticamente `swagger.json` después de la compilación, utilizando la herramienta local `dotnet swagger`.
  - Añadido un nuevo target MSBuild (`GenerateApiClient`) a `GestMantIA.Web.csproj` para invocar NSwag. Este target utiliza el `swagger.json` (copiado desde el proyecto API) para generar el archivo `ApiClient.cs`, asegurando que el cliente API esté siempre actualizado con la definición del backend.


## [2025-05-31] - Cascade AI

### Agregado
- **Funcionalidad de Recuperación de Contraseña (Backend)**:
  - Creados DTOs `ForgotPasswordRequest` y `ResetPasswordRequest` para las solicitudes API.
  - Añadidas propiedades `PasswordResetToken` (string nullable) y `PasswordResetTokenExpiration` (DateTime nullable) a la entidad `ApplicationUser`.
  - Generada y aplicada migración de base de datos (`20250531175404_AddPasswordResetFieldsToUser`) para añadir las nuevas columnas a la tabla de usuarios.
  - Implementado `IAccountService` y `AccountService` con lógica para:
    - Generar un token de restablecimiento de contraseña único y con tiempo de expiración.
    - Enviar un correo electrónico (simulado, guardado como archivo HTML en `TempEmails`) al usuario con el enlace de restablecimiento.
    - Validar el token de restablecimiento y actualizar la contraseña del usuario.
  - Creado `AccountsController` con endpoints API:
    - `POST /api/accounts/forgot-password`: Para solicitar el restablecimiento de contraseña.
    - `POST /api/accounts/reset-password`: Para confirmar el restablecimiento con el token y la nueva contraseña.
  - Actualizado `swagger.json` y regenerado `ApiClient.cs` en `GestMantIA.Web` para incluir los nuevos endpoints y DTOs.

## [2025-05-31] - Cascade AI

### Agregado
- **Módulo de Gestión de Usuarios**:
  - Implementada la interfaz completa de gestión de usuarios con listado, búsqueda, creación, edición y eliminación
  - Creado componente de diálogo de confirmación reutilizable (`ConfirmationDialog.razor`)
  - Agregada validación de formularios con DataAnnotations y mensajes de error descriptivos
  - Implementado manejo de roles y estados de usuario (activo/inactivo)
  - Añadida paginación y búsqueda en tiempo real en el listado de usuarios
  - Integrada la interfaz con el servicio de usuarios existente

### Mejorado
- **Experiencia de Usuario**:
  - Mejorado el diseño de tarjetas de usuario con avatares y estados visuales
  - Agregados tooltips para acciones y estados
  - Mejorado el feedback visual durante operaciones asíncronas
  - Validación en tiempo real en formularios de usuario

## [2025-05-31] - Cascade AI

### Agregado
- **Generación e Integración de Cliente API para Autenticación en `GestMantIA.Web`**:
  - Configurado NSwag (`nswag.json` y target MSBuild en `GestMantIA.Web.csproj`) para generar automáticamente el cliente C# `GestMantIAApiClient` a partir de `swagger.json`.
  - Registrado `IGestMantIAApiClient` en `Program.cs` para inyección de dependencias, configurando su `BaseAddress`.
  - Creado y registrado `AuthHeaderHandler` como `DelegatingHandler` para adjuntar automáticamente el token JWT (desde `localStorage`) a las cabeceras `Authorization` de las solicitudes HTTP realizadas por `GestMantIAApiClient`.
  - Refactorizado `AuthService.cs` para utilizar `IGestMantIAApiClient` en el método `LoginAsync`, gestionando la obtención del token y la notificación al `CustomAuthStateProvider`.
  - Actualizado `CustomAuthStateProvider.cs`: el método `NotifyUserAuthentication` ahora acepta el token JWT directamente para una actualización más eficiente del estado de autenticación.
  - El flujo de inicio de sesión ahora está completamente conectado con la API a través del cliente generado, incluyendo el manejo de tokens y la actualización del estado de autenticación en la aplicación Blazor WASM.

## [2025-05-30] - Cascade AI

### Corregido
- Alineada la implementación de `AuthService.cs` en `GestMantIA.Web` con las definiciones de DTOs generadas por NSwag en `swaggerClient.cs`:
  - `LoginRequest` ahora usa `UsernameOrEmail` en lugar de `Username`.
  - `RegisterRequest` ahora usa `UserName` en lugar de `Username`, y `FullName` en lugar de `FirstName` y `LastName`.
  - La obtención del token de `AuthenticationResult` (retornado por `LoginAsync` y `RefreshTokenAsync`) ahora usa `AccessToken` en lugar de `Token`.
- Esto resuelve errores de compilación previos (CS0117) relacionados con propiedades de DTO no encontradas.
- Verificada la compilación exitosa del proyecto `GestMantIA.Web` tras los cambios.

### Mejorado
- Se verificó que los atributos de MudBlazor como `AlignItems` y `PreserveOpenState` están correctamente capitalizados, evitando posibles advertencias.

## [2024-07-29] - Cascade

### Corregido
- Resueltos errores de compilación en `GestMantIA.Web` relacionados con `ILayoutService` en `MainLayout.razor`.
- Refactorizada la interfaz `ILayoutService` y su implementación `LayoutService` para gestionar correctamente el estado del tema oscuro y del panel lateral (drawer), incluyendo la persistencia en `LocalStorage`.
- Actualizado `MainLayout.razor` para utilizar la `ILayoutService` refactorizada, mejorando la sincronización del estado de la UI con el servicio.

### Mejorado
- Refactorizada la página `Login.razor` para utilizar `EditForm` con `DataAnnotationsValidator`, mejorando la validación del modelo.
- Implementado el uso de `ISnackbar` para notificaciones de error y éxito en `Login.razor`.
- Corregido el manejo de excepciones y errores del servidor en `Login.razor` para una experiencia de usuario más robusta.

## [2025-05-30] - Cascade AI

### Mejorado
- Refactorizada la página `Register.razor` para utilizar `EditForm` con `DataAnnotationsValidator`, mejorando la validación del modelo y la estructura del formulario.
- Actualizados los componentes `MudTextField` en `Register.razor` para enlazar directamente con las propiedades del modelo `RegisterModel` usando el atributo `For`.
- Eliminada la lógica de validación manual de confirmación de contraseña en `Register.razor`, delegándola a los `DataAnnotations` del modelo.

### Corregido
- Resueltos errores de compilación `CS1061` en `Login.razor` corrigiendo el acceso a las propiedades del objeto de respuesta de autenticación (usando `result.Success` y `result.Message`).
- Solucionado error de compilación `More than one compatible asset found for 'app.css'` en `GestMantIA.Web` corrigiendo la referencia de `css/app.min.css` a `css/app.css` en `wwwroot/index.html` y asegurando que el `PWAManifest` en `GestMantIA.Web.csproj` apunte a `manifest.webmanifest`.

{{ ... }}

- Validada la compilación limpia de toda la solución tras la migración.
- Fecha: 2025-05-30
{{ ... }}

### Changed
- **Refactorización y Completitud de `IUserService` y `ApplicationUserService`**:
  - Implementados los métodos restantes de `IUserService` en `ApplicationUserService`: `UpdateUserProfileAsync`, `LockUserAsync`, `UnlockUserAsync`, `IsUserLockedOutAsync`, y `GetUserLockoutInfoAsync`.
  - Se aseguró la alineación con la interfaz `IUserService`, utilizando DTOs apropiados y eliminando el uso del patrón `Result<T>` en la capa de servicio.
  - Eliminados placeholders de métodos obsoletos en `ApplicationUserService`.

## [Unreleased]

### Added
- Generado cliente de API (`swaggerClient`) para `GestMantIA.Web` a partir de la especificación OpenAPI (`swagger.json`) para la comunicación con `GestMantIA.API`.
- Registrado `swaggerClient` en `Program.cs` de `GestMantIA.Web` para inyección de dependencias, con configuración de `BaseAddress`.
      - Corregida la implementación de `IAuditableEntity` en `Entities/BaseEntity.cs` (tipos de `CreatedAt`, `UpdatedAt`, `DeletedAt`, `DeletedBy`).
    - `GestMantIA.Shared` / `GestMantIA.Application`:
      - Creado `UserProfileDto.cs` en `Shared/Identity/DTOs/` para resolver errores de tipo no encontrado en `ApplicationUserProfileService`.
  - La solución ahora compila sin errores.

## [2025-05-28] - Cascade AI
### Agregado
- **Migración a PostgreSQL**:
  - Configuración de PostgreSQL para entornos de desarrollo y producción
  - Base de datos `gestmantia_dev` para desarrollo
  - Base de datos `gestmantia_prod` para producción
  - Esquemas organizados: `public` (migraciones), `identity` (usuarios y roles), `security` (alertas y notificaciones)
  - Configuración de cadenas de conexión seguras
  - Documentación de la estructura de la base de datos

## [2025-05-28] - Cascade AI
### Corregido
- **Consolidación de DbContexts**:
  - Se eliminó la duplicación del `ApplicationDbContext` en la inyección de dependencias que causaba un ciclo de dependencia.
  - Se actualizó la configuración de Entity Framework para usar un único `ApplicationDbContext` en lugar de múltiples contextos.
  - Se corrigió la configuración de las entidades para evitar conflictos en el modelo de datos.
  - Se actualizaron las pruebas unitarias para reflejar los cambios en el modelo de datos y la estructura de la base de datos.
  - Se resolvieron advertencias de compilación relacionadas con propiedades no inicializadas en las pruebas.

## [YYYY-MM-DD] - Cascade AI
### Corregido
- Resueltos múltiples errores de compilación en `GestMantIA.API`:
  - CS1503 (Argument type mismatch) en `UserManagementController` al llamar a `GetAllUsersAsync` debido a un `bool?` pasado a un parámetro `bool`. Solucionado usando `activeOnly ?? true`.
  - CS1503 (Argument type mismatch) en `AuthController` al llamar a `ForgotPasswordAsync` debido a pasar `request.Email` (string) en lugar del objeto `ForgotPasswordRequest`. Solucionado pasando el objeto `request` completo.
  - CS0117 (Missing member) y CS0200 (Read-only property) en `SecurityNotificationsController` al inicializar `PagedResult<SecurityNotificationDto>`. Solucionado usando `PageNumber` en lugar de `Page` y eliminando la asignación a `TotalPages`.
- Resueltas advertencias de compilación en `GestMantIA.API`:
  - CS8618 (Non-nullable property not initialized) en `SecurityNotificationDto` para `Title` y `Message`. Solucionado inicializándolas a `string.Empty`.
  - CS8602 (Possible null reference dereference) y CS8603 (Possible null return) en `AuthController` dentro de los métodos `GetIpAddress` y `GetOrigin`. Solucionado mejorando el manejo de nulos.
  - CS1998 (Async method lacks 'await') en `SecurityNotificationsController` para los métodos `MarkAsRead` y `GetUnreadCount`. Solucionado convirtiéndolos a métodos síncronos.

## [1.2.1] - 2025-05-25

### Cambiado
- **Refactorización de DTOs y Estructura de Proyecto**:
  - Se creó el proyecto `GestMantIA.Shared` para albergar DTOs y modelos comunes, mejorando el desacoplamiento entre capas.
  - Todos los DTOs relacionados con Identidad (ubicados anteriormente en `GestMantIA.Core.Identity.DTOs`) fueron movidos a `GestMantIA.Shared.Identity.DTOs`.
  - Se actualizaron los espacios de nombres en todos los DTOs movidos para reflejar su nueva ubicación.
  - Se actualizaron las referencias de proyecto en `GestMantIA.Api`, `GestMantIA.Infrastructure` y `GestMantIA.Web` para incluir `GestMantIA.Shared`.
  - Se actualizaron los `using` statements en los proyectos `GestMantIA.Api` y `GestMantIA.Infrastructure` para apuntar a los DTOs en `GestMantIA.Shared`.
  - Se verificó que `GestMantIA.Web` utiliza modelos locales y no requirió cambios extensivos de `using` para los DTOs movidos.
  - La solución completa compila exitosamente después de la refactorización.

## [1.2.0] - 2025-05-25

### Agregado
- **Migración a MudBlazor**:
  - Instalación de paquetes NuGet de MudBlazor
  - Configuración del tema personalizado con colores corporativos
  - Implementación del layout principal con componentes de navegación
  - Soporte para tema oscuro/claro
  - Sistema de notificaciones integrado

### Cambiado
- **Mejoras en el UserService**:
  - Actualización de tipos para usar nombres completos en los DTOs
  - Mejora en el manejo de mapeos con AutoMapper
  - Optimización de consultas para mejor rendimiento
- **Mejora en el manejo de errores**
- **Arquitectura Frontend/Backend**: Se decidió crear un proyecto `GestMantIA.Shared` para los DTOs y modelos comunes, eliminando las referencias directas de `GestMantIA.Web` a `GestMantIA.Core` y `GestMantIA.Infrastructure` para mejorar el desacoplamiento.

### Corregido
- **Correcciones de tipos**:
  - Resolución de conflictos de nombres en los DTOs
  - Corrección en los mapeos entre entidades y DTOs
  - Mejora en el manejo de errores
- **Errores de Compilación Backend (GestMantIA.Infrastructure, GestMantIA.Core, GestMantIA.Api)**:
  - Corregida conversión de `string` a `Guid` en `JwtTokenService`.
  - Añadidas propiedades faltantes (`Username`, `FullName`) a `UserInfo` DTO.
  - Añadida directiva `using` necesaria en `DependencyInjection` para servicios.
  - Añadidas propiedades de auditoría (`CreatedAt`, `UpdatedAt`) a `ApplicationRole` y `ApplicationUser`.
  - Corregido mapeo de AutoMapper en `UserManagementMapping` para `UserResponseDTO` (uso de `DateRegistered`, eliminación de `UpdatedAt` inexistente).
  - Añadida directiva `using` necesaria en `UsersController` para DTOs de respuesta.
  - Corregido mapeo de AutoMapper en `UserProfileMapping` para `UserResponseDTO.DateRegistered`.
  - Corregidas asignaciones directas a `UserResponseDTO` en `UserService` para usar `DateRegistered` y eliminar referencias a `UpdatedAt`.
  - Corregido uso de `SecurityEventTypes` y eliminada directiva `using` innecesaria en `SecurityLogger`.
- **Error de Compilación Frontend (GestMantIA.Web)**:
  - Resuelto error `NETSDK1082` ("No había ningún paquete de tiempo de ejecución para Microsoft.AspNetCore.App disponible...") eliminando las referencias directas de `GestMantIA.Web.csproj` a los proyectos `GestMantIA.Core` y `GestMantIA.Infrastructure`.

## [1.1.0] - 2025-05-24

### Agregado
- **Panel de Administración (Frontend)**:
  - Configuración inicial con tema oscuro y tonos naranjas
  - Módulo de autenticación con JWT
  - Dashboard con estadísticas y notificaciones
  - Gestión completa de usuarios, roles y permisos
  - Sección de perfil de usuario con preferencias
  - Pruebas unitarias y de integración

### Cambiado
- **Actualización del ROADMAP**:
  - Reorganización de las fases para priorizar el desarrollo frontend
  - Actualización de la Fase 4 para incluir el nuevo panel de administración
  - Ajuste de la numeración de fases posteriores
  - Actualización del progreso general al 50%
  - Mejora en la documentación de estándares de código
  - Especificación detallada de la arquitectura y patrones de diseño

### Eliminado
- Referencias obsoletas a configuraciones antiguas de frontend
- Tareas duplicadas en el roadmap

### Corregido
- Problemas de codificación en archivos de documentación
- Inconsistencias en la numeración de fases y tareas
- Errores de formato en el archivo ROADMAP.md

## [1.0.0] - 2025-05-23

### Agregado
- **Documentación de Arquitectura**:
  - Se agregó el archivo `ARCHITECTURE.md` con estándares y convenciones de código
  - Se incluyeron guías para estructura de proyectos, patrones de diseño y documentación
- **Plantillas de Código**:
  - Se agregaron plantillas para DTOs, controladores y pruebas unitarias
  - Se creó un script `New-Component.ps1` para generar componentes basados en plantillas
  - Se documentó el uso de las plantillas en `.templates/README.md`
- **Módulo de Mantenimiento**:
  - Estructura inicial para el inventario de equipos
  - Modelos de datos para equipos y ubicaciones
  - Servicios básicos para la gestión de inventario
  - Reportes básicos de mantenimiento
- **Planificación Extendida**:
  - Se detalló la Fase 5 (Módulo de Reportes) con funcionalidades avanzadas
  - Se agregó la Fase 6 (Despliegue y Operaciones) con infraestructura como código
  - Se incluyeron secciones de análisis predictivo y dashboards interactivos

### Cambiado
- **Actualización del ROADMAP**:
  - Se marcó la Fase 3 (Módulo de Usuarios) como completada
  - Se inició la Fase 4 (Módulo de Mantenimiento)
  - Se actualizó el progreso general al 45%
- **Mejoras en la Documentación**:
  - Se actualizó la estructura de la documentación técnica
  - Se agregaron ejemplos de implementación
  - Se mejoró la guía de contribución
- **Actualización de UserProfileDTO a UserResponseDTO**:
  - Reemplazado `UserProfileDTO` por `UserResponseDTO` en todo el proyecto
  - Actualizado el mapeo de `ApplicationUser` a `UserResponseDTO` en `UserProfileMapping`
  - Actualizados los tests unitarios para usar `UserResponseDTO`
  - Eliminadas referencias obsoletas a `UserProfileDTO`
  - Mejorado el manejo de roles y claims en `UserResponseDTO`

### Agregado
- **Sistema de Notificaciones de Seguridad**:
  - Entidades `SecurityLog`, `SecurityNotification` y `SecurityAlert` para el registro de eventos de seguridad
  - Servicio `SecurityLogger` para el registro centralizado de eventos de seguridad
  - Servicio `SecurityNotificationService` para el envío de notificaciones a usuarios y al equipo de seguridad
  - Controladores `SecurityNotificationsController` y `SecurityAlertsController` para la gestión de notificaciones
  - Detección automática de actividades sospechosas (intentos de inicio de sesión fallidos, nuevos dispositivos, etc.)
  - Integración con el sistema de correo electrónico para notificaciones
  - Documentación de la API para los nuevos endpoints

### Cambiado
- Actualizado el contexto de base de datos para incluir las nuevas entidades de seguridad
- Mejorado el manejo de errores en los servicios de autenticación
- Actualizada la documentación del proyecto

### Corregido
- Problemas de compilación relacionados con la nulabilidad en varias clases
- Conflictos de versiones de paquetes NuGet
- Errores de validación en los DTOs


## [0.9.1] - 2025-05-23

### Agregado
- Implementación de la funcionalidad de restablecimiento de contraseña:
  - Endpoint `POST /api/auth/forgot-password` para solicitar restablecimiento
  - Endpoint `POST /api/auth/reset-password` para establecer nueva contraseña
  - Servicio `AuthenticationService` con métodos para manejar el flujo de restablecimiento
  - Integración con `IEmailService` para enviar correos de restablecimiento
  - Clase `OperationResult` para estandarizar respuestas de operaciones

### Cambiado
- Actualizado `AuthController` para incluir los nuevos endpoints
- Mejorado el manejo de errores en los servicios de autenticación
- Actualizada la documentación de la API con los nuevos endpoints

### Corregido
- Validaciones de entrada en los controladores de autenticación
- Manejo seguro de tokens de restablecimiento


## [0.9.0] - 2025-05-22

### Agregado
- Implementación de la funcionalidad de bloqueo/desbloqueo de usuarios:
  - Bloqueo temporal o permanente de usuarios
  - Registro de la razón del bloqueo
  - Consulta del estado de bloqueo
- Nuevo controlador `UserLockoutController` con endpoints para:
  - Bloquear usuario (`POST /api/users/{userId}/lock`)
  - Desbloquear usuario (`POST /api/users/{userId}/unlock`)
  - Obtener información de bloqueo (`GET /api/users/{userId}/lockout-info`)
- Nuevo DTO `UserLockoutInfo` para la información de bloqueo
- Pruebas unitarias para la funcionalidad de bloqueo/desbloqueo

### Cambiado
- Actualizada la entidad `ApplicationUser` con propiedades para el manejo de bloqueos
- Mejorado el manejo de errores en el `UserService`
- Actualizada la documentación de la API

### Corregido
- Problemas de concurrencia en la gestión de bloqueos
- Validaciones de entrada en los controladores

## [0.8.0] - 2025-05-22

### Agregado
- Implementación del servicio de gestión de roles (`RoleService`):
  - Creación, actualización y eliminación de roles
  - Asignación y revocación de roles a usuarios
  - Gestión de permisos por rol
  - Búsqueda y consulta de roles y sus usuarios
- Controlador `RolesController` con endpoints para:
  - Gestión completa de roles (CRUD)
  - Asignación/revocación de roles a usuarios
  - Consulta de roles por usuario y usuarios por rol
- DTOs para gestión de roles:
  - `RoleDto` para representar roles
  - `CreateRoleDto` para la creación de roles
  - `UpdateRoleDto` para la actualización de roles
- Pruebas unitarias para el `RoleService` y `RolesController`
- Documentación XML para la API de roles

### Cambiado
- Mejorada la estructura de permisos en la aplicación
- Actualizada la documentación de la API con los nuevos endpoints
- Optimizadas las consultas a la base de datos en `RoleService`

### Corregido
- Problemas de concurrencia en la gestión de roles
- Validaciones de entrada en los controladores

## [0.7.0] - 2025-05-22

### Agregado
- Implementación del servicio de gestión de usuarios (`UserService`):
  - Obtención de perfiles de usuario
  - Búsqueda de usuarios con paginación
  - Actualización de perfiles de usuario
- Controlador `UsersController` con endpoints para:
  - Obtener perfil de usuario (`GET /api/users/{userId}`)
  - Buscar usuarios (`GET /api/users/search`)
  - Actualizar perfil de usuario (`PUT /api/users/{userId}`)
- DTOs para perfiles de usuario:
  - `UserProfileDTO` para representar perfiles de usuario
  - `UpdateProfileDTO` para actualización de perfiles
- Pruebas unitarias para el `UserService` y `UsersController`
- Documentación XML para los controladores y servicios
- Configuración de AutoMapper para el mapeo entre entidades y DTOs

### Cambiado
- Mejorado el manejo de errores en los controladores
- Actualizada la documentación de la API con Swagger
- Optimizadas las consultas a la base de datos en `UserService`

### Corregido
- Problemas de referencias nulas en los DTOs
- Validaciones de entrada en los controladores
- Configuración de AutoMapper para el mapeo de perfiles

## [0.6.0] - 2025-05-22

### Agregado
- Implementación completa del sistema de autenticación JWT:
  - Servicio `JwtTokenService` para generación y validación de tokens
  - Servicio `AuthenticationService` para manejo de autenticación y autorización
  - Controlador `AuthController` con endpoints para login, registro y renovación de tokens
- Soporte para refresh tokens con rotación y revocación
- Verificación de correo electrónico con tokens seguros
- Servicio de correo electrónico simulado para desarrollo
- Documentación Swagger/OpenAPI para los endpoints de autenticación
- Configuración de políticas de autorización basadas en roles

### Cambiado
- Actualizada la configuración de autenticación en `Program.cs`
- Mejorado el manejo de errores en los controladores
- Actualizado el ROADMAP.md para reflejar el progreso

### Corregido
- Problemas de configuración de CORS para autenticación
- Validación de tokens JWT en diferentes entornos

## [0.5.0] - 2025-05-22

### Agregado
- Implementación de los patrones Repository y Unit of Work:
  - Interfaz genérica `IRepository<T>` para operaciones CRUD
  - Clase base `Repository<T>` con implementación de las operaciones CRUD
  - Interfaz `IUnitOfWork` para manejar transacciones y repositorios
  - Clase `UnitOfWork` con implementación de la gestión de transacciones
- Configuración de inyección de dependencias para los nuevos servicios
- Soporte para migraciones de base de datos mediante línea de comandos

### Cambiado
- Actualizada la estructura del proyecto para incluir las nuevas interfaces y clases
- Mejorada la gestión de transacciones en la base de datos
- Actualizado `Program.cs` para soportar migraciones mediante línea de comandos

### Corregido
- Problemas de compatibilidad con .NET 9.0.5
- Configuración de la inyección de dependencias para el contexto de base de datos

## [0.4.0] - 2025-05-22

### Agregado
- Sistema de logging estructurado con Serilog:
  - Configuración de sinks para consola y archivos
  - Enriquecimiento de logs con información contextual
  - Niveles de log configurados según entorno
- Monitoreo y métricas con OpenTelemetry: (Eliminado temporalmente por problemas de compatibilidad)
  - Exportador Prometheus configurado
  - Métricas personalizadas para solicitudes activas, duración y errores
  - Dashboard en Grafana para visualización de métricas
- Pruebas unitarias para validadores:
  - Implementación de pruebas para UsuarioValidator
  - Verificación de reglas de validación
- Configuración de contenedores Docker para monitoreo:
  - Prometheus para recolección de métricas
  - Grafana para visualización y dashboards

### Cambiado
- Actualizada la estructura del proyecto para soportar métricas y logging
- Mejorada la configuración de logging en Program.cs
- Actualizado ROADMAP.md para reflejar las nuevas implementaciones

### Corregido
- Problemas de compilación relacionados con dependencias de paquetes
- Referencias nulas en la configuración de Serilog

## [0.3.0] - 2025-05-21

### Agregado
- Entidades de dominio para autenticación:
  - `BaseEntity`: Clase base con propiedades comunes
  - `Usuario`: Entidad para la gestión de usuarios
  - `Rol`: Entidad para la gestión de roles
  - `Permiso`: Entidad para la gestión de permisos
  - `UsuarioRol`: Entidad para la relación muchos a muchos entre Usuario y Rol
  - `RolPermiso`: Entidad para la relación muchos a muchos entre Rol y Permiso
- Configuración de Entity Framework Core:
  - `GestMantIADbContext`: Contexto de base de datos principal
  - Configuraciones de entidades con Fluent API
  - Relaciones y restricciones de base de datos
  - Índices para mejorar el rendimiento
- Actualización de paquetes NuGet a versiones compatibles con .NET 9.0.5
- Actualizado `CHANGELOG.md` con los nuevos cambios
- Actualizado `ROADMAP.md` para reflejar el progreso

## [0.2.0] - 2025-05-21

### Agregado
- Configuración de autenticación JWT para la API
- Sistema de registro de cambios (CHANGELOG.md)
- Script de actualización automática del CHANGELOG
- Documentación detallada de configuración

### Cambiado
- Mejorada la configuración de CORS para soportar autenticación
- Actualizada la documentación del proyecto

### Corregido
- Problemas de codificación de caracteres en la documentación
- Configuración de entorno de desarrollo

## [0.1.0] - 2025-05-21

### Agregado
- Configuración inicial del proyecto
- Estructura básica de la solución con arquitectura limpia
- Configuración de Docker para desarrollo
- Documentación inicial del proyecto

---

## Estructura del Changelog

Cada versión debe documentar:

- **Agregado**: Para nuevas características
- **Cambiado**: Para cambios en funcionalidades existentes
- **Obsoleto**: Para funcionalidades que se eliminarán en futuras versiones
- **Eliminado**: Para funcionalidades eliminadas
- **Corregido**: Para corrección de errores
- **Seguridad**: En caso de vulnerabilidades corregidas
## [2025-06-08] - Cascade AI\n### Mejorado\n- Movido CookieService e ICookieService al proyecto de Infraestructura para mantener una mejor organizaci�n del c�digo.\n- Actualizadas las referencias en AuthController y Program.cs para usar la nueva ubicaci�n.\n\n