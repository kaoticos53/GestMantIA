# Registro de Cambios (Changelog)

Este archivo documenta los cambios notables en el proyecto GestMantIA. El formato está basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

## [Unreleased]

### Changed
- Migrados UserRepository y RoleRepository a la nueva estructura vertical slice, utilizando las interfaces de Identity y eliminando referencias obsoletas.
- Eliminado el repositorio genérico antiguo (`Data.Repositories.Repository<T>`) y actualizado ApplicationDbContext para lanzar NotImplementedException si se solicita un repositorio genérico (alineado con la nueva arquitectura vertical slice).
- Actualizada la inyección de dependencias en Infrastructure para registrar correctamente los nuevos repositorios y servicios de Identity.
- Resueltos todos los errores de compilación en GestMantIA.Infrastructure relacionados con referencias a repositorios y dependencias antiguas.
- Validada la compilación limpia de toda la solución tras la migración.
- Fecha: 2025-05-30


### Changed
- **Actualización del Script de Configuración de Base de Datos Local (`scripts/setup_local_db.sql`)**:
  - Mejorado el script `setup_local_db.sql` para la creación de la base de datos PostgreSQL `gestmantia_dev` y el usuario `gestmantia_user`.
  - El script ahora incluye creación condicional del rol, asignación de propietario a la base de datos, configuración de codificación y colación, y mensajes informativos (`RAISE NOTICE`).
  - Añadidas instrucciones claras para la ejecución del script y pasos posteriores como la creación de la extensión `uuid-ossp`.

### Fixed
- Reinsertado el método de prueba unitaria `GetUserProfileAsync_Should_Return_Null_When_User_Does_Not_Exist` en `ApplicationUserServiceTests.cs` que se había perdido en ediciones anteriores.
- Corregida la indentación de algunos atributos `[Fact]` en `ApplicationUserServiceTests.cs` como efecto secundario de la reinserción del método.
- Verificado que todas las pruebas unitarias en `GestMantIA.Application.UnitTests` pasan correctamente.

### Added
- **Creación y Configuración de Proyectos de Pruebas Unitarias**:
  - Creados los proyectos de pruebas unitarias: `GestMantIA.Core.UnitTests`, `GestMantIA.Application.UnitTests`, `GestMantIA.Infrastructure.UnitTests`, y `GestMantIA.API.UnitTests`.
  - Añadidos los proyectos a la solución `GestMantIA.sln`.
  - Configurados los proyectos para utilizar Gestión Centralizada de Paquetes (CPM).
  - Añadidas referencias a los proyectos de origen correspondientes (ej. `Core.UnitTests` -> `Core`).
  - Incluidos paquetes NuGet esenciales para pruebas: `xUnit`, `Moq`, `FluentAssertions`, y `Microsoft.AspNetCore.Mvc.Testing` (para API tests).
- **Estructuración de Directorios para Pruebas Unitarias**:
  - Creada una estructura de carpetas y archivos de marcador de posición (`*.Tests.cs`) en cada proyecto de pruebas, reflejando la estructura del proyecto de origen.
  - Eliminados los archivos `UnitTest1.cs` por defecto.

### Added
- Entradas de log más detalladas en `ApplicationUserService` para operaciones de asignación y eliminación de roles.

### Added
- **Pruebas Unitarias para Entidades del Core (`GestMantIA.Core.UnitTests`)**:
  - Completadas las pruebas unitarias para las siguientes entidades, cubriendo constructores, propiedades y métodos específicos:
    - `ApplicationUser` (en `UserTests.cs`)
    - `UserProfile`
    - `ApplicationRole`
    - `RefreshToken`
    - `ApplicationPermission`
    - `SecurityAlert` (incluyendo métodos `MarkAsResolved` y `Reopen`)
    - `SecurityLog`
    - `SecurityNotification` (incluyendo método `MarkAsRead`)

### Fixed
- **Errores de Compilación Críticos en la Solución:**
  - Resuelto conflicto `More than one compatible asset found for 'service-worker.published.js'` en `GestMantIA.Web` eliminando el archivo `service-worker.published.js` preexistente de `wwwroot`.
  - Resueltos conflictos `More than one compatible asset found` para `weather.json` y `mudTheme.json` en `GestMantIA.Web` ajustando cómo se referencian estos archivos en `GestMantIA.Web.csproj` (eliminando `weather.json` del `Content` y la entrada explícita `None Update` para `mudTheme.json`).
  - Resuelto error `System.ArgumentException: An item with the same key has already been added` (relacionado con `SwaggerDark.css` y `app.css`) durante la tarea `DiscoverPrecompressedAssets` al deshabilitar la compresión de activos web estáticos (`<EnableStaticWebAssetsCompression>false</EnableStaticWebAssetsCompression>` en `Directory.Build.props`) y asegurar `StaticWebAssetBasePath` en los proyectos API y Web.
- Ambiguity `CS0121` en `GestMantIA.API` eliminando el método duplicado `AddApplicationServices` de `GestMantIA.API.Extensions.ApplicationServiceExtensions.cs`.

### Fixed
- **Resolución de Error de Referencia en `GestMantIA.API` (`CS0234`)**:
  - Se añadió la referencia de proyecto faltante a `GestMantIA.Application` en el archivo `GestMantIA.API.csproj`.
  - Esta acción resolvió el error de compilación `CS0234` ("El tipo o el nombre del espacio de nombres 'Application' no existe en el espacio de nombres 'GestMantIA'"), permitiendo que el proyecto API reconozca y utilice correctamente la capa de Aplicación.

### Fixed
- **Resolución de Errores de Duplicidad en `Infrastructure.UserService` (`GestMantIA.Infrastructure`)**:
  - Se eliminaron las implementaciones duplicadas de múltiples métodos (como `UnlockUserAsync`, `IsUserLockedOutAsync`, `GetUserLockoutInfoAsync`, `CreateUserAsync`, `UpdateUserAsync`, `DeleteUserAsync`, `GetUserByIdAsync`, `GetAllUsersAsync`, `AssignRolesToUserAsync`, `RemoveRolesFromUserAsync`) de `UserService.cs`.
  - Esta acción resolvió los errores de compilación `CS0111` (miembro ya definido) que ocurrían debido a la coexistencia de estos métodos con los stubs que lanzan `NotImplementedException` (destinados a señalar que la lógica principal se ha movido a `ApplicationUserService`).

### Changed
- **Refactorización y Completitud de `ApplicationUserService` (`GestMantIA.Application`)**:
  - Se implementaron todos los métodos de la interfaz `IUserService` en `ApplicationUserService.cs`.
  - Se corrigieron las firmas de los métodos (incluyendo tipos de retorno, parámetros y manejo de nulabilidad) para que coincidan exactamente con `IUserService.cs`.
  - Se aseguró que los métodos que devuelven tipos no nullables (ej. `Task<UserResponseDTO>`) lancen excepciones en lugar de retornar `null` en casos de error o datos no válidos.
  - Se eliminaron de `ApplicationUserService.cs` los métodos que no pertenecían a la interfaz `IUserService` (ej. gestión directa de Roles y Claims, `UpdateUserActiveStatusAsync`).
  - Esta refactorización alinea completamente `ApplicationUserService` con su contrato definido en `IUserService`, resolviendo problemas de compilación y asegurando una implementación coherente.




## [2025-05-29] - Cascade AI

### Added
- **Creación de la Capa de Aplicación (`GestMantIA.Application`)**:
  - Creado el nuevo proyecto `GestMantIA.Application` como una biblioteca de clases .NET, con `TargetFramework net9.0`.
  - Establecida la estructura de carpetas inicial: `Features`, `Interfaces`, `DTOs`, `Mappings`, `Validators`, `Behaviours`.
  - Añadidas referencias de proyecto a `GestMantIA.Core` y `GestMantIA.Shared`.

### Changed
- **Configuración de Paquetes NuGet para `GestMantIA.Application`**:
  - Añadidos los paquetes NuGet `MediatR` (v12.5.0), `AutoMapper` (v14.0.0), `FluentValidation` (v12.0.0), y `FluentValidation.DependencyInjectionExtensions` (v12.0.0) al proyecto `GestMantIA.Application`.
  - Actualizado `Directory.Packages.props` para gestionar centralmente las versiones de `MediatR` y `AutoMapper`, eliminando las referencias a los paquetes de extensión de DI obsoletos (`AutoMapper.Extensions.Microsoft.DependencyInjection` y `MediatR.Extensions.Microsoft.DependencyInjection`).
  - Corregido el archivo `GestMantIA.Application.csproj` para asegurar que las referencias a los paquetes NuGet principales sean correctas.

### Corregido
- **Resolución de Errores de Compilación Post-Refactorización Inicial de Application Layer**:
  - Corregidos múltiples errores CS1061 y CS0117 relacionados con el uso incorrecto de la propiedad `Timestamp` en la entidad `SecurityLog` (debería ser `CreatedAt`) en los proyectos `GestMantIA.Infrastructure` y `GestMantIA.API`.
  - Corregidos errores CS1061 y CS0117 relacionados con el uso incorrecto de la propiedad `Created` en la entidad `RefreshToken` (debería ser `CreatedAt`) en el proyecto `GestMantIA.Infrastructure`.
  - Corregidos errores CS0029 (conversión implícita `DateTimeOffset` a `DateTime`) en `SecurityNotificationService.cs` utilizando `.UtcDateTime`.
  - Corregido error CS1929 (`IWebHostEnvironment` sin `IsDevelopment`) en `DependencyInjection.cs` añadiendo `using Microsoft.Extensions.Hosting;`.
  - Eliminadas directivas `using` duplicadas (CS0105) en `DependencyInjection.cs`.
  - Corregidas advertencias de nulabilidad (CS8609, CS8602) en `CustomSignInManager.cs` ajustando la firma del método y las comprobaciones de nulidad.
- **Modificador de Acceso en `CustomSignInManager`**:
  - Cambiado el modificador de acceso del método `PreSignInCheck` de `public` a `protected` en `CustomSignInManager.cs` para resolver el error de compilación `CS0507`.
### Corregido
- **Resolución Completa de Advertencias de Compilación**:
  - Se verificó y confirmó que todas las advertencias de compilación en la solución completa han sido resueltas.
  - Esto incluye las advertencias CS8618 previamente identificadas en el proyecto `GestMantIA.Shared`.
  - La solución ahora compila sin errores ni advertencias.
### Corregido
- **Resolución de Errores de Compilación en `GestMantIA.API`**:
  - Se abordaron múltiples errores que impedían la compilación del proyecto `GestMantIA.API`:
    - Error `CS0718` en `PersistenceServiceExtensions.cs` (tipos estáticos como argumentos de tipo para `ILogger<T>`). Solucionado utilizando `ILoggerFactory` para crear el logger con un nombre de categoría explícito.
    - Error `CS0246` en `PresentationServiceExtensions.cs` (no se encontraba `TagDescriptionsDocumentFilter`). Solucionado comentando la línea, ya que el archivo de filtro no existía en el proyecto.
    - Error `CS1061` en `Program.cs` (uso incorrecto de `app.Lifetime.EnvironmentName`). Solucionado usando `app.Environment.EnvironmentName`.
    - Error `CS0246` en `PipelineConfigurationExtensions.cs` (no se encontraba `HealthCheckOptions`). Solucionado añadiendo la referencia al paquete `Microsoft.AspNetCore.Diagnostics.HealthChecks` (v2.2.0) y corrigiendo la directiva `using` a `Microsoft.AspNetCore.Diagnostics.HealthChecks`.
    - Error `CS0136` en `PersistenceServiceExtensions.cs` (redeclaración de la variable `loggerFactory`). Solucionado eliminando la declaración duplicada dentro del bloque `catch`.
    - Error `CS1503` en `PersistenceServiceExtensions.cs` (conversión de grupo de métodos a `Action<string>` para `options.LogTo`). Solucionado usando una expresión lambda.
  - El proyecto `GestMantIA.API` ahora compila sin errores.

### Cambiado
- Eliminado el archivo `Class1.cs` (placeholder no utilizado) del proyecto `GestMantIA.Core`.
- Añadido `README.md` a la carpeta `src/GestMantIA.Core/Entities` para documentar su propósito.
- Eliminada la carpeta vacía `src/GestMantIA.Core/Models` por no tener un propósito definido en la arquitectura actual.
- Centralizada la configuración de índices de base de datos en las clases `IEntityTypeConfiguration<T>` correspondientes. Se eliminó el método `ConfigurarIndices` de `ApplicationDbContext` y se verificó/actualizó la configuración de índices para `ApplicationUser`, `ApplicationRole`, `RefreshToken`, `SecurityLog`, `SecurityNotification` y `SecurityAlert` en sus archivos de configuración dedicados.
- Refactorizada la auditoría de entidades para `ApplicationUser`:
  - Creada la interfaz `IAuditableEntity` en `GestMantIA.Core.Interfaces`.
  - `BaseEntity` ahora implementa `IAuditableEntity`.
  - `ApplicationUser` ahora implementa `IAuditableEntity`.
  - `ApplicationDbContext.ProcessAuditEntities` modificado para operar sobre `IAuditableEntity`, permitiendo que `ApplicationUser` sea auditado automáticamente.
- **Mejoras en Borrado Lógico de Usuarios (`UserService.cs`) y finalización de Auditoría de Borrado**:
  - `IAuditableEntity`, `BaseEntity`, `ApplicationUser` ahora incluyen consistentemente `DeletedAt` y `DeletedBy`.
  - `ApplicationDbContext.ProcessAuditEntities` gestiona `DeletedAt` automáticamente al cambiar `IsDeleted`.
  - `UserService.cs`:
    - `DeleteUserAsync` implementa borrado suave por defecto, establece `IsDeleted = true` y `DeletedBy` con el ID del usuario actual (vía `IHttpContextAccessor`).
    - `GetUserProfileAsync` y `SearchUsersAsync` filtran usuarios marcados como eliminados.
    - Logs mejorados en `DeleteUserAsync`.
  - `DatabaseInitializer`: Eliminada asignación manual de `CreatedAt` para usuarios, ahora manejada por la auditoría.
- **Integración de `IsActive` e `IsDeleted` en el Flujo de Inicio de Sesión**:
  - Creado `CustomSignInManager` (en `Infrastructure.Services.Auth`) que hereda de `SignInManager<ApplicationUser>`.
  - `CustomSignInManager.PreSignInCheck` sobrescrito para impedir el inicio de sesión si `ApplicationUser.IsDeleted` es `true` o `ApplicationUser.IsActive` es `false`.
  - `UserService.DeleteUserAsync` ahora establece `IsActive = false` cuando se realiza un borrado suave (soft delete).
  - Registrado `CustomSignInManager` en la configuración de servicios de Identity.
- **Simplificación de `ApplicationDbContext`**:
  - Eliminado el campo `_currentTransaction` y los métodos de gestión explícita de transacciones (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`, `DisposeTransactionAsync`) de `ApplicationDbContext.cs`, ya que no eran utilizados por la implementación de `IUnitOfWork` proporcionada por la DI (`UnitOfWork.cs`).
- **Centralización de Registros de Persistencia**:
  - Se movió la configuración de servicios de persistencia (DbContext, UnitOfWork, DatabaseInitializer, SeedDataSettings) desde `GestMantIA.API.Extensions.PersistenceServiceExtensions` a un nuevo método `AddPersistence` dentro de `GestMantIA.Infrastructure.DependencyInjection`.
  - `Program.cs` (API) ahora llama a `services.AddInfrastructure(configuration)` para incluir estos servicios.
  - `PersistenceServiceExtensions.cs` ha sido marcado como obsoleto.


### Corregido
- **Resolución de Errores de Compilación en Backend (`GestMantIA.API`)**:
  - Se corrigió la llamada al inicializador de base de datos en `Program.cs` para usar `SeedDataAsync()` en lugar del método inexistente `InitializeAsync()`.
  - Se añadió la referencia al paquete NuGet `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` (versión `9.0.0-rc.2.24474.3`) necesario para `AddDbContextCheck`.
  - Se gestionó la versión del paquete a través de la Administración Central de Paquetes (CPM), actualizando `Directory.Packages.props` y eliminando la versión del `.csproj`.
  - Como resultado, el proyecto `GestMantIA.API` ahora compila sin errores. Las 11 advertencias restantes se encuentran en `GestMantIA.Shared`.


### Changed
- **Refactorización de `ApplicationUser` y generación de Claims**:
  - Se eliminó el método `ToClaims()` de la entidad `ApplicationUser`.
  - Se creó la clase `CustomUserClaimsPrincipalFactory` (en `GestMantIA.Infrastructure`) que hereda de `UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>` para centralizar la lógica de generación de claims del usuario.
  - `CustomUserClaimsPrincipalFactory` se registró en el contenedor de DdI.
  - Se actualizó `JwtTokenService` para utilizar `UserManager.CreateUserPrincipalAsync(user)` (que a su vez usa la factoría personalizada) para obtener los claims base del usuario al generar tokens JWT, en lugar de construirlos manualmente. Los claims específicos del token (`Jti`) se siguen añadiendo en `JwtTokenService`.
  - La interfaz `ITokenService` y su implementación `JwtTokenService` fueron actualizadas para que el método `GenerateAccessTokenAsync` sea asíncrono.

### Changed
- Refactorizado `ApplicationDbContext` para mover todas las configuraciones Fluent API a clases de configuración dedicadas en `Infrastructure/Data/Configurations`. `OnModelCreating` ahora usa `ApplyConfigurationsFromAssembly`.

### Added
- Implementado el manejo automático de propiedades de auditoría (`CreatedAt`, `UpdatedAt`, `IsDeleted`) en `ApplicationDbContext` mediante la sobreescritura de `SaveChangesAsync` y `SaveChanges`.
- Entradas de log para la configuración de ApplicationDbContext en `DependencyInjection.cs`.

### Cambiado
- Refactorizado `DependencyInjection.cs` en `GestMantIA.Infrastructure`:
    - Se extrajo la configuración de los servicios de persistencia (`DbContext`, `UnitOfWork`, `DatabaseInitializer`, `SeedDataSettings`) al método `AddPersistenceServices`.
    - Se extrajo la configuración de los servicios de Identity (`Identity<ApplicationUser, ApplicationRole>`, `TokenProviders`, `ITokenService`, `IAuthenticationService`, `IUserService`, `IRoleService`) al método `AddIdentityServices`.
    - Se corrigió la ubicación de los registros de `IUnitOfWork` y `DbContext` para que estén dentro de `AddPersistenceServices`.

### Cambiado
- **Refactorización de `ApplicationDbContext`**:
  - Se eliminó la implementación redundante de la interfaz `IUnitOfWork` de la clase `ApplicationDbContext`.
  - La responsabilidad de la unidad de trabajo ahora recae exclusivamente en la clase `Infrastructure.Data.UnitOfWork`, mejorando la adhesión al Principio de Responsabilidad Única (SRP).


### Corregido
- Se cambió el puerto HTTP de desarrollo de `6000` a `6080` para evitar el error `ERR_UNSAFE_PORT` en navegadores modernos. La aplicación ahora escucha en `http://localhost:6080` (con redirección a HTTPS) y `https://localhost:6001`.

### Cambiado
- Se implementó el tema "Material" para Swagger UI, proporcionando una interfaz más clara y moderna en lugar del tema oscuro anterior.

### Corregido
- **Resolución de Advertencias de Compilación**:
  - Se eliminaron todas las advertencias de compilación restantes en la solución (13 advertencias abordadas).
  - Detalles específicos de las advertencias corregidas:
    - **CS8618** (`UnitOfWork._transaction` se hizo anulable).
    - **CS8601** (`AuthenticationService`: uso de `?? string.Empty` para `token.ReplacedByToken` y propiedades de `UserInfo`).
    - **CS8604** (`RoleService`: comprobación de nulidad para `role.Name` antes de llamar a `_userManager.GetUsersInRoleAsync()`).
    - **CS1998** (`DatabaseInitializer.SeedSampleDataAsync` modificado a no asíncrono y devuelve `Task.CompletedTask`).
    - **CS8603** (`IRoleService.GetRoleByIdAsync` e impl. devuelven `Task<RoleDTO?>`; `IRepository.GetByIdAsync` e impl. devuelven `Task<T?>`).
    - **CS8602** (`RolesController.CreateRole`: comprobación de nulidad para `createdRole`).
    - **ASP0014** (`Program.cs` API: `MapHealthChecks` movido a registro de rutas de nivel superior, eliminado `UseEndpoints`).

### Changed
- **Refactorización y Completitud de `IUserService` y `ApplicationUserService`**:
  - Implementados los métodos restantes de `IUserService` en `ApplicationUserService`: `UpdateUserProfileAsync`, `LockUserAsync`, `UnlockUserAsync`, `IsUserLockedOutAsync`, y `GetUserLockoutInfoAsync`.
  - Se aseguró la alineación con la interfaz `IUserService`, utilizando DTOs apropiados y eliminando el uso del patrón `Result<T>` en la capa de servicio.
  - Eliminados placeholders de métodos obsoletos en `ApplicationUserService`.

### Added
- **Configuración de Inyección de Dependencias para `GestMantIA.Application`**:
  - Creado el archivo `DependencyInjection.cs` en la capa `GestMantIA.Application` para registrar los servicios de aplicación, incluyendo `ApplicationUserService` para `IUserService`.
  - Actualizado `Program.cs` en `GestMantIA.API` para invocar la configuración de servicios de la capa de aplicación y añadir el `using` correspondiente.

### Corregido
- **Resolución de Errores de Compilación (Bloqueo de Migraciones)**:
  - Se resolvieron errores de compilación críticos que impedían la creación de migraciones para `UserProfile`:
    - `GestMantIA.Core`:
      - Eliminado modificador `new` innecesario en métodos `Failure` de `AuthenticationResult`, `RegisterResult` y `VerifyEmailResult`.
      - Añadida directiva `using GestMantIA.Core.Identity.Entities;` a `UserProfile.cs` para resolver `ApplicationUser`.
      - Eliminado modificador `new` innecesario del método `NormalizeName()` en `ApplicationRole.cs`.
      - Ajustados parámetros `errors` y `message` a anulables en `AuthenticationResult.Failure`.
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
  - `RoleDTO` para representar roles
  - `CreateRoleDTO` para la creación de roles
  - `UpdateRoleDTO` para la actualización de roles
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
