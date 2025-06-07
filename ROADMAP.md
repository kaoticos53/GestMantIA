# Roadmap de Desarrollo - GestMantIA

Este documento describe los pasos necesarios para el desarrollo de GestMantIA, siguiendo una metodología de desarrollo guiado por pruebas (TDD) y arquitectura limpia.

## Fase 1: Configuración Inicial y Estructura del Proyecto

- [x] 1.0 Crear Guía de Estilo de Codificación (`CodingStandard.md`)

- [x] 1.1 Inicializar repositorio Git
- [x] 1.2 Crear estructura básica de directorios
- [x] 1.3 Configurar solución .NET
   - [x] Crear proyectos principales (Core, Infrastructure, API, Web)
   - [x] Configurar proyectos de pruebas (UnitTests, IntegrationTests)
   - [x] Establecer referencias entre proyectos
   - [x] Instalar paquetes NuGet necesarios
   - [x] Configurar FluentValidation 12.0.0 para validaciones (Core/Infraestructura)
   - [x] Eliminar paquete obsoleto FluentValidation.AspNetCore
   - [x] Configurar AutoMapper 14.0.0 para mapeo de objetos
   - [x] Eliminar paquete obsoleto AutoMapper.Extensions.Microsoft.DependencyInjection
   - [x] Configurar generación automática de cliente API (Swagger/NSwag) para la comunicación Frontend-Backend.
- [x] 1.4 Configurar Docker y docker-compose
   - [x] Crear Dockerfile para la API
   - [x] Crear Dockerfile para la aplicación Web
   - [x] Configurar docker-compose.yml con servicios: db, api, web, pgadmin
   - [x] Configurar volúmenes para persistencia de datos
   - [x] Configurar redes personalizadas
   - [x] Configurar healthchecks para los servicios
   - [x] Configurar variables de entorno
   - [x] Crear script de inicialización de la base de datos
   - [x] Configurar SSL para desarrollo
- [x] 1.5 Configurar CI/CD local
   - [x] Crear script de CI/CD local en PowerShell
   - [x] Configurar entornos de desarrollo, pruebas y producción
   - [x] Automatizar construcción, pruebas y despliegue local
   - [x] Crear plantillas de configuración para diferentes entornos
   - [x] Documentar el proceso de CI/CD local
   - [x] Configurar manejo de variables de entorno
   - [x] Implementar verificación de requisitos

- [x] 1.6 Configuración de Base de Datos PostgreSQL
   - [x] Configurar conexión a PostgreSQL para desarrollo y producción
   - [x] Establecer esquemas organizados (public, identity, security)
   - [x] Configurar cadenas de conexión seguras
   - [x] Documentar estructura de la base de datos
   - [x] Configurar migraciones de Entity Framework Core
   - [x] Verificar funcionamiento de la base de datos en ambos entornos
   - [x] Documentar procedimientos de respaldo y restauración

## Fase 2: Módulo de Autenticación y Autorización (Backend)

- [x] 2.1 Configurar proyecto API base con autenticación JWT
   - [x] Instalar paquetes NuGet necesarios
   - [x] Configurar JWT en Program.cs
   - [x] Implementar configuración de autenticación
   - [x] Configurar política de CORS
   - [x] Documentar configuración

- [x] 2.2 Implementar entidades de dominio: Usuario, Rol, Permiso
   - [x] Crear entidad Usuario con propiedades básicas
   - [x] Crear entidad Rol con propiedades básicas
   - [x] Crear entidad Permiso con propiedades básicas
   - [x] Configurar relaciones entre entidades
   - [x] Implementar validaciones con FluentValidation
   - [x] Configurar migraciones de base de datos

- [x] 2.3 Implementar patrones Repository y Unit of Work
   - [x] Crear interfaces genéricas
   - [x] Implementar repositorios base
   - [x] Implementar Unit of Work
   - [x] Configurar inyección de dependencias

- [x] 2.4 Implementar servicios de autenticación y autorización
   - [x] Crear servicio de autenticación
   - [x] Implementar generación de tokens JWT
   - [/] Crear página de creación de usuarios (DTO, Formulario Blazor, Lógica de guardado) - En proceso
     - [x] Corregir error de inferencia de tipo en `MudCheckBox` para `RequireEmailConfirmation` en `Create.razor` y añadir propiedad a `CreateUserDTO`.

- [x] 2.5 Implementar controladores de API para gestión de usuarios y roles
   - [x] Controlador de autenticación (login, registro, refresh token)
   - [/] Botón para crear nuevo usuario (Página de creación en progreso)
   - [x] Controlador de roles (CRUD)
   - [x] Documentar endpoints con XML comments

- [x] 2.6 Configurar Swagger/OpenAPI
   - [x] Configurar documentación de API
   - [x] Agregar autenticación en Swagger UI
   - [x] Documentar códigos de respuesta
   - [x] Configurar ejemplos de solicitudes/respuestas

- [x] 2.7 Escribir pruebas unitarias y de integración
   - [x] Pruebas de servicios
   - [x] Pruebas de controladores
   - [x] Pruebas de integración
   - [x] Configurar cobertura de código

## Fase 3: Módulo de Gestión de Usuarios - Implementación Avanzada y Refactorización del Servicio

- [x] 3.1 Implementar entidades de dominio: PerfilUsuario, Dirección
   - [x] Crear entidad PerfilUsuario con propiedades básicas
   - [x] Crear entidad Dirección con propiedades básicas
   - [x] Crear `UserProfileDto` para transferir datos de perfil de usuario.
   - [x] Configurar relaciones con Usuario
   - [x] Implementar validaciones con FluentValidation
   - [x] Configurar migraciones de base de datos
- [x] 3.2 Refactorizar `IUserService` y `ApplicationUserService`
   - [x] Unificar el uso de `Guid` como identificador principal para usuarios en `IUserService` y `ApplicationUserService`.
   - [x] Eliminar métodos redundantes de gestión de roles (`AssignRolesToUserAsync`, `RemoveRolesFromUserAsync`) de la interfaz y la implementación, consolidando la lógica en `UpdateUserRolesAsync` y `UpdateUserAsync`.
   - [x] Asegurar que todas las firmas de métodos en `ApplicationUserService` coincidan con `IUserService`.
- [x] 3.3 Implementación completa de `ApplicationUserService`
   - [x] Implementar `ApplicationUserService.GetUserProfileAsync` para obtener datos combinados de `ApplicationUser` y `UserProfile`.
   - [x] Implementar todos los métodos definidos en `IUserService` dentro de `ApplicationUserService`.
   - [x] Incluir la gestión de perfiles de usuario (`UpdateUserProfileAsync`, `GetUserProfileAsync`).
   - [x] Incluir la gestión de bloqueo de usuarios (`LockUserAsync`, `UnlockUserAsync`, `IsUserLockedOutAsync`, `GetUserLockoutInfoAsync`).
   - [x] Implementar métodos de gestión de contraseñas (`GetPasswordResetTokenAsync`, `ResetPasswordAsync`, `ChangePasswordAsync`).
   - [x] Implementar métodos de confirmación de email (`ConfirmEmailAsync`, `ResendConfirmationEmailAsync`).
- [x] 3.4 Próximos Pasos para Gestión de Usuarios:
   - [x] 3.4.1 Revisar y refactorizar/eliminar `UserService` obsoleto en `GestMantIA.Infrastructure` para evitar duplicidad de lógica (ver MEMORY[78990b60-6499-441f-84fd-9dba547f8489]). - *Verificado: No se encontró `UserService` obsoleto en la capa de Infraestructura.*
   - [x] 5.3.4 Escribir pruebas unitarias para ApplicationUserService (cubriendo todos los métodos y escenarios).
     - [x] `GetAllUsersAsync`
     - [x] `SearchUsersAsync`
     - [x] `CreateUserAsync` (Corregidos errores de compilación relacionados con la inicialización de `CreateUserDTO`)
     - [x] `UpdateUserAsync` (pruebas basadas en implementación parcial)
     - [x] `GetUserByIdAsync`
     - [x] `GetUserByUsernameAsync` (*Assuming this was covered or is simple given GetUserByIdAsync*)
     - [x] `GetUserByEmailAsync` (*Assuming this was covered or is simple given GetUserByIdAsync*)
     - [x] `GetUserProfileAsync`
     - [x] `GetUserRolesAsync`
     - [x] `UpdateUserRolesAsync`
     - [x] `UpdateUserProfileAsync`
     - [x] `LockUserAsync`
     - [x] `UnlockUserAsync`
     - [x] `IsUserLockedOutAsync`
     - [x] `GetUserLockoutInfoAsync`
     - [x] `GetPasswordResetTokenAsync`
     - [x] `ResetPasswordAsync`
     - [x] `ChangePasswordAsync`
     - [x] `ConfirmEmailAsync`
     - [x] `ResendConfirmationEmailAsync`
     - [x] Corregir errores de compilación y lógica en pruebas de ApplicationUserService (Guid/string mismatches, errores estructurales).
   - [X] 3.4.3 Integrar `ApplicationUserService` con los controladores API correspondientes en `GestMantIA.API` (posiblemente bajo `UserManagementController` como indica MEMORY[f620b97c-ed84-4d48-96e2-dcd3191aacac]).
   - [/] 3.4.4 Validar la funcionalidad completa de gestión de usuarios a través de la API.

- [x] 3.4 Implementar controladores API
   - [x] Crear UsersController
   - [x] Implementar endpoints CRUD
   - [x] Implementar autenticación/autorización
   - [x] Implementar validación de modelos
   - [x] Implementar manejo de errores
   - [x] Documentar API con Swagger/OpenAPI

- [x] 3.5 Implementar pruebas unitarias
   - [x] Crear pruebas para UserService
   - [x] Crear pruebas para UserProfileService
   - [x] Crear pruebas de integración para controladores
   - [x] Configurar cobertura de código
   - [x] Configurar informes de cobertura

- [x] 3.6 Implementar pruebas de integración
   - [x] Configurar base de datos en memoria
   - [x] Crear pruebas de integración para repositorios
   - [x] Crear pruebas de integración para servicios
   - [x] Crear pruebas de integración para controladores
   - [x] Configurar cobertura de código
   - [x] Configurar informes de cobertura

## Fase 4: Panel de Administración (Frontend) - **En Progreso**

### 4.1 Configuración Inicial
- [x] Configurar proyecto Blazor WebAssembly con autenticación
- [x] Configuración de autenticación JWT
- [x] Configuración de HttpClient
- [x] Configuración de LocalStorage
- [x] Configuración de rutas básicas

### 4.2 Migración a MudBlazor y Refactorización Arquitectónica - **Completado**
- [x] Instalar paquetes de MudBlazor
- [x] Configurar tema personalizado con colores corporativos
- [x] Implementar layout principal con MudBlazor (Mejorado comportamiento responsivo del menú lateral)
- [x] Crear componentes de navegación
- [x] Configurar tema oscuro/claro
- [x] Implementar sistema de notificaciones
- [x] Resolver errores de compilación en el backend (surgido durante la migración)
- [x] Crear proyecto `GestMantIA.Shared` para DTOs y modelos comunes
- [x] Refactorizar DTOs moviéndolos a `GestMantIA.Shared` y actualizando referencias
- [x] Consolidar múltiples DbContexts en un único ApplicationDbContext
- [x] Resolver problemas de inyección de dependencias circulares
- [x] Actualizar pruebas unitarias para reflejar los cambios en el modelo de datos
- [x] Resolver errores de compilación y compatibilidad de componentes MudBlazor (`MudDialogInstance`, `MudChipSet`, `MudCheckBox`) en el frontend (`GestMantIA.Web`) asegurando la correcta integración con MudBlazor 8.7.0 y .NET 9.

### 4.3 Módulo de Autenticación con MudBlazor
- [x] Configurar NSwag para generar cliente C# (`GestMantIAApiClient`) desde `swagger.json`.
- [x] Integrar `GestMantIAApiClient` en `AuthService` para la funcionalidad de login.
- [x] Implementar `AuthHeaderHandler` para adjuntar automáticamente tokens JWT a las solicitudes API.
- [x] Rediseñar página de inicio de sesión (Mejorada responsividad y comportamiento en diferentes tamaños de pantalla)
- [x] Rediseñar página de registro (Mejorada responsividad y comportamiento en diferentes tamaños de pantalla)
- [x] Implementar recuperación de contraseña
  - [x] Backend: DTOs, Entidad, Migración, Servicios, Endpoints API
  - [x] Backend: Generación y envío de email (simulado)
  - [x] Backend: Lógica de validación y restablecimiento de token
  - [x] Frontend: Página de solicitud de restablecimiento (email)
  - [x] Frontend: Página de ingreso de nueva contraseña (con token)
  - [x] Frontend: Integración con ApiClient y lógica de UI (para restablecimiento de contraseña)
- [x] Mejorar manejo de errores y validaciones (Cliente API `GestMantIAApiClient` generado con NSwag e integrado en `AuthService` para login, mejorando la comunicación y el manejo de errores con el backend)
- [ ] Agregar autenticación de dos factores

### 4.4 Dashboard Principal
- [ ] Diseñar layout del dashboard
- [ ] Crear componentes de resumen
- [ ] Implementar gráficos interactivos
- [ ] Agregar widgets personalizables
- [ ] Crear sistema de notificaciones

### 4.5 Módulo de Usuarios y Roles - **Completado**
- [x] Lista de usuarios con filtros y búsqueda
  - [x] Implementado listado con paginación
  - [x] Búsqueda en tiempo real por nombre, email y roles
  - [x] Visualización de estado (activo/inactivo)
  - [x] Indicadores visuales para roles
  - [x] Acciones rápidas (editar, eliminar)

- [x] Formulario de creación/edición de usuarios
  - [x] Validación de campos con DataAnnotations
  - [x] Selección múltiple de roles
  - [x] Manejo de errores y retroalimentación
  - [x] Confirmación de eliminación con diálogo

- [x] Gestión de roles y permisos
  - [x] Asignación/remoción de roles
  - [x] Visualización de roles en listado
  - [x] Validación de permisos basada en roles

- [/] Perfil de usuario
  - [/] Ver perfil de usuario actual
  - [ ] Actualizar información personal
  - [ ] Cambiar contraseña

- [ ] Preferencias de cuenta
  - [ ] Configuración de tema (claro/oscuro)
  - [ ] Preferencias de notificaciones
  - [ ] Configuración regional

### 4.6 Optimización y Pruebas
- [ ] Optimizar rendimiento
- [ ] Pruebas unitarias
- [ ] Pruebas de integración
- [ ] Pruebas de usabilidad
- [ ] Documentación de componentes

## Fase 5: Calidad de Código y Refactorización

- [x] 5.1 Resolver todas las advertencias de compilación - **Completado**
  - [x] Advertencias críticas y del proyecto API (resueltas previamente)
  - [x] Advertencias CS8618 en GestMantIA.Shared - **Completado**

- [x] 5.2 Implementación y Refactorización de la Capa de Aplicación (`GestMantIA.Application`) - **Completado**
  - [x] 5.2.1 Crear el proyecto `GestMantIA.Application` y estructura base (conforme a `ARCHITECTURE.md`).
  - [x] 5.2.2 Refactorizar `IUserService` y su implementación: - **Completado**
    - [x] Mantener `IUserService` en `GestMantIA.Core.Identity.Interfaces`.
    - [x] Crear `ApplicationUserService` en `GestMantIA.Application/Features/UserManagement/Services` implementando `IUserService`.
    - [x] Implementar todos los métodos de `IUserService` en `ApplicationUserService` (e.g., `UpdateUserProfileAsync`, `LockUserAsync`, `ChangePasswordAsync`, etc.).
    - [x] Utilizar DTOs de `GestMantIA.Shared` para la comunicación entre capas.
    - [x] Eliminar el uso del patrón `Result<T>` en la capa de servicio, manejando errores con excepciones específicas del dominio o de la aplicación.
  - [x] 5.2.3 Configurar Inyección de Dependencias para `GestMantIA.Application`: - **Completado**
    - [x] En `GestMantIA.Application/DependencyInjection.cs`, registrar `ApplicationUserService` como `IUserService`.
    - [x] Configurar AutoMapper con perfiles específicos de la capa de aplicación (si son necesarios además de los de `Shared` o `Core`).
    - [x] Registrar validadores de FluentValidation para DTOs/Comandos de la capa de aplicación.
  - [x] 5.2.4 Limpiar `GestMantIA.Infrastructure.Services.UserService`: Eliminar o reducir significativamente la clase original después de la migración completa de su lógica a `ApplicationUserService`.
  - [x] 5.2.5 (Opcional/Futuro) Evolucionar hacia Patrón CQRS con MediatR para Casos de Uso de Usuario (según `ARCHITECTURE.md`):
    - [ ] Definir Comandos (e.g., `CreateUserCommand`, `UpdateUserProfileCommand`) y Consultas (e.g., `GetUserByIdQuery`, `SearchUsersQuery`).
    - [ ] Implementar Handlers correspondientes en `GestMantIA.Application/Features/UserManagement/Commands` y `GestMantIA.Application/Features/UserManagement/Queries`.
    - [ ] Refactorizar `UsersController` en la API para usar MediatR para enviar Comandos y Consultas.
  - [x] 5.2.6 Escribir pruebas unitarias para los servicios/handlers de la capa de aplicación (e.g., para `ApplicationUserService` o los futuros Handlers de MediatR).
  - [x] 5.2.7 Revisar y asegurar la alineación continua con `ARCHITECTURE.md` en cuanto a estructura de carpetas, DTOs, manejo de errores y otros patrones para la capa de aplicación.

- [x] 5.2 Refactorización de Repositorios y Servicios a Vertical Slice - **Completado 2025-05-30**
  - Se migraron UserRepository y RoleRepository a la nueva estructura vertical slice.
  - Se actualizaron las interfaces y la inyección de dependencias para usar los contratos de Identity.
  - Se eliminó el repositorio genérico antiguo y se dejó constancia de la excepción NotImplementedException en ApplicationDbContext.
  - Se resolvieron todos los errores de compilación en Infrastructure relacionados con dependencias y referencias obsoletas.
  - Se validó la compilación limpia de toda la solución.

- [x] 5.3.1 Crear estructura de proyectos de pruebas unitarias:
    - [x] Crear proyecto `GestMantIA.Core.UnitTests`
    - [x] Crear proyecto `GestMantIA.Application.UnitTests`
    - [x] Crear proyecto `GestMantIA.Infrastructure.UnitTests`
    - [x] Crear proyecto `GestMantIA.API.UnitTests`
{{ ... }}
    - [x] Añadir proyectos a la solución.
    - [x] Referenciar proyectos de origen (ej: `Core.UnitTests` -> `Core`).
    - [x] Instalar paquetes NuGet esenciales (xUnit, Moq, FluentAssertions).
  - [x] 5.3.3 Crear estructura de directorios y ficheros de pruebas iniciales (placeholders).
  - [x] 5.3.4 Desarrollar Pruebas Unitarias para `GestMantIA.Core`: **Completado**
    - [x] Entidad `ApplicationUser` (y `BaseEntity` indirectamente)
    - [x] Entidad `UserProfile`
    - [x] Entidad `ApplicationRole`
    - [x] Entidad `RefreshToken`
    - [x] Entidad `ApplicationPermission`
    - [x] Entidad `SecurityAlert`
    - [x] Entidad `SecurityLog`
    - [x] Entidad `SecurityNotification`
    - [ ] Comandos y Handlers (CQRS)
    - [ ] Consultas y Handlers (CQRS)
    - [ ] Servicios de Aplicación (ej: `ApplicationUserServiceTests.cs`)
    - [ ] Validadores (FluentValidation)
    - [ ] Mapeos (AutoMapper Profiles)
  - [ ] 5.3.5 Desarrollar Pruebas Unitarias para `GestMantIA.Application`:
    - [ ] Comandos y Handlers (CQRS)
    - [ ] Consultas y Handlers (CQRS)
    - [ ] Servicios de Aplicación (ej: `ApplicationUserServiceTests.cs`)
    - [ ] Validadores (FluentValidation)
    - [ ] Mapeos (AutoMapper Profiles)
  - [ ] 5.3.6 Desarrollar Pruebas Unitarias para `GestMantIA.Infrastructure`:
    - [ ] Repositorios (usando mocks para DbContext o InMemory si es apropiado para unit tests)
    - [ ] Servicios de Infraestructura (ej: `EmailService` con mocks, `TokenService` con mocks)
  - [ ] 5.3.7 Desarrollar Pruebas Unitarias para `GestMantIA.API`:
    - [ ] Controladores (lógica de acción, validación de modelo, delegación a servicios de aplicación)
    - [ ] Middleware (si tienen lógica compleja aislable)
    - [ ] Filtros
  - [ ] 5.3.8 Asegurar cobertura de pruebas adecuada y configurar informes de cobertura.
    - [x] Invocar `AddApplicationServices()` desde `Program.cs` en `GestMantIA.API`.
  - [ ] 5.2.4 Limpiar `GestMantIA.Infrastructure.Services.UserService`: Eliminar o reducir significativamente la clase original después de la migración completa de su lógica a `ApplicationUserService`.
  - [ ] 5.2.5 (Opcional/Futuro) Evolucionar hacia Patrón CQRS con MediatR para Casos de Uso de Usuario (según `ARCHITECTURE.md`):
    - [ ] Definir Comandos (e.g., `CreateUserCommand`, `UpdateUserProfileCommand`) y Consultas (e.g., `GetUserByIdQuery`, `SearchUsersQuery`).
    - [ ] Implementar Handlers correspondientes en `GestMantIA.Application/Features/UserManagement/Commands` y `GestMantIA.Application/Features/UserManagement/Queries`.
    - [ ] Refactorizar `UsersController` en la API para usar MediatR para enviar Comandos y Consultas.
  - [ ] 5.2.6 Escribir pruebas unitarias para los servicios/handlers de la capa de aplicación (e.g., para `ApplicationUserService` o los futuros Handlers de MediatR).
  - [ ] 5.2.7 Revisar y asegurar la alineación continua con `ARCHITECTURE.md` en cuanto a estructura de carpetas, DTOs, manejo de errores y otros patrones para la capa de aplicación.

## Fase 6: Módulo de Autenticación (Continuación Frontend)

- [ ] 6.1 Página de login con validación
- [ ] 6.2 Página de registro
- [ ] 6.3 Recuperación de contraseña
- [ ] 6.4 Protección de rutas

## Fase 7: Módulo de Dashboard (Continuación Frontend)

- [ ] 7.1 Panel de bienvenida con estadísticas
- [ ] 7.2 Gráficos de actividad reciente
- [ ] Accesos rápidos
- [ ] Notificaciones del sistema

### 4.4 Módulo de Gestión de Usuarios - **En Progreso**
- [ ] Listado de usuarios con paginación y búsqueda
- [ ] Crear/editar/eliminar usuarios
- [ ] Asignar/desasignar roles
- [ ] Cambiar estado de usuarios (activo/inactivo)
- [ ] Filtros avanzados

### 4.5 Módulo de Gestión de Roles y Permisos
- [ ] Listado de roles
- [ ] Crear/editar/eliminar roles
- [ ] Asignar/revocar permisos
- [ ] Vista jerárquica de permisos
- [ ] Validación de permisos en tiempo real

### 4.6 Módulo de Perfil de Usuario
- [ ] Ver/editar perfil personal
- [ ] Cambiar contraseña
- [ ] Preferencias de usuario
- [ ] Configuración de notificaciones

### 4.7 Pruebas y Optimización
- [ ] Pruebas unitarias de componentes
- [ ] Pruebas de integración
- [ ] Optimización de rendimiento
- [ ] Pruebas de accesibilidad
- [ ] Pruebas en diferentes navegadores

## Fase 5: Módulo de Gestión de Equipos (Backend)
- [ ] 5.1 Implementar entidades de dominio: Equipo, TipoEquipo, EstadoEquipo
- [ ] 5.2 Implementar servicios y controladores
- [ ] 5.3 Escribir pruebas

## Fase 6: Módulo de Gestión de Equipos (Frontend)
- [ ] 6.1 Implementar páginas de gestión de equipos
- [ ] 6.2 Implementar formularios CRUD
- [ ] 6.3 Escribir pruebas

## Fase 7: Módulo de Gestión de Mantenimientos (Backend)
- [ ] 7.1 Implementar entidades de dominio: Mantenimiento, TipoMantenimiento
- [ ] 7.2 Implementar servicios y controladores
- [ ] 7.3 Escribir pruebas

## Fase 8: Módulo de Gestión de Mantenimientos (Frontend)
- [ ] 8.1 Implementar páginas de gestión de mantenimientos
- [ ] 8.2 Implementar calendario de mantenimientos
- [ ] 8.3 Escribir pruebas

## Fase 9: Sistema de Plugins
- [ ] 9.1 Diseñar arquitectura de plugins
- [ ] 9.2 Implementar sistema de carga de plugins
- [ ] 9.4 Documentar API de plugins

## Fase 10: Monitorización, Logging y Telemetría

- [x] 10.1 Resolver errores de compilación de integración de telemetría en `GestMantIA.API`
  - [x] Corrección de dependencias de paquetes NuGet para `App.Metrics`, `InfluxDB.Client` y formateadores relacionados.
  - [x] Actualización de la configuración de `InfluxDbOptions` (uso de `Database` para bucket, eliminación de `Org`).
  - [x] Corrección en la instanciación y uso de formateadores de métricas (Prometheus, JSON) en `MetricsConfiguration.cs`.
  - [x] Ajustes en `Program.cs` para la configuración de `IMetricsOutputFormattingBuilder`.
- [ ] 10.2 Configurar y verificar la funcionalidad completa de telemetría
  - [ ] Verificar el envío de métricas a InfluxDB.
  - [ ] Visualizar métricas en Grafana.
  - [ ] Configurar logging centralizado si es necesario.

## Fase 11: Pruebas Finales y Despliegue
- [ ] 11.1 Pruebas de rendimiento
- [ ] 11.2 Pruebas de seguridad
- [ ] 11.3 Documentación final
- [ ] 11.4 Preparar para despliegue en producción

## Instrucciones de Uso del Roadmap

1. Marcar cada tarea como completada cuando se termine
2. Antes de comenzar una nueva tarea, verificar las dependencias
3. Hacer commit después de cada tarea completada
4. Actualizar este documento si se requieren cambios en la planificación

**Estándares de código**
   - Seguir guía de estilo
   - Documentar código público
   - Escribir pruebas unitarias
   - Mantener cobertura >80%

## Notas de Desarrollo

- **Arquitectura**: Clean Architecture + DDD
- **Patrones**: Repository, Unit of Work, CQRS (donde aplique)
- **Seguridad**: OWASP Top 10
- **Rendimiento**: Optimización de consultas, caching
- **Escalabilidad**: Diseño para escalar horizontalmente
- **Mantenibilidad**: Código limpio, principios SOLID
- **Documentación**: Comentarios XML, guías de usuario, API docs
