# Propuestas de Mejora y Optimización para GestMantIA

## 1. Estructura y Organización de Carpetas
- **Revisar y homogeneizar la estructura vertical por feature:**
  - Asegurar que cada feature tenga su carpeta en Application, Core, Infrastructure y Web.
  - Evitar carpetas genéricas como "Services" o "Helpers" fuera de contexto de feature.
- **Separar claramente dominio, aplicación, infraestructura y API** siguiendo DDD y vertical slice.

## 2. Consistencia en Nombres y Convenciones
- Revisar nombres de clases, interfaces y métodos:
  - Clases y métodos en PascalCase.
  - Interfaces con prefijo "I".
  - Variables y parámetros en camelCase.
  - Constantes en MAYUSCULAS_CON_GUION_BAJO.
- Unificar sufijos (ej: ...Service, ...Repository, ...Controller).

## 3. Entidades y Uso de Guid
- Verificar que todas las entidades heredan de BaseEntity<Guid> y todas las relaciones usan Guid.
- Revisar DTOs, mapeos y claves foráneas para asegurar consistencia.

## 4. Limpieza de Código y Eliminación de Código Muerto
- Eliminar archivos de test vacíos o sin utilidad.
- Eliminar clases, métodos y propiedades no usados (especialmente tras refactorizaciones recientes).
- Limpiar usings y dependencias innecesarias.

## 5. Refactorización de Servicios y Repositorios
- Asegurar que la lógica de orquestación está en Application, no en Infrastructure.
- Revisar que todos los repositorios implementan correctamente el patrón Repository y UnitOfWork.
- Unificar la gestión de excepciones y logs.

## 6. Tests Unitarios y Cobertura
- Volver a crear tests unitarios relevantes para Infrastructure usando InMemory y mocks.
- Completar tests para ApplicationUserService y UserProfileService.
- Mantener la cobertura >80% y seguir la convención de nombres.

## 7. Documentación y Comentarios
- Añadir comentarios XML en clases y métodos públicos.
- Documentar excepciones personalizadas y puntos críticos de la arquitectura.
- Mantener actualizado CHANGELOG.md y ROADMAP.md tras cada cambio relevante.

## 8. Validaciones y Seguridad
- Revisar validaciones en DTOs y Models.
- Unificar el uso de FluentValidation.
- Asegurar el uso correcto de Claims y políticas de autorización.

## 9. Optimización de Dependencias y Configuración
- Verificar que todos los paquetes NuGet están gestionados centralizadamente.
- Eliminar referencias y atributos Version innecesarios en los .csproj.
- Revisar configuración de logging y connection strings.

## 10. Plan de Acciones Detallado
1. **Auditoría de entidades y claves Guid**: Revisar todos los modelos y relaciones.
2. **Revisión y limpieza de carpetas y archivos**: Eliminar o mover archivos según el estándar vertical slice.
3. **Homogeneizar nombres y convenciones**: Refactorizar nombres incoherentes.
4. **Eliminar código muerto y dependencias no usadas**.
5. **Reestructurar y documentar ApplicationUserService y UserProfileService**.
6. **Recrear tests unitarios en Infrastructure y Application**.
7. **Actualizar documentación (CHANGELOG, ROADMAP, comentarios XML)**.
8. **Validar configuración centralizada de paquetes y settings**.
9. **Revisión de seguridad, validaciones y logs**.
10. **Automatizar revisión de estilos y análisis estático (ej: StyleCop, SonarQube).**

---

**Nota:** Este plan se debe revisar y actualizar iterativamente tras cada fase de refactorización o integración importante.
