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

## Fase 2: Módulo de Autenticación y Autorización (Backend)

- [x] 2.1 Configurar proyecto API base con autenticación JWT
   - [x] Instalar paquetes NuGet necesarios
   - [x] Configurar JWT en Program.cs
   - [x] Implementar configuración de autenticación
   - [x] Configurar política de CORS
   - [x] Documentar configuración

- [ ] 2.2 Implementar entidades de dominio: Usuario, Rol, Permiso
   - [x] Crear entidad Usuario con propiedades básicas
   - [x] Crear entidad Rol con propiedades básicas
   - [x] Crear entidad Permiso con propiedades básicas
   - [ ] Configurar relaciones entre entidades
   - [ ] Implementar validaciones con FluentValidation
   - [ ] Configurar migraciones de base de datos

- [ ] 2.3 Implementar patrones Repository y Unit of Work
   - [ ] Crear interfaces genéricas
   - [ ] Implementar repositorios base
   - [ ] Implementar Unit of Work
   - [ ] Configurar inyección de dependencias

- [ ] 2.4 Implementar servicios de autenticación y autorización
   - [ ] Crear servicio de autenticación
   - [ ] Implementar generación de tokens JWT
   - [ ] Crear servicio de usuarios
   - [ ] Implementar autorización basada en roles

- [ ] 2.5 Implementar controladores de API para gestión de usuarios y roles
   - [ ] Controlador de autenticación (login, registro, refresh token)
   - [ ] Controlador de usuarios (CRUD)
   - [ ] Controlador de roles (CRUD)
   - [ ] Documentar endpoints con XML comments

- [ ] 2.6 Configurar Swagger/OpenAPI
   - [ ] Configurar documentación de API
   - [ ] Agregar autenticación en Swagger UI
   - [ ] Documentar códigos de respuesta
   - [ ] Configurar ejemplos de solicitudes/respuestas

- [ ] 2.7 Escribir pruebas unitarias y de integración
   - [ ] Pruebas de servicios
   - [ ] Pruebas de controladores
   - [ ] Pruebas de integración
   - [ ] Configurar cobertura de código

## Fase 3: Módulo de Autenticación y Autorización (Frontend)

- [ ] 3.1 Configurar aplicación Blazor WebAssembly
- [ ] 3.2 Implementar autenticación con JWT
- [ ] 3.3 Crear páginas de login/registro
- [ ] 3.4 Implementar gestión de usuarios y roles
- [ ] 3.5 Implementar diseño responsive con MudBlazor
- [ ] 3.6 Escribir pruebas unitarias y de integración

## Fase 4: Módulo de Gestión de Equipos (Backend)
- [ ] 4.1 Implementar entidades de dominio: Equipo, TipoEquipo, EstadoEquipo
- [ ] 4.2 Implementar servicios y controladores
- [ ] 4.3 Escribir pruebas

## Fase 5: Módulo de Gestión de Equipos (Frontend)
- [ ] 5.1 Implementar páginas de gestión de equipos
- [ ] 5.2 Implementar formularios CRUD
- [ ] 5.3 Escribir pruebas

## Fase 6: Módulo de Gestión de Mantenimientos (Backend)
- [ ] 6.1 Implementar entidades de dominio: Mantenimiento, TipoMantenimiento
- [ ] 6.2 Implementar servicios y controladores
- [ ] 6.3 Escribir pruebas

## Fase 7: Módulo de Gestión de Mantenimientos (Frontend)
- [ ] 7.1 Implementar páginas de gestión de mantenimientos
- [ ] 7.2 Implementar calendario de mantenimientos
- [ ] 7.3 Escribir pruebas

## Fase 8: Sistema de Plugins
- [ ] 8.1 Diseñar arquitectura de plugins
- [ ] 8.2 Implementar sistema de carga de plugins
- [ ] 8.3 Documentar API para desarrollo de plugins

## Fase 9: Pruebas Finales y Despliegue
- [ ] 9.1 Pruebas de rendimiento
- [ ] 9.2 Pruebas de seguridad
- [ ] 9.3 Documentación final
- [ ] 9.4 Preparar para despliegue en producción

## Instrucciones de Uso del Roadmap

1. Marcar cada tarea como completada cuando se termine
2. Antes de comenzar una nueva tarea, verificar las dependencias
3. Hacer commit después de cada tarea completada
4. Actualizar este documento si se requieren cambios en la planificación

## Notas de Desarrollo

- Seguir principios SOLID y Clean Architecture
- Usar TDD para todas las funcionalidades
- Documentar el código siguiendo estándares XML
- Escribir pruebas unitarias con cobertura mínima del 80%
