# Plan de Refactorización: GestMantIA

Este documento describe las tareas de refactorización planificadas para el proyecto GestMantIA, con el objetivo de mejorar la organización, mantenibilidad y adherencia a los principios SOLID y las mejores prácticas de arquitectura.

## Leyenda de Estado

*   [ ] Pendiente
*   [~] En Progreso
*   [X] Completado
*   [-] Descartado / No aplica

## 1. GestMantIA.API

### 1.1. Estructura y Organización

*   [X] **1.1.1.** Crear la carpeta `Extensions` para agrupar las clases de extensión de configuración de servicios. (Completado según sesión anterior)

## 2. GestMantIA.Core

### 2.1. Limpieza y Organización General

*   [X] **2.1.1.** Eliminar el archivo `Class1.cs` (placeholder no utilizado).
*   [X] **2.1.2.** Mantener la carpeta `Entities` para futuras entidades de dominio. Considerar añadir un `README.md` para explicar su propósito si permanece vacía por mucho tiempo.
*   [X] **2.1.3.** Clarificar el propósito de la carpeta `Models`. Si no tiene un uso definido, considerar su eliminación o renombrar a algo más específico (ej. `ViewModels`, `DTOs` si aplica, aunque los DTOs suelen ir en `Application`). (Decisión: Eliminada por estar vacía y sin propósito claro).

### 2.2. Revisión de Interfaces

*   [ ] **2.2.1.** `IRepository<T>`: Considerar la introducción del patrón Specification para consultas complejas en el futuro, especialmente si los métodos de `ListAsync` con predicados se vuelven demasiado numerosos o complejos.
*   [ ] **2.2.2.** `IUnitOfWork`: Revisar la necesidad de los métodos explícitos de transacción (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`) durante el análisis de los servicios de aplicación. Si `CompleteAsync()` es suficiente para la mayoría de los casos de uso, estos podrían simplificarse o eliminarse de la interfaz para reducir su superficie.

## 3. GestMantIA.Infrastructure

### 3.1. Data (`GestMantIA.Infrastructure.Data`)

*   [X] **3.1.1. (`Elemento 6`) Centralizar Configuración de Índices en Clases `IEntityTypeConfiguration<T>`:**
    *   **Problema**: El método `ConfigurarIndices` en `ApplicationDbContext.cs` define índices directamente.
    *   **Objetivo**: Mover toda la configuración Fluent API, incluidos los índices, a las clases `IEntityTypeConfiguration<T>` dedicadas.
    *   **Pasos**:
        1.  Para `ApplicationUser`, `ApplicationRole`, `RefreshToken`, `SecurityLog`, `SecurityNotification`: mover la definición de sus índices a sus respectivas clases de configuración en `GestMantIA.Infrastructure\Data\Configurations\Identity\` o `GestMantIA.Infrastructure\Data\Configurations\Security\`.
        2.  Eliminar el método `ConfigurarIndices` de `ApplicationDbContext.cs` y su llamada desde `OnModelCreating`.
        3.  Verificar compilación y migraciones.

*   [X] **3.1.2. (`Elemento 7`) Revisar y Eliminar Campo `_currentTransaction` no Utilizado en `ApplicationDbContext`:**
    *   **Problema**: El campo `private IDbContextTransaction? _currentTransaction;` existía pero la lógica de transacción asociada no era invocada directamente, ya que la inyección de dependencias para `IUnitOfWork` resolvía a la clase `UnitOfWork.cs`.
    *   **Solución**: Se eliminó el campo `_currentTransaction` y los métodos de gestión explícita de transacciones (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`, `DisposeTransactionAsync`) de `ApplicationDbContext.cs` para simplificar la clase. La implementación de `IUnitOfWork` en `ApplicationDbContext` (métodos `Repository<T>` y `CompleteAsync`) se mantuvo por ahora, aunque la interfaz `IUnitOfWork` ya no es formalmente implementada por `ApplicationDbContext`.

*   [~] **3.1.3. (`Elemento 8`) Auditoría para `ApplicationUser`:**
    *   **Problema**: `ApplicationUser` hereda de `IdentityUser<Guid>` y no podía heredar también de `BaseEntity` para la auditoría automática. Las propiedades de auditoría se definían directamente en `ApplicationUser`.
    *   **Objetivo**: Estandarizar el manejo de propiedades de auditoría para `ApplicationUser` para que se beneficien del procesamiento automático en `ApplicationDbContext`.
    *   **Pasos**:
        1.  **[X]** Introducir la interfaz `IAuditableEntity` con las propiedades `CreatedAt`, `UpdatedAt`, `IsDeleted`.
        2.  **[X]** Hacer que `BaseEntity` implemente `IAuditableEntity`.
        3.  **[X]** Hacer que `ApplicationUser` implemente `IAuditableEntity` (ya tenía las propiedades, se añadió la declaración de interfaz).
        4.  **[X]** Modificar `ApplicationDbContext.ProcessAuditEntities` para operar sobre `ChangeTracker.Entries<IAuditableEntity>()`.
        5.  **[X]** Revisar `DatabaseInitializer` para simplificar o eliminar el establecimiento manual de estas propiedades si la auditoría automática ahora las cubre completamente. _(Eliminada la asignación manual de `CreatedAt`)_
        6.  **[X]** Asegurar que `IsDeleted`, `DeletedAt` y `DeletedBy` se manejen adecuadamente para el borrado lógico de usuarios en los servicios correspondientes (ej. `UserService`). _(Propiedades `DeletedAt` y `DeletedBy` añadidas a `IAuditableEntity`, `BaseEntity`, `ApplicationUser`. `ApplicationDbContext` actualizado para gestionar `DeletedAt` automáticamente al cambiar `IsDeleted`. `UserService` modificado para gestionar `IsDeleted` y `DeletedBy` en `DeleteUserAsync`, y para filtrar por `IsDeleted` en `GetUserProfileAsync` y `SearchUsersAsync`)._
        7.  **[X]** Considerar si `LockoutEnd` de `IdentityUser` es suficiente o si se necesita una gestión de "desactivación" más explícita (la propiedad `IsActive` ya existe en `ApplicationUser`, revisar su uso y la interacción con `IsDeleted` y `LockoutEnd`). _(Análisis realizado. `IsActive` se mantiene como un flag administrativo. Se implementó `CustomSignInManager` para que `IsDeleted=true` o `IsActive=false` impidan el inicio de sesión. `UserService.DeleteUserAsync` ahora también establece `IsActive=false` en borrado suave)._

### 3.2. Inyección de Dependencias (`GestMantIA.Infrastructure.DependencyInjection`)

*   [X] **3.2.1. (`Elemento 9`) Centralizar Registros de Persistencia en `Infrastructure.DependencyInjection`:**
    *   **Problema**: Los servicios de persistencia (`DbContext`, `IUnitOfWork`, `IDatabaseInitializer`, etc.) se registran actualmente en `GestMantIA.API\Extensions\PersistenceServiceExtensions.cs`.
    *   **Objetivo**: Mejorar la modularidad haciendo que `GestMantIA.Infrastructure\DependencyInjection.cs` sea responsable de registrar todos los servicios que implementa.
    *   **Pasos**:
        1.  Crear un método `private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)` en `GestMantIA.Infrastructure\DependencyInjection.cs`.
        2.  Mover los registros de `ApplicationDbContext`, `IUnitOfWork`, `IDatabaseInitializer`, `services.Configure<SeedDataSettings>()` y `services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>())` de `PersistenceServiceExtensions.cs` (API) al nuevo método `AddPersistence` (Infrastructure).
        3.  Asegurar que `IRepository<>` se registre si se decide que debe ser inyectable directamente (actualmente comentado en API, lo cual es aceptable si se accede vía UoW).
        4.  Llamar a `services.AddPersistence(configuration);` desde el método público `AddInfrastructure` en `GestMantIA.Infrastructure\DependencyInjection.cs`.
        5.  Modificar `GestMantIA.API\Extensions\PersistenceServiceExtensions.cs` para que simplemente llame a `services.AddInfrastructure(configuration);` (o eliminarlo si `Program.cs` llama directamente a `AddInfrastructure`).

## 4. GestMantIA.Application

El objetivo principal para esta capa es establecerla como el núcleo de la lógica de negocio y orquestación de casos de uso, desacoplando la API de la infraestructura y el dominio.

*   [X] **4.1. (`Elemento X`) Crear el proyecto `GestMantIA.Application` como una librería de clases (`net9.0`).** (Estado: Completado)
    *   **Objetivo**: Establecer la estructura base para la capa de aplicación.
    *   **Pasos**:
        1.  Crear un nuevo proyecto de biblioteca de clases .NET llamado `GestMantIA.Application`.
        2.  Definir la estructura de carpetas inicial (e.g., `Features`, `Interfaces`, `DTOs` (si no se usan exclusivamente los de `Shared`), `Mappings`, `Validators`, `Behaviours` para MediatR si se usa).
        3.  Configurar las referencias de proyecto necesarias (a `GestMantIA.Core`, `GestMantIA.Shared`, y paquetes NuGet como `MediatR`, `AutoMapper`, `FluentValidation` si se decide usarlos).
    *   **Estado**: Completado

*   [ ] **4.2. (`Elemento Y`) Refactorizar `IUserService` y su implementación a la capa de Aplicación:**
    *   **Objetivo**: Mover la lógica de aplicación relacionada con la gestión de usuarios de la capa de Infraestructura a la capa de Aplicación.
    *   **Pasos**:
        1.  Decidir la ubicación final de `IUserService` (actualmente en `Core.Identity.Interfaces`). Considerar si moverla a `Application.Interfaces`. (Decisión inicial: mantener en `Core` por ahora para minimizar cambios iniciales, revaluar más tarde).
        2.  Crear una nueva clase (e.g., `ApplicationUserService` o manejadores de MediatR por feature) en `GestMantIA.Application` que implemente `IUserService` o maneje los casos de uso correspondientes.
        3.  Mover la lógica de orquestación, mapeo de DTOs, y construcción de consultas complejas desde `GestMantIA.Infrastructure.Services.UserService` a la nueva implementación en la capa de Aplicación.
        4.  La nueva implementación en `Application` tomará dependencias de `UserManager<ApplicationUser>`, `RoleManager<ApplicationRole>`, `IMapper` (AutoMapper), y, si es necesario, nuevas interfaces de repositorio (definidas en `Core` o `Application.Interfaces`) para operaciones no cubiertas por `UserManager`.
        5.  Revisar y refactorizar el uso directo de `ApplicationDbContext`. El acceso a datos debe ser a través de repositorios o `UserManager`.
        6.  Revisar el uso de `IHttpContextAccessor`. Para la auditoría, considerar pasar el `actingUserId` explícitamente a los métodos de servicio/comando, o centralizar la obtención del `userId` en un servicio de infraestructura/aplicación dedicado si es necesario.
    *   **Estado**: Pendiente

*   [ ] **4.3. (`Elemento Z`) Actualizar la Inyección de Dependencias para los servicios de Aplicación:**
    *   **Objetivo**: Asegurar que las interfaces de servicio de aplicación se resuelvan a sus nuevas implementaciones.
    *   **Pasos**:
        1.  Crear una clase `DependencyInjection.cs` en el proyecto `GestMantIA.Application`.
        2.  Registrar los servicios de aplicación (e.g., `IUserService` con `ApplicationUserService`, o los handlers de MediatR) y otras dependencias de la capa de aplicación (AutoMapper, FluentValidation).
        3.  Llamar al método de extensión `AddApplicationServices(this IServiceCollection services, IConfiguration configuration)` desde `Program.cs` en `GestMantIA.API`.
    *   **Estado**: Pendiente

*   [ ] **4.4. (`Elemento W`) Eliminar/Reducir `GestMantIA.Infrastructure.Services.UserService`:**
    *   **Objetivo**: Limpiar la capa de infraestructura de lógica de aplicación.
    *   **Pasos**:
        1.  Una vez que toda la lógica de aplicación de `UserService` se haya movido a `GestMantIA.Application`, la clase `GestMantIA.Infrastructure.Services.UserService` original puede ser eliminada.
        2.  Si alguna funcionalidad residual es puramente de infraestructura y aún es necesaria (poco probable si se sigue el patrón de delegar a `UserManager`), podría refactorizarse en un nuevo servicio de infraestructura más específico.
    *   **Estado**: Pendiente

## 5. Pruebas

*(Pendiente de revisión y definición de estrategia TDD)*

## CHANGELOG

Ver `CHANGELOG.md` para el historial de cambios implementados.
