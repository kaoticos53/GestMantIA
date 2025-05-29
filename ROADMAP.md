# Roadmap de Desarrollo - GestMantIA

Este documento describe los pasos necesarios para el desarrollo de GestMantIA, siguiendo una metodología de desarrollo guiado por pruebas (TDD) y arquitectura limpia.

## Fase 1: Configuración Inicial y Estructura del Proyecto

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
   - [x] Crear servicio de usuarios
   - [x] Implementar autorización basada en roles

- [x] 2.5 Implementar controladores de API para gestión de usuarios y roles
   - [x] Controlador de autenticación (login, registro, refresh token)
   - [x] Controlador de usuarios (CRUD)
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

## Fase 3: Módulo de Gestión de Usuarios (Backend) - **Completado**

- [x] 3.1 Implementar entidades de dominio: PerfilUsuario, Dirección
   - [x] Crear entidad PerfilUsuario con propiedades básicas
   - [x] Crear entidad Dirección con propiedades básicas
   - [x] Configurar relaciones con Usuario
   - [x] Implementar validaciones con FluentValidation
   - [x] Configurar migraciones de base de datos

- [x] 3.2 Implementar repositorios
   - [x] Crear IUserRepository
   - [x] Implementar UserRepository
   - [x] Crear IUserProfileRepository
   - [x] Implementar UserProfileRepository
   - [x] Configurar inyección de dependencias

- [x] 3.3 Implementar servicios de dominio
   - [x] Crear IUserService
   - [x] Implementar UserService
   - [x] Crear IUserProfileService
   - [x] Implementar UserProfileService
   - [x] Implementar validaciones de negocio
   - [x] Implementar notificaciones
   - [x] Configurar AutoMapper
   - [x] Configurar inyección de dependencias

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
- [x] Implementar layout principal con MudBlazor
- [x] Crear componentes de navegación
- [x] Configurar tema oscuro/claro
- [x] Implementar sistema de notificaciones
- [x] Resolver errores de compilación en el backend (surgido durante la migración)
- [x] Crear proyecto `GestMantIA.Shared` para DTOs y modelos comunes
- [x] Refactorizar DTOs moviéndolos a `GestMantIA.Shared` y actualizando referencias
- [x] Consolidar múltiples DbContexts en un único ApplicationDbContext
- [x] Resolver problemas de inyección de dependencias circulares
- [x] Actualizar pruebas unitarias para reflejar los cambios en el modelo de datos

### 4.3 Módulo de Autenticación con MudBlazor
- [ ] Rediseñar página de inicio de sesión
- [ ] Rediseñar página de registro
- [ ] Implementar recuperación de contraseña
- [ ] Mejorar manejo de errores y validaciones
- [ ] Agregar autenticación de dos factores

### 4.4 Dashboard Principal
- [ ] Diseñar layout del dashboard
- [ ] Crear componentes de resumen
- [ ] Implementar gráficos interactivos
- [ ] Agregar widgets personalizables
- [ ] Crear sistema de notificaciones

### 4.5 Módulo de Usuarios y Roles
- [ ] Lista de usuarios con filtros y búsqueda
- [ ] Formulario de creación/edición de usuarios
- [ ] Gestión de roles y permisos
- [ ] Perfil de usuario
- [ ] Preferencias de cuenta

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

### 4.4 Módulo de Gestión de Usuarios
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
- [ ] 9.3 Documentar API para desarrollo de plugins

## Fase 10: Pruebas Finales y Despliegue
- [ ] 10.1 Pruebas de rendimiento
- [ ] 10.2 Pruebas de seguridad
- [ ] 10.3 Documentación final
- [ ] 10.4 Preparar para despliegue en producción

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
