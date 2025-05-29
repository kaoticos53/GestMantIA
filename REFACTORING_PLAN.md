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
*   [ ] **2.1.3.** Clarificar el propósito de la carpeta `Models`. Si no tiene un uso definido, considerar su eliminación o renombrar a algo más específico (ej. `ViewModels`, `DTOs` si aplica, aunque los DTOs suelen ir en `Application`).

### 2.2. Revisión de Interfaces

*   [ ] **2.2.1.** `IRepository<T>`: Considerar la introducción del patrón Specification para consultas complejas en el futuro, especialmente si los métodos de `ListAsync` con predicados se vuelven demasiado numerosos o complejos.
*   [ ] **2.2.2.** `IUnitOfWork`: Revisar la necesidad de los métodos explícitos de transacción (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`) durante el análisis de los servicios de aplicación. Si `CompleteAsync()` es suficiente para la mayoría de los casos de uso, estos podrían simplificarse o eliminarse de la interfaz para reducir su superficie.

## 3. GestMantIA.Infrastructure

### 3.1. Data (`GestMantIA.Infrastructure.Data`)

*   [ ] **3.1.1. (`Elemento 6`) Centralizar Configuración de Índices en Clases `IEntityTypeConfiguration<T>`:**
    *   **Problema**: El método `ConfigurarIndices` en `ApplicationDbContext.cs` define índices directamente.
    *   **Objetivo**: Mover toda la configuración Fluent API, incluidos los índices, a las clases `IEntityTypeConfiguration<T>` dedicadas.
    *   **Pasos**:
        1.  Para `ApplicationUser`, `ApplicationRole`, `RefreshToken`, `SecurityLog`, `SecurityNotification`: mover la definición de sus índices a sus respectivas clases de configuración en `GestMantIA.Infrastructure\Data\Configurations\Identity\` o `GestMantIA.Infrastructure\Data\Configurations\Security\`.
        2.  Eliminar el método `ConfigurarIndices` de `ApplicationDbContext.cs` y su llamada desde `OnModelCreating`.
        3.  Verificar compilación y migraciones.

*   [ ] **3.1.2. (`Elemento 7`) Revisar y Eliminar Campo `_currentTransaction` no Utilizado en `ApplicationDbContext`:**
    *   **Problema**: El campo `private IDbContextTransaction? _currentTransaction;` existe y no se utiliza.
    *   **Objetivo**: Limpiar `ApplicationDbContext`.
    *   **Pasos**:
        1.  Confirmar que no hay usos (ya verificado).
        2.  Eliminar el campo.
        3.  Verificar compilación.

*   [ ] **3.1.3. (`Elemento 8`) Auditoría para `ApplicationUser`:**
    *   **Problema**: `ApplicationUser` hereda de `IdentityUser<Guid>` y no de `BaseEntity`, por lo que la auditoría automática de `CreatedAt`, `UpdatedAt`, `IsDeleted` no se aplica. Se establecen manualmente en `DatabaseInitializer`.
    *   **Objetivo**: Estandarizar el manejo de propiedades de auditoría para `ApplicationUser`.
    *   **Pasos (Opción B recomendada)**:
        1.  Añadir propiedades `CreatedAt`, `UpdatedAt`, `IsDeleted` (o `IsActive` si es el campo preferido) a `ApplicationUser` si no existen.
        2.  Modificar `ProcessAuditEntities` en `ApplicationDbContext` para que también procese `ChangeTracker.Entries<ApplicationUser>()` de forma similar a `BaseEntity`.
        3.  Simplificar `DatabaseInitializer` eliminando el establecimiento manual de estas propiedades si la auditoría las cubre.

### 3.2. Inyección de Dependencias (`GestMantIA.Infrastructure.DependencyInjection`)

*   [ ] **3.2.1. (`Elemento 9`) Centralizar Registros de Persistencia en `Infrastructure.DependencyInjection`:**
    *   **Problema**: Los servicios de persistencia (`DbContext`, `IUnitOfWork`, `IDatabaseInitializer`, etc.) se registran actualmente en `GestMantIA.API\Extensions\PersistenceServiceExtensions.cs`.
    *   **Objetivo**: Mejorar la modularidad haciendo que `GestMantIA.Infrastructure\DependencyInjection.cs` sea responsable de registrar todos los servicios que implementa.
    *   **Pasos**:
        1.  Crear un método `private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)` en `GestMantIA.Infrastructure\DependencyInjection.cs`.
        2.  Mover los registros de `ApplicationDbContext`, `IUnitOfWork`, `IDatabaseInitializer`, `services.Configure<SeedDataSettings>()` y `services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>())` de `PersistenceServiceExtensions.cs` (API) al nuevo método `AddPersistence` (Infrastructure).
        3.  Asegurar que `IRepository<>` se registre si se decide que debe ser inyectable directamente (actualmente comentado en API, lo cual es aceptable si se accede vía UoW).
        4.  Llamar a `services.AddPersistence(configuration);` desde el método público `AddInfrastructure` en `GestMantIA.Infrastructure\DependencyInjection.cs`.
        5.  Modificar `GestMantIA.API\Extensions\PersistenceServiceExtensions.cs` para que simplemente llame a `services.AddInfrastructure(configuration);` (o eliminarlo si `Program.cs` llama directamente a `AddInfrastructure`).

## 4. GestMantIA.Application

*(Pendiente de revisión)*

## 5. Pruebas

*(Pendiente de revisión y definición de estrategia TDD)*

## CHANGELOG

Ver `CHANGELOG.md` para el historial de cambios implementados.
