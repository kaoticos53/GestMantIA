# Plan de Migración a Vertical Slice por Feature para GestMantIA

## Objetivo
Adoptar una estructura vertical por feature en todas las capas del proyecto para mejorar la mantenibilidad, escalabilidad y alineación con DDD y vertical slice.

---

## Fases del Plan

### Fase 1: Auditoría y Diagnóstico
1. Listar todos los features actuales (ej: UserManagement, Auth, Security, Maintenance, etc.).
2. Identificar archivos y servicios genéricos que deben ser reubicados.
3. Documentar dependencias cruzadas y posibles duplicidades.

### Auditoría y Diagnóstico

### 1. Servicios genéricos en Infrastructure
- **UserService.cs**  
  Ubicación: `Infrastructure/Services/UserService.cs`  
  Implementa lógica de usuario, solapa con `ApplicationUserService`.  
  Depende de entidades de Core y repositorios de Infrastructure.  
  **Duplicidad detectada:** Métodos similares/idénticos a los de `Application/Features/UserManagement/Services/ApplicationUserService.cs`.  
  **Estado:** Pendiente de migración y consolidación.
- **RoleService.cs**  
  Ubicación: `Infrastructure/Services/RoleService.cs`  
  Lógica de roles, debe migrar a feature correspondiente.  
  **Estado:** Pendiente de migración.

### 2. Repositorios genéricos en Infrastructure
- **Repository.cs**  
  Ubicación: `Infrastructure/Data/Repositories/Repository.cs`  
  Implementación genérica, requiere evaluar si se adapta a vertical slice o debe fragmentarse.  
  **Estado:** Pendiente de análisis/migración.
- **UserProfileRepository.cs** y **UserRepository.cs**  
  Ubicación: `Infrastructure/Data/Repositories/`  
  Relacionados con la feature UserManagement, deben migrar a su carpeta vertical slice.  
  **Estado:** Pendiente de migración.

### 3. Features en Application
- **UserManagement**  
  Única feature explícita en `Application/Features/`.  
  Contiene servicios y lógica de usuario, pero parte de la lógica sigue en Infrastructure.  
  **Estado:** Pendiente de consolidación total.

### 4. Entidades en Core
- **BaseEntity.cs, UserProfile.cs**  
  Entidades de dominio, correctamente ubicadas en Core.  
  **Estado:** Correcto, sólo requiere referencia clara desde features.

### 5. Controladores en API
- **Controladores relevantes:**  
  `UserManagementController.cs`, `UsersController.cs`, `RolesController.cs`, `AuthController.cs`, etc.  
  Usan servicios e interfaces que están duplicadas o dispersas entre Infrastructure y Application.  
  **Estado:** Pendiente de actualización de dependencias tras migración.

### 6. Dependencias cruzadas y duplicidades
- **IUserService**  
  Declaración y uso en ambos Application e Infrastructure.  
  Debe centralizarse en Application y eliminar duplicidad.  
  **Estado:** Pendiente de migración/consolidación.
- **UserService y ApplicationUserService**  
  Métodos y lógica solapada.  
  Debe consolidarse en Application, eliminando implementación en Infrastructure.  
  **Estado:** Pendiente de consolidación.
- **Repositorios y servicios genéricos**  
  Usados por varias features, requieren fragmentación y migración a carpetas vertical slice.  
  **Estado:** Pendiente de migración.

### 7. DTOs, Validators y otros artefactos
- No se han listado explícitamente, pero deben identificarse y migrarse junto a cada feature.  
  **Estado:** Pendiente de identificación y migración.

### 8. Riesgos y puntos de bloqueo
- **Dependencias cruzadas:** Algunos servicios pueden depender de infraestructura (por ejemplo, acceso a datos o servicios externos), lo que requiere refactor para invertir dependencias.
- **Duplicidad de lógica:** Riesgo de pérdida de funcionalidad si no se consolidan correctamente los métodos duplicados.

### Fase 2: Diseño de la Nueva Estructura
1. Definir para cada feature una carpeta en Application, Core, Infrastructure y API.
2. Especificar subcarpetas internas estándar:
   - Commands / Queries / Handlers (CQRS)
   - DTOs
   - Validators
   - Repositories
   - Services
   - Entities
   - Controllers
3. Actualizar el diagrama y explicación en ARCHITECTURE.md.

### Fase 3: Migración Feature a Feature
**Ejemplo: UserManagement**

#### Application
- Mover servicios, DTOs, validadores y handlers a `Features/UserManagement/`

#### Core
- Mover entidades y contratos a `Entities/UserManagement/` y `Interfaces/UserManagement/`

#### Infrastructure
- Mover repositorios y servicios a `Data/Repositories/UserManagement/` y `Services/UserManagement/`

#### API
- Mover controladores y validadores a `Controllers/UserManagementController.cs` y `Validators/UserManagement/`

#### Web (si aplica)
- Mover componentes, páginas y servicios a `Pages/UserManagement/`, `Services/UserManagement/`

#### Shared
- Ubicar DTOs o utilidades realmente transversales en `Shared`.

### Fase 4: Refactorización y Actualización de Namespaces
1. Actualizar todos los namespaces para reflejar la nueva estructura.
2. Corregir los usings y dependencias en cada archivo.
3. Actualizar los tests unitarios y de integración para reflejar la nueva ubicación de las clases.

### Fase 5: Validación y Documentación
1. Ejecutar todos los tests y corregir errores derivados de la migración.
2. Validar la compilación y el correcto funcionamiento de la solución.
3. Actualizar la documentación técnica y los diagramas.
4. Registrar los cambios en CHANGELOG.md y ROADMAP.md.

---

## Recomendaciones
- Migrar un feature a la vez, validando la compilación y los tests tras cada migración.
- Mantener ramas de trabajo separadas para cada feature o fase.
- Priorizar features críticos (UserManagement, Auth, Security).
- Revisar dependencias cruzadas y eliminar código duplicado durante la migración.
- Involucrar a todo el equipo en la revisión de la nueva estructura.

---

## Ejemplo de estructura final para UserManagement

```
src/
  GestMantIA.Application/
    Features/
      UserManagement/
        Commands/
        Queries/
        Handlers/
        DTOs/
        Validators/
  GestMantIA.Core/
    Entities/
      UserManagement/
    Interfaces/
      UserManagement/
  GestMantIA.Infrastructure/
    Data/
      Repositories/
        UserManagement/
    Services/
      UserManagement/
  GestMantIA.API/
    Controllers/
      UserManagementController.cs
    Validators/
      UserManagement/
  GestMantIA.Web/
    Pages/
      UserManagement/
    Services/
      UserManagement/
```

---

**Este plan puede adaptarse iterativamente según los resultados de cada fase y el feedback del equipo.**
