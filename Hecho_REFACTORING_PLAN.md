# Plan de Refactorización de GestMantIA

Este documento describe las áreas identificadas para refactorización en la solución GestMantIA, con el objetivo de mejorar la estructura del código, la mantenibilidad y el cumplimiento de los principios SOLID, especialmente el de Responsabilidad Única.

## Leyenda de Estado
- [ ] Pendiente
- [~] En Proceso
- [X] Completado

---

## 1. GestMantIA.API

### 1.1. `Program.cs`
- **Problema**: El archivo `Program.cs` ha crecido considerablemente debido a la acumulación de configuraciones de servicios, middleware y otras inicializaciones. Esto dificulta su lectura y mantenimiento, y agrupa responsabilidades que podrían separarse.
- **Objetivo**: Reducir el tamaño de `Program.cs` extrayendo configuraciones a métodos de extensión dedicados, mejorando la organización y la legibilidad.
- **Principios SOLID Enfocados**: Responsabilidad Única (SRP), Principio Abierto/Cerrado (OCP) al facilitar la adición de nuevas configuraciones sin modificar excesivamente `Program.cs`.
- **Pasos Detallados**:
    - [X] **Paso 1.1.1**: Crear una carpeta `Extensions` dentro del proyecto `GestMantIA.API` (ruta: `d:\source\Repos\GestMantIA\src\GestMantIA.API\Extensions`) si no existe.
    - [X] **Paso 1.1.2**: Refactorizar la configuración de servicios de persistencia (DbContext, Repositories, UnitOfWork, DatabaseInitializer) en un método de extensión estático `AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)` en un nuevo archivo `ServiceCollectionExtensions.cs` (o un nombre más específico como `PersistenceServiceExtensions.cs`) dentro de la carpeta `Extensions`.
    - [X] **Paso 1.1.3**: Refactorizar la configuración de servicios de la capa de aplicación (MediatR, AutoMapper, FluentValidation, otros servicios de aplicación) en un método de extensión estático `AddApplicationServices(this IServiceCollection services, IConfiguration configuration)` en `ServiceCollectionExtensions.cs` (o `ApplicationServiceExtensions.cs`).
    - [X] **Paso 1.1.4**: Refactorizar la configuración de servicios de Identity y Autenticación/Autorización (`AddIdentityCore`, `AddAuthentication`, `AddJwtBearer`, `AddAuthorization`) en un método de extensión estático `AddSecurityServices(this IServiceCollection services, IConfiguration configuration)` en `ServiceCollectionExtensions.cs` (o `SecurityServiceExtensions.cs`).
    - [X] **Paso 1.1.5**: Refactorizar la configuración de otros servicios específicos de la API (Swagger/OpenAPI, CORS, Controllers, API Versioning si existe, HealthChecks) en un método de extensión estático `AddPresentationServices(this IServiceCollection services, IConfiguration configuration)` en `ServiceCollectionExtensions.cs` (o `PresentationServiceExtensions.cs`).
    - [X] **Paso 1.1.6**: Actualizar `Program.cs` para llamar a estos nuevos métodos de extensión en la sección de configuración de servicios (`builder.Services`).
    - [X] **Paso 1.1.7**: (Opcional, si la sección de configuración del pipeline es extensa) Refactorizar la configuración del pipeline de middleware (ej. `app.UseAuthentication()`, `app.UseAuthorization()`, `app.UseSwagger()`, `app.MapControllers()`, etc.) en un método de extensión estático `ConfigureApiPipeline(this WebApplication app, IWebHostEnvironment env)` en un nuevo archivo `ApplicationBuilderExtensions.cs` (o `PipelineConfigurationExtensions.cs`) dentro de la carpeta `Extensions`.
    - [X] **Paso 1.1.8**: Actualizar `Program.cs` para llamar a este nuevo método de extensión después de crear el objeto `app`.
    - [X] **Paso 1.1.9**: Verificar que la aplicación se compila y se ejecuta correctamente después de los cambios.

---

## 2. GestMantIA.Core

### 2.1. Entidades de Identidad (`GestMantIA.Core\Identity\Entities\`)

#### 2.1.1. `ApplicationUser.cs`
- **Problema**:
    - Duplicación de la propiedad `AccessFailedCount` ya presente en `IdentityUser`.
    - Definición local de propiedades de auditoría (`CreatedAt`, `UpdatedAt`, `IsDeleted`) y `Id` que ya están (o deberían estar) en `BaseEntity`. La propiedad `DeletedAt` también es una propiedad de auditoría común para borrado lógico.
    - El método `ToClaims()` introduce una responsabilidad de conversión de formato de token en la entidad de dominio, violando SRP.
- **Objetivo**:
    - Simplificar `ApplicationUser` eliminando propiedades redundantes y delegando propiedades comunes a `BaseEntity`.
    - Mover la lógica de generación de claims a una capa más apropiada.
- **Principios SOLID Enfocados**: Responsabilidad Única (SRP), No te Repitas (DRY).
- **Pasos Detallados**:
    - [X] **Paso 2.1.1.1**: Asegurar que `ApplicationUser` hereda de `IdentityUser<Guid>` y no de `BaseEntity` directamente, ya que `IdentityUser` es su "base" más específica en el contexto de ASP.NET Core Identity. Las propiedades de `BaseEntity` se integrarán de otra manera o se evaluará si `IdentityUser` ya cubre algunas.
    - [X] **Paso 2.1.1.2**: Eliminar la propiedad `public int AccessFailedCount { get; set; } = 0;` de `ApplicationUser.cs`. Utilizar la propiedad `AccessFailedCount` heredada de `IdentityUser`.
    - [X] **Paso 2.1.1.3**: Revisar las propiedades `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted` en `ApplicationUser`.
        - `Id`: `IdentityUser<Guid>` ya proporciona `public TKey Id { get; set; }`. La inicialización `Id = Guid.NewGuid();` en el constructor de `ApplicationUser` podría entrar en conflicto o ser redundante si `IdentityUser` ya lo maneja. Investigar y simplificar.
        - `CreatedAt`, `UpdatedAt`, `IsDeleted`: Estas son propiedades de auditoría. Considerar si `IdentityUser` ofrece mecanismos equivalentes o si se deben mantener. Si se mantienen, asegurar que no haya conflicto con `BaseEntity` si se decidiera usarla indirectamente (poco probable para `IdentityUser`).
        - `DeletedAt`: Mantener si se usa para borrado lógico y no está cubierta por `IdentityUser`.
    - [X] **Paso 2.1.1.4**: Mover el método `ToClaims()`:
        - Crear una nueva clase, por ejemplo, `UserClaimsFactoryService` o `CustomUserClaimsPrincipalFactory` (si se extiende `UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>`) en el proyecto `GestMantIA.Infrastructure` (o `GestMantIA.API` si es específico de la presentación API).
        - Este servicio tomará un `ApplicationUser` (y posiblemente `UserManager<ApplicationUser>` y `RoleManager<ApplicationRole>`) como entrada y devolverá `IEnumerable<Claim>`.
        - Registrar este nuevo servicio en `Program.cs` (o en la extensión de servicios correspondiente).
        - Actualizar el lugar donde se generan los tokens JWT para que utilice este nuevo servicio en lugar de llamar a `user.ToClaims()`.
    - [X] **Paso 2.1.1.5**: Revisar el constructor de `ApplicationUser` y eliminar inicializaciones de propiedades que ahora se heredan o se manejan de forma diferente (ej. `Id`). (Verificado, sin cambios necesarios)

#### 2.1.2. Otras Entidades de Identidad (`RefreshToken.cs`, `SecurityAlert.cs`, `SecurityLog.cs`, `SecurityNotification.cs`)
- **Problema**: Estas entidades probablemente también definen propiedades (`Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`) que están en `BaseEntity.cs`.
- **Objetivo**: Asegurar que estas entidades hereden de `BaseEntity` para centralizar las propiedades comunes y seguir el principio DRY.
- **Principios SOLID Enfocados**: No te Repitas (DRY).
- **Pasos Detallados**:
    - [X] **Paso 2.1.2.1**: Para cada entidad (`RefreshToken`, `SecurityAlert`, `SecurityLog`, `SecurityNotification`):
        - Modificar la declaración de la clase para que herede de `BaseEntity`.
        - Eliminar las propiedades `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted` (y `DeletedAt` si `BaseEntity` la incluye o se decide añadirla allí) de la definición de la clase, ya que se heredarán.
        - Ajustar los constructores si es necesario.

---

## 3. GestMantIA.Infrastructure

### 3.1. `DependencyInjection.cs`
- **Problema**: El archivo `DependencyInjection.cs` (método `AddInfrastructure`) es muy extenso y configura una amplia gama de servicios de infraestructura, incluyendo persistencia, Identity, servicios de autenticación, AutoMapper y otros servicios. Esto reduce la legibilidad y dificulta el mantenimiento.
- **Objetivo**: Descomponer el método `AddInfrastructure` en varios métodos de extensión más pequeños y cohesivos, cada uno responsable de configurar un conjunto específico de servicios.
- **Principios SOLID Enfocados**: Responsabilidad Única (SRP), Principio Abierto/Cerrado (OCP).
- **Pasos Detallados**:
    - [X] **Paso 3.1.1**: Dentro de `DependencyInjection.cs` (o en un nuevo archivo `PersistenceServiceExtensions.cs` dentro de `GestMantIA.Infrastructure\Extensions` si se prefiere mayor granularidad y esa carpeta se crea):
        - Crear un método de extensión estático `private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)`.
        - Mover la configuración de `SeedDataSettings`, `IDatabaseInitializer`, `ApplicationDbContext`, `IUnitOfWork` y el registro genérico de `DbContext` a este nuevo método.
    - [X] **Paso 3.1.2**: Dentro de `DependencyInjection.cs` (o en un nuevo archivo `IdentityServiceExtensions.cs`):
        - Crear un método de extensión estático `private static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)`.
        - Mover toda la configuración de `services.AddIdentity<ApplicationUser, ApplicationRole>(...)`, `services.Configure<DataProtectionTokenProviderOptions>(...)`, y el registro de `ITokenService`, `IAuthenticationService`, `Core.Identity.Interfaces.IUserService`, `Core.Identity.Interfaces.IRoleService` a este método.
        - *Nota*: La configuración de autenticación JWT (AddAuthentication().AddJwtBearer()) ya se maneja en `Program.cs` (o sus extensiones), lo cual es correcto. Esta sección se enfoca en los servicios de Identity y los servicios personalizados relacionados con la autenticación/usuarios.
    - [X] **Paso 3.1.3**: Dentro de `DependencyInjection.cs` (o en un nuevo archivo `ApplicationUtilityServiceExtensions.cs`):
        - Crear un método de extensión estático `private static IServiceCollection AddApplicationUtilities(this IServiceCollection services, IConfiguration configuration)`.
        - Mover la configuración de AutoMapper y el registro de `IEmailService` (y otros servicios de utilidad general de infraestructura) a este método.
    - [X] **Paso 3.1.4**: Modificar el método público `public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)` para que llame a estos nuevos métodos privados (o públicos si están en archivos separados):
      ```csharp
      public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
      {
          services
              .AddPersistence(configuration)
              .AddIdentityServices(configuration)
              .AddApplicationUtilities(configuration);
          // ... cualquier otro servicio que quede directamente aquí o llamadas a otras extensiones ...
          return services;
      }
      ```
    - [X] **Paso 3.1.5**: Asegurar que todos los `using` necesarios estén presentes en la parte superior de los archivos donde se definan los nuevos métodos de extensión.
    - [X] **Paso 3.1.6**: (Opcional) Si se crean nuevos archivos de extensión (ej. `PersistenceServiceExtensions.cs`), asegurarse de que estén en el namespace correcto (ej. `GestMantIA.Infrastructure` o `GestMantIA.Infrastructure.Extensions`). (NA - No se crearon archivos separados)

---

### 3.2. `Data\ApplicationDbContext.cs`
- **Problema**:
    - El método `OnModelCreating` es muy extenso y contiene numerosas configuraciones Fluent API directamente, en lugar de delegarlas completamente a clases `IEntityTypeConfiguration<T>`.
    - Un método privado `ConfigurarIndices` también define configuraciones de índices que deberían estar con sus respectivas entidades.
    - No hay una sobreescritura visible de `SaveChangesAsync()` para manejar propiedades de auditoría (como `CreatedAt`, `UpdatedAt` de `BaseEntity`), lo cual es una práctica común si se espera que estas se actualicen automáticamente al guardar.
    - Implementación redundante de `IUnitOfWork` cuando `Infrastructure.Data.UnitOfWork` es la implementación principal registrada.
- **Objetivo**:
    - Limpiar `OnModelCreating` moviendo todas las configuraciones de entidades a clases `IEntityTypeConfiguration<T>` dedicadas en el directorio `GestMantIA.Infrastructure\Data\Configurations`.
    - Asegurar que las propiedades de auditoría se manejen correctamente, preferiblemente mediante una sobreescritura de `SaveChangesAsync()`.
- **Principios SOLID Enfocados**: Responsabilidad Única (SRP) para `ApplicationDbContext` y para cada clase de configuración.
- **Pasos Detallados**:
    - [X] **Paso 3.2.1**: Para cada entidad que actualmente tiene su configuración Fluent API directamente en `OnModelCreating` o en `ConfigurarIndices` (ej. `IdentityUserLogin<Guid>`, `ApplicationUserRole`, `ApplicationUser`, `ApplicationRole`, `IdentityUserClaim<Guid>`, `IdentityRoleClaim<Guid>`, `IdentityUserToken<Guid>`, `RefreshToken`, `ApplicationPermission`, `ApplicationRolePermission`, `SecurityLog`, `SecurityNotification`, `SecurityAlert`):
        - Crear una nueva clase de configuración en el directorio `GestMantIA.Infrastructure\Data\Configurations` (o subdirectorios como `Identity` o `Security` si es apropiado) que implemente `IEntityTypeConfiguration<TEntity>`. Por ejemplo, `SecurityLogConfiguration.cs`.
        - Mover toda la configuración Fluent API relevante para esa entidad (incluyendo `ToTable()`, claves, relaciones, índices, tipos de columna, etc.) desde `OnModelCreating` y `ConfigurarIndices` al método `Configure(EntityTypeBuilder<TEntity> builder)` de su nueva clase de configuración.
    - [X] **Paso 3.2.2**: Eliminar el método privado `ConfigurarIndices` y las configuraciones Fluent API individuales de `OnModelCreating`.
    - [X] **Paso 3.2.3**: Modificar `OnModelCreating` para que registre todas las configuraciones de una manera más limpia. La forma preferida es usar:
      ```csharp
      protected override void OnModelCreating(ModelBuilder builder)
      {
          base.OnModelCreating(builder);
          builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
      }
      ```
      Esto cargará automáticamente todas las clases que implementan `IEntityTypeConfiguration<T>` del ensamblado donde reside `ApplicationDbContext`. Alternativamente, se pueden aplicar una por una si se prefiere un control más explícito: `builder.ApplyConfiguration(new MiEntidadConfiguration());`.
    - [X] **Paso 3.2.4**: Investigar cómo se están manejando actualmente las propiedades de auditoría (`CreatedAt`, `UpdatedAt`, `IsDeleted` de `BaseEntity`) para las entidades que heredan de ella (ej. `RefreshToken`, `SecurityLog`, etc.).
    - [X] **Paso 3.2.5**: Si las propiedades de auditoría no se están actualizando automáticamente, sobreescribir el método `SaveChangesAsync(CancellationToken cancellationToken = default)` en `ApplicationDbContext`. En esta sobreescritura:
        - Iterar sobre `ChangeTracker.Entries<BaseEntity>()`.
        - Para las entidades añadidas (`EntityState.Added`), establecer `CreatedAt` y `UpdatedAt` a la fecha y hora actuales.
        - Para las entidades modificadas (`EntityState.Modified`), establecer `UpdatedAt` a la fecha y hora actuales.
        - Considerar el manejo de `IsDeleted` si se implementa un borrado lógico a través de este mecanismo.
    - [X] **Paso 3.2.6**: Asegurar que todas las clases de configuración nuevas tengan los `using` necesarios y estén en el namespace correcto.
    - [X] **Paso 3.2.7**: Eliminar la interfaz `IUnitOfWork` de la declaración de la clase `ApplicationDbContext` y los métodos/miembros asociados a su implementación directa (como `_currentTransaction`, `Repository<T>()`, `CompleteAsync()`, `BeginTransactionAsync()`, `CommitTransactionAsync()`, `RollbackTransactionAsync()`), ya que `Infrastructure.Data.UnitOfWork` es la implementación registrada y utilizada.

---
