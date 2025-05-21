# Registro de Cambios (Changelog)

Este archivo documenta los cambios notables en el proyecto GestMantIA. El formato está basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

## [0.3.0] - 2025-05-21

### Agregado
- Entidades de dominio: Usuario, Rol, Permiso
- Entidades de unión: UsuarioRol, RolPermiso
- Propiedades de navegación y relaciones entre entidades
- Documentación XML para todas las entidades
- Estructura base para el manejo de usuarios y roles

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
