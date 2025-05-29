# Arquitectura y Estándares de Código

## 1. Estructura del Proyecto

### 1.1 Estructura de Carpetas

```
src/
├── GestMantIA.API/               # API principal (Web API)
├── GestMantIA.Core/            # Lógica de negocio central
│   ├── Configuration/           # Configuraciones de la aplicación
│   ├── Entities/                # Entidades del dominio
│   ├── Identity/                # Identidad y autenticación
│   │   ├── DTOs/                # Objetos de transferencia de datos
│   │   ├── Entities/            # Entidades de identidad
│   │   ├── Interfaces/          # Contratos de servicios de identidad
│   │   └── Results/             # Resultados de operaciones
│   ├── Interfaces/              # Contratos de repositorios y servicios
│   ├── Models/                  # DTOs y ViewModels
│   └── Shared/                  # Utilidades y extensiones compartidas de transferencia de datos
│   └── Shared/                   # Tipos y utilidades compartidas
├── GestMantIA.Infrastructure/    # Implementaciones de infraestructura
│   ├── Data/                     # Contextos de base de datos
│   ├── Services/                 # Implementaciones de servicios
│   │   ├── Auth/                 # Servicios de autenticación
│   │   ├── Email/                # Servicios de correo electrónico
│   │   └── Security/             # Servicios de seguridad
│   └── Migrations/               # Migraciones de base de datos
└── GestMantIA.Web/               # Frontend (MudBlazor)
    ├── Components/               # Componentes reutilizables
    ├── Layout/                   # Layouts de la aplicación
    ├── Models/                   # Modelos del frontend
    ├── Pages/                    # Páginas de la aplicación
    │   ├── Admin/                # Páginas de administración
    │   └── Auth/                 # Páginas de autenticación
    ├── Services/                 # Servicios del frontend
    ├── Shared/                   # Recursos compartidos
    └── wwwroot/                  # Archivos estáticos
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

## 6. Autenticación y Gestión de Usuarios

### 6.1 Estructura de Identidad

La gestión de identidad sigue la estructura de ASP.NET Core Identity con personalizaciones:

```
Identity/
├── DTOs/                    # Objetos de transferencia de datos
│   ├── Requests/            # Modelos de solicitud
│   │   ├── LoginRequest.cs
│   │   ├── RegisterRequest.cs
│   │   └── ...
│   └── Responses/           # Modelos de respuesta
│       ├── AuthResponse.cs
│       └── UserResponse.cs
├── Entities/                # Entidades de identidad
│   ├── ApplicationUser.cs
│   ├── ApplicationRole.cs
│   └── ...
├── Interfaces/              # Contratos de servicios
│   ├── IAuthService.cs
│   ├── IUserService.cs
│   └── IRoleService.cs
└── Results/                 # Resultados de operaciones
    ├── AuthenticationResult.cs
    └── OperationResult.cs
```

### 6.2 Autenticación con JWT

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Configuration["Jwt:Issuer"],
            ValidAudience = Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
        };
    });
```

### 6.3 Gestión de Usuarios y Roles

La gestión de usuarios y roles se realiza a través de los servicios `IUserService` e `IRoleService` respectivamente, que encapsulan la lógica de negocio relacionada con la identidad.

```csharp
public interface IUserService
{
    Task<UserResponseDTO?> GetUserProfileAsync(string userId);
    Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDTO profile);
    Task<PagedResult<UserResponseDTO>> SearchUsersAsync(string? searchTerm, int pageNumber, int pageSize);
    Task<UserResponseDTO> CreateUserAsync(CreateUserDTO createUserDto, IEnumerable<string>? roleNames = null);
    Task<UserResponseDTO> UpdateUserAsync(UpdateUserDTO updateUserDto);
    Task<bool> DeleteUserAsync(string userId);
    Task<UserResponseDTO?> GetUserByIdAsync(string userId);
    Task<PagedResult<UserResponseDTO>> GetAllUsersAsync(int pageNumber, int pageSize, string? searchTerm, bool? activeOnly);
}
```

### 6.4 Políticas de Autorización

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", 
        policy => policy.RequireRole("Admin"));
    
    options.AddPolicy("RequireUserRole", 
        policy => policy.RequireRole("User"));
});
```

## 7. Pruebas

### 7.1 Estructura de Pruebas

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

### 7.2 Ejemplo de Prueba Unitaria

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

## 8. Convenciones de Git

### 8.1 Mensajes de Commit

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

### 8.2 Tipos de Cambio

- `feat`: Nueva característica
- `fix`: Corrección de error
- `docs`: Cambios en la documentación
- `style`: Cambios de formato (espacios, comas, etc.)
- `refactor`: Cambios que no corrigen errores ni agregan funcionalidades
- `test`: Adición o modificación de pruebas
- `chore`: Cambios en el proceso de construcción o herramientas auxiliares

## 9. Configuración de Herramientas

### 9.1 EditorConfig

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

### 9.2 Directory.Build.props

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

## 10. Guía de Implementación

### 10.1 Pasos para Agregar una Nueva Característica

1. Crear rama feature/ desde develop
2. Implementar la característica siguiendo la estructura definida
3. Agregar pruebas unitarias y de integración
4. Actualizar documentación si es necesario
5. Crear solicitud de extracción (Pull Request)
6. Pasar revisión de código
7. Fusionar a develop después de la aprobación

### 10.2 Revisión de Código

- [ ] El código sigue las convenciones de estilo
- [ ] Las pruebas unitarias pasan
- [ ] La documentación está actualizada
- [ ] No hay código comentado innecesario
- [ ] No hay advertencias del compilador
- [ ] El rendimiento es aceptable
- [ ] Se manejan correctamente los errores

## 11. Recursos Adicionales

- [Guía de Estilo de C# de Microsoft](https://docs.microsoft.com/es-es/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Patrones de Diseño](https://refactoring.guru/es/design-patterns/csharp)
- [Documentación de .NET](https://docs.microsoft.com/es-es/dotnet/)
- [Documentación de Entity Framework Core](https://docs.microsoft.com/es-es/ef/core/)

---

*Última actualización: 23 de mayo de 2025*
