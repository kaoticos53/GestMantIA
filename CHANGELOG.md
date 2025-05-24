# Registro de Cambios (Changelog)

Este archivo documenta los cambios notables en el proyecto GestMantIA. El formato está basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

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
