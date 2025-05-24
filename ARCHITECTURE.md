# Arquitectura y Estándares de Código

## 1. Estructura del Proyecto

### 1.1 Estructura de Carpetas

```
src/
├── GestMantIA.API/               # API principal (Web API)
├── GestMantIA.Core/              # Lógica de negocio
│   ├── Common/                   # Utilidades y helpers
│   ├── Domain/                   # Entidades de dominio
│   ├── Application/              # Casos de uso
│   │   ├── Features/             # Agrupación por características
│   │   │   ├── Users/            # Ejemplo: Característica de usuarios
│   │   │   └── ...
│   │   └── Shared/              # Lógica compartida entre características
│   └── Shared/                   # Objetos de valor y tipos compartidos
├── GestMantIA.Infrastructure/    # Implementaciones de infraestructura
│   ├── Persistence/             # Persistencia (EF Core)
│   ├── Identity/                # Identidad y autenticación
│   ├── Services/                 # Servicios de infraestructura
│   └── External/                # Integraciones externas
└── GestMantIA.Web/              # Frontend (MudBlazor)
```

### 1.2 Convenciones de Nombres

- **Clases**: PascalCase (ej: `UserService`)
- **Interfaces**: Prefijo "I" + PascalCase (ej: `IUserService`)
- **Métodos**: PascalCase (ej: `GetUserByIdAsync`)
- **Variables y parámetros**: camelCase
- **Constantes**: Todo en mayúsculas con guiones bajos (ej: `MAX_RETRY_COUNT`)
- **Archivos**: Mismo nombre que la clase principal que contienen

## 2. Patrones de Diseño

### 2.1 Patrón Repository

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
```

### 2.2 Patrón Unit of Work

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> CompleteAsync();
}
```

### 2.3 Patrón CQRS (Command Query Responsibility Segregation)

```
Application/
└── Features/
    └── Users/
        ├── Queries/          # Consultas
        │   ├── GetUserById/
        │   │   ├── GetUserByIdQuery.cs
        │   │   └── GetUserByIdHandler.cs
        │   └── GetUsers/
        │       ├── GetUsersQuery.cs
        │       └── GetUsersHandler.cs
        └── Commands/         # Comandos
            ├── CreateUser/
            │   ├── CreateUserCommand.cs
            │   └── CreateUserHandler.cs
            └── UpdateUser/
                ├── UpdateUserCommand.cs
                └── UpdateUserHandler.cs
```

## 3. Estructura de Clases

### 3.1 Entidades de Dominio

```csharp
public class User : BaseEntity
{
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    
    // Métodos de dominio
    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
    }
}
```

### 3.2 DTOs (Data Transfer Objects)

```csharp
public record UserDto
{
    public string Id { get; init; }
    public string UserName { get; init; }
    public string Email { get; init; }
    public string FullName { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = new List<string>();
}
```

### 3.3 Perfiles de AutoMapper

```csharp
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
    }
}
```

## 4. Manejo de Errores

### 4.1 Excepciones Personalizadas

```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entidad \"{name}" + $" ({key})" + " no fue encontrada.")
    {
    }
}
```

### 4.2 Middleware de Manejo de Errores

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Lógica de manejo de errores
    }
}
```

## 5. Documentación

### 5.1 Comentarios XML

```csharp
/// <summary>
/// Obtiene un usuario por su identificador único.
/// </summary>
/// <param name="id">Identificador único del usuario.</param>
/// <returns>El usuario si se encuentra; de lo contrario, null.</returns>
/// <exception cref="NotFoundException">Se lanza cuando el usuario no se encuentra.</exception>
public async Task<UserDto> GetUserByIdAsync(int id)
{
    // Implementación
}
```

### 5.2 Documentación de API

```csharp
/// <summary>
/// Obtiene un usuario por su ID
/// </summary>
/// <param name="id">ID del usuario</param>
/// <returns>Usuario encontrado</returns>
/// <response code="200">Retorna el usuario solicitado</response>
/// <response code="404">Usuario no encontrado</response>
[HttpGet("{id}")]
[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<UserDto>> GetUser(int id)
{
    // Implementación
}
```

## 6. Pruebas

### 6.1 Estructura de Pruebas

```
tests/
├── GestMantIA.UnitTests/         # Pruebas unitarias
│   ├── Application/
│   │   └── Features/
│   │       └── Users/
│   │           └── GetUserByIdTests.cs
│   └── Domain/
│       └── Entities/
│           └── UserTests.cs
└── GestMantIA.IntegrationTests/  # Pruebas de integración
    ├── API/
    │   └── UsersControllerTests.cs
    └── Infrastructure/
        └── UserRepositoryTests.cs
```

### 6.2 Ejemplo de Prueba Unitaria

```csharp
public class UserTests
{
    [Fact]
    public void UpdateName_WithValidNames_ShouldUpdateProperties()
    {
        // Arrange
        var user = new User("test@example.com", "Test", "User");
        
        // Act
        user.UpdateName("New", "Name");
        
        // Assert
        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("Name");
    }
}
```

## 7. Convenciones de Git

### 7.1 Mensajes de Commit

```
tipo(ámbito): descripción breve

Descripción detallada si es necesario

[OPCIONAL: Referencia a incidencia o tarea]
```

Ejemplo:
```
feat(users): agregar autenticación de dos factores

Se implementó la autenticación de dos factores usando Google Authenticator.
Se agregaron los endpoints necesarios para la generación y validación de códigos.

Refs: #123
```

### 7.2 Tipos de Cambio

- `feat`: Nueva característica
- `fix`: Corrección de error
- `docs`: Cambios en la documentación
- `style`: Cambios de formato (espacios, comas, etc.)
- `refactor`: Cambios que no corrigen errores ni agregan funcionalidades
- `test`: Adición o modificación de pruebas
- `chore`: Cambios en el proceso de construcción o herramientas auxiliares

## 8. Configuración de Herramientas

### 8.1 EditorConfig

```ini
# Archivo .editorconfig
root = true

[*]
charset = utf-8
indent_style = space
indent_size = 4
insert_final_newline = true
trim_trailing_whitespace = true

[*.{cs,csx,vb,vbx}]
dotnet_sort_system_directives_first = true

# Estilo de código de C#
# ...
```

### 8.2 Directory.Build.props

```xml
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn> <!-- Suprime advertencias de documentación faltante -->
  </PropertyGroup>
</Project>
```

## 9. Guía de Implementación

### 9.1 Pasos para Agregar una Nueva Característica

1. Crear rama feature/ desde develop
2. Implementar la característica siguiendo la estructura definida
3. Agregar pruebas unitarias y de integración
4. Actualizar documentación si es necesario
5. Crear solicitud de extracción (Pull Request)
6. Pasar revisión de código
7. Fusionar a develop después de la aprobación

### 9.2 Revisión de Código

- [ ] El código sigue las convenciones de estilo
- [ ] Las pruebas unitarias pasan
- [ ] La documentación está actualizada
- [ ] No hay código comentado innecesario
- [ ] No hay advertencias del compilador
- [ ] El rendimiento es aceptable
- [ ] Se manejan correctamente los errores

## 10. Recursos Adicionales

- [Guía de Estilo de C# de Microsoft](https://docs.microsoft.com/es-es/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Patrones de Diseño](https://refactoring.guru/es/design-patterns/csharp)
- [Documentación de .NET](https://docs.microsoft.com/es-es/dotnet/)
- [Documentación de Entity Framework Core](https://docs.microsoft.com/es-es/ef/core/)

---

*Última actualización: 23 de mayo de 2025*
