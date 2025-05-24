# Plantillas para Generación de Código

Este directorio contiene plantillas para generar código consistente en todo el proyecto GestMantIA. Estas plantillas siguen las mejores prácticas y patrones de diseño establecidos en la arquitectura del proyecto.

## Plantillas Disponibles

### 1. DTO (Data Transfer Object)

**Archivo:** `dto.template.cs`

Genera una clase DTO para transferencia de datos entre capas.

**Variables de plantilla:**
- `{{FeatureName}}`: Nombre de la característica (ej: "UserManagement")
- `{{DtoName}}`: Nombre del DTO (ej: "User")
- `{{PropertyName}}`: Nombre de la propiedad de ejemplo
- `{{PropertyType}}`: Tipo de la propiedad de ejemplo
- `{{PropertyDescription}}`: Descripción de la propiedad
- `{{Description}}`: Descripción del DTO

---

### 2. Controlador de API

**Archivo:** `controller.template.cs`

Genera un controlador de API RESTful con operaciones CRUD estándar.

**Variables de plantilla:**
- `{{FeatureName}}`: Nombre de la característica (ej: "UserManagement")
- `{{FeatureNamePlural}}`: Nombre pluralizado de la característica (ej: "UserManagements")
- `{{ControllerName}}`: Nombre del controlador (ej: "User")
- `{{ControllerNamePlural}}`: Nombre pluralizado del controlador (ej: "Users")
- `{{DtoName}}`: Nombre del DTO principal (ej: "UserDto")

---

### 3. Prueba Unitaria

**Archivo:** `unittest.template.cs`

Genera una clase de prueba unitaria con ejemplos de pruebas para un manejador.

**Variables de plantilla:**
- `{{FeatureName}}`: Nombre de la característica (ej: "UserManagement")
- `{{TestName}}`: Nombre de la prueba (ej: "UserCreation")
- `{{EntityName}}`: Nombre de la entidad (ej: "User")
- `{{HandlerName}}`: Nombre del manejador (ej: "CreateUserHandler")
- `{{CommandName}}`: Nombre del comando (ej: "CreateUserCommand")
- `{{ExpectedBehavior}}`: Comportamiento esperado (ej: "RetornarExito")
- `{{Condition}}`: Condición de la prueba (ej: "SeProporcionanDatosValidos")
- `{{InvalidCondition}}`: Condición inválida (ej: "SeProporcionanDatosInvalidos")

## Uso con el Script de Generación

Se recomienda utilizar el script `New-Component.ps1` en el directorio `Scripts` para generar componentes completos basados en estas plantillas.

Ejemplo:
```powershell
# Desde la raíz del proyecto
.\Scripts\New-Component.ps1 -ComponentName "Product" -FeatureName "Inventory"
```

## Personalización

Puedes modificar las plantillas según las necesidades específicas de tu proyecto. Asegúrate de mantener la consistencia con la arquitectura general.

## Convenciones

- Los nombres de las características deben estar en singular y en PascalCase
- Los nombres de los controladores deben estar en plural cuando corresponda (ej: "UsersController")
- Todas las plantillas usan espacios (no tabs) para la indentación
- La documentación XML es obligatoria para todas las clases y métodos públicos

## Actualización de Plantillas

Si realizas cambios en las plantillas, asegúrate de actualizar la documentación correspondiente y notificar al equipo para mantener la consistencia.
