# Guía de Estilo y Convenciones de Codificación C# para GestMantIA

Este documento establece las directrices de estilo y convenciones de codificación que se seguirán en el proyecto GestMantIA. El objetivo es asegurar un código consistente, legible y mantenible. Estas directrices se basan en las recomendaciones oficiales de Microsoft para C#.

## 1. Convenciones de Nomenclatura de Identificadores

Referencia: [Nombres de identificador - C# | Microsoft Docs](https://learn.microsoft.com/es-es/dotnet/csharp/fundamentals/coding-style/identifier-names)

### 1.1. Reglas Generales
- Usar nombres descriptivos y significativos.
- Evitar abreviaturas o acrónimos que no sean ampliamente conocidos. Si se usan, deben ser consistentes.
- No usar identificadores que entren en conflicto con palabras clave de C#.

### 1.2. PascalCase
Se usará `PascalCase` para:
- Tipos (clases, structs, interfaces, enums, delegates)
  ```csharp
  public class MiClase {}
  public interface IMiInterfaz {}
  public enum MiEnumeracion {}
  ```
- Miembros públicos (métodos, propiedades, eventos, constantes públicas, campos públicos)
  ```csharp
  public class Ejemplo {
      public const int ConstantePublica = 10;
      public static readonly string CampoEstaticoLecturaPublico = "Valor";
      public string PropiedadPublica { get; set; }
      public event EventHandler MiEventoPublico;
      public void MetodoPublico() {}
  }
  ```

### 1.3. camelCase
Se usará `camelCase` para:
- Parámetros de métodos
  ```csharp
  public void MiMetodo(string primerParametro, int segundoParametro) {}
  ```
- Variables locales
  ```csharp
  public void OtroMetodo() {
      string miVariableLocal = "test";
      // ...
  }
  ```

### 1.4. Prefijos para Identificadores
- **Interfaces**: Prefijo `I` seguido de `PascalCase`.
  ```csharp
  public interface IUserService {}
  ```
- **Campos privados (no estáticos)**: Prefijo `_` seguido de `camelCase`.
  ```csharp
  public class Ejemplo {
      private string _campoPrivado;
  }
  ```
- **Campos privados estáticos**: Prefijo `s_` seguido de `camelCase`.
  ```csharp
  public class Ejemplo {
      private static string s_campoPrivadoEstatico;
  }
  ```
- **Campos privados estáticos de solo lectura**: Prefijo `t_` seguido de `camelCase` para campos estáticos de subproceso. (Nota: Microsoft recomienda `s_` para estáticos y `t_` para estáticos de hilo. Nos ceñiremos a `s_` para todos los campos estáticos privados a menos que se especifique lo contrario para `ThreadStatic`).
  ```csharp
  // Ejemplo con ThreadStatic
  [ThreadStatic]
  private static int t_contadorPorHilo;
  ```

### 1.5. Atributos
- Los nombres de las clases de atributos deben terminar con el sufijo `Attribute`.
  ```csharp
  public class MiAtributoPersonalizadoAttribute : Attribute {}
  ```
- Al aplicar un atributo, se puede omitir el sufijo `Attribute`.
  ```csharp
  [MiAtributoPersonalizado]
  public class ClaseConAtributo {}
  ```

### 1.6. Nombres de Ensamblados y DLLs
- Usar `PascalCase`.
- Ejemplo: `GestMantIA.Core.dll`

### 1.7. Nombres de Espacios de Nombres (Namespaces)
- Usar `PascalCase`.
- Seguir el patrón: `NombreCompania.NombreTecnologia.Caracteristica` o `NombreProyecto.Capa.Caracteristica`.
- Ejemplo: `GestMantIA.Application.Features.UserManagement`

## 2. Convenciones de Codificación

Referencia: [Convenciones de codificación de C# (guía de C#) | Microsoft Docs](https://learn.microsoft.com/es-es/dotnet/csharp/fundamentals/coding-style/coding-conventions)

### 2.1. Diseño de Lenguaje
- **`var`**: Usar `var` para la declaración de variables locales cuando el tipo de la variable es obvio desde el lado derecho de la asignación, o cuando el tipo preciso no es importante.
  ```csharp
  // Recomendado
  var servicio = new UserService();
  var usuarios = servicio.ObtenerUsuarios();

  // No recomendado si el tipo no es obvio o si la claridad se ve comprometida
  // var resultado = MetodoComplejo(); // ¿Qué tipo es 'resultado'?
  ```
- **Tipos implícitos para arrays**: Usar la sintaxis de tipo implícito al inicializar arrays.
  ```csharp
  var numeros = new[] { 1, 2, 3 }; // Recomendado
  // int[] numeros = new int[] { 1, 2, 3 }; // Menos preferido
  ```
- **Interpolación de cadenas**: Preferir la interpolación de cadenas (`$"{variable}"`) sobre `string.Format()`.
  ```csharp
  string nombre = "Mundo";
  string saludo = $"Hola, {nombre}!"; // Recomendado
  // string saludo = string.Format("Hola, {0}!", nombre); // Menos preferido
  ```
- **Propiedades autoimplementadas**: Usar propiedades autoimplementadas cuando no se requiere lógica adicional en los descriptores de acceso.
  ```csharp
  public string Nombre { get; set; } // Recomendado
  ```
- **Miembros con cuerpo de expresión (Expression-bodied members)**: Usar para métodos, propiedades, constructores, finalizadores e indizadores simples.
  ```csharp
  // Propiedad
  public string NombreCompleto => $"{Nombre} {Apellido}";

  // Método
  public override string ToString() => $"{NombreCompleto} ({Edad})";
  ```
- **Inicializadores de objetos y colecciones**: Usar para crear e inicializar objetos y colecciones de forma concisa.
  ```csharp
  var usuario = new Usuario
  {
      Nombre = "Juan",
      Edad = 30
  };

  var lista = new List<int> { 1, 2, 3 };
  ```
- **Instrucción `using`**: Usar siempre la instrucción `using` para objetos que implementan `IDisposable` para asegurar su correcta liberación. Se prefiere la declaración `using` simplificada (sin llaves) si el ámbito es hasta el final del bloque actual.
  ```csharp
  // Recomendado (declaración using simplificada)
  using var stream = new MemoryStream();
  // ... usar stream ...

  // Alternativa (bloque using tradicional)
  using (var reader = new StreamReader(filePath))
  {
      // ... usar reader ...
  }
  ```
- **Async/Await**: Usar `async` y `await` para operaciones enlazadas a E/S y otras operaciones asíncronas para mejorar la capacidad de respuesta y evitar el bloqueo de hilos.
  - Los métodos asíncronos deben tener el sufijo `Async`.
  - Evitar `async void` excepto para controladores de eventos. Usar `async Task` o `async Task<T>` en su lugar.
  ```csharp
  public async Task<string> ObtenerDatosAsync()
  {
      // ...
      return await httpClient.GetStringAsync(url);
  }
  ```
- **Tuplas**: Usar tuplas para devolver múltiples valores de un método de forma sencilla. Preferir tuplas con nombres de elementos para mayor claridad.
  ```csharp
  public (string nombre, int edad) ObtenerInformacionUsuario()
  {
      return ("Ana", 25);
  }
  var info = ObtenerInformacionUsuario();
  Console.WriteLine($"{info.nombre} tiene {info.edad} años.");
  ```
- **Coincidencia de patrones (Pattern Matching)**: Utilizar para mejorar la legibilidad y concisión en comprobaciones de tipo y conversiones.
  ```csharp
  if (obj is string str && str.Length > 0) { /* ... */ }

  switch (forma)
  {
      case Cuadrado c:
          // ...
          break;
      case Circulo c when c.Radio > 10:
          // ...
          break;
  }
  ```
- **Directivas `using`**:
  - Colocar todas las directivas `using` al principio del archivo.
  - Ordenar alfabéticamente (opcional, pero puede ser gestionado por el IDE).
  - Eliminar directivas `using` no utilizadas.

### 2.2. Estilo de Formato
- **Sangría**: Usar 4 espacios para la sangría. No usar tabulaciones.
- **Llaves**: Usar el estilo Allman (llaves en su propia línea).
  ```csharp
  public class MiClase
  { // Llave de apertura en nueva línea
      public void MiMetodo()
      { // Llave de apertura en nueva línea
          if (condicion)
          {
              // ...
          }
      } // Llave de cierre en nueva línea
  } // Llave de cierre en nueva línea
  ```
- **Espaciado**:
  - Usar un espacio después de una coma entre argumentos de método o parámetros.
  - Usar un espacio alrededor de operadores binarios (`+`, `-`, `*`, `/`, `=`, `==`, etc.).
  - No usar espacios después de los paréntesis de apertura y antes de los de cierre en llamadas a métodos o declaraciones.
  - Usar una línea en blanco para separar bloques lógicos de código (por ejemplo, entre métodos).
- **Longitud de línea**: Intentar mantener las líneas de código por debajo de un límite razonable (ej. 120 caracteres) para mejorar la legibilidad. Dividir líneas largas si es necesario.

### 2.3. Comentarios
- **Comentarios XML para documentación**: Usar para todos los tipos y miembros públicos.
  ```csharp
  /// <summary>
  /// Representa un servicio para la gestión de usuarios.
  /// </summary>
  public interface IUserService
  {
      /// <summary>
      /// Obtiene un usuario por su identificador.
      /// </summary>
      /// <param name="userId">El identificador del usuario.</param>
      /// <returns>El usuario encontrado o null si no existe.</returns>
      Task<User> GetUserByIdAsync(Guid userId);
  }
  ```
- **Comentarios en línea**: Usar `//` para comentarios de una sola línea o al final de una línea. Usar `/* ... */` para comentarios de varias líneas solo si es estrictamente necesario y no se puede refactorizar el código para que sea autoexplicativo.
- Evitar comentarios que simplemente repiten lo que el código hace. Los comentarios deben explicar el "por qué" o clarificar lógica compleja.

### 2.4. Organización del Código
- **Archivos**: Un tipo (clase, interfaz, struct, enum) por archivo. El nombre del archivo debe coincidir con el nombre del tipo (ej. `UserService.cs`).
- **Miembros de clase**: Organizar los miembros de una clase en un orden consistente:
  1. Campos (constantes, estáticos, de instancia)
  2. Constructores
  3. Propiedades
  4. Métodos públicos
  5. Métodos protegidos
  6. Métodos privados
  7. Tipos anidados
- **Regiones**: Usar `#region` con moderación, solo para agrupar bloques de código muy grandes o lógicamente distintos dentro de una clase extensa. No abusar de ellas.

### 2.5. Manejo de Excepciones
- Preferir el manejo específico de excepciones sobre la captura genérica de `Exception`.
  ```csharp
  try
  {
      // ... código que puede lanzar excepciones ...
  }
  catch (FileNotFoundException ex)
  {
      // Manejar específicamente la ausencia del archivo
      _logger.LogError(ex, "Archivo no encontrado.");
  }
  catch (HttpRequestException ex)
  {
      // Manejar errores de red
      _logger.LogError(ex, "Error en la solicitud HTTP.");
  }
  // Evitar: catch (Exception ex) { /* ... */ } a menos que se relance o sea el último recurso.
  ```
- Usar `throw;` para relanzar una excepción capturada sin perder la pila de llamadas original. No usar `throw ex;`.
- Crear excepciones personalizadas que hereden de `Exception` o de una clase base de excepción más específica para errores de dominio o aplicación.

### 2.6. LINQ (Language Integrated Query)
- Usar la sintaxis de consulta o la sintaxis de método según la legibilidad. La sintaxis de método es a menudo más concisa.
  ```csharp
  // Sintaxis de método (generalmente preferida)
  var adultos = usuarios.Where(u => u.Edad >= 18).ToList();

  // Sintaxis de consulta
  var adultosQuery = from u in usuarios
                     where u.Edad >= 18
                     select u;
  var adultosList = adultosQuery.ToList();
  ```
- Evitar la ejecución múltiple de consultas LINQ que iteran sobre colecciones. Materializar la consulta con `.ToList()`, `.ToArray()`, etc., si se va a usar varias veces.

### 2.7. Principios SOLID
- Seguir los principios SOLID para un diseño de software robusto y mantenible.
  - **S**ingle Responsibility Principle (SRP)
  - **O**pen/Closed Principle (OCP)
  - **L**iskov Substitution Principle (LSP)
  - **I**nterface Segregation Principle (ISP)
  - **D**ependency Inversion Principle (DIP)

### 2.8. Inmutabilidad
- Preferir tipos inmutables (ej. `records` en C# 9+, `structs` de solo lectura, colecciones inmutables) cuando sea apropiado para mejorar la previsibilidad y seguridad en entornos multihilo.

### 2.9. Evitar Código Obsoleto
- No usar características del lenguaje o API que estén marcadas como obsoletas (`[Obsolete]`).

## 3. Convenciones Específicas del Proyecto GestMantIA

- **Idiomas**:
  - Nombres de variables, clases, métodos, etc.: Inglés.
  - Comentarios en el código: Español.
  - Mensajes de log: Español.
  - Interfaz de usuario: Español.
- **IDs de Entidades**: Usar `Guid` para todos los identificadores de entidades (claves primarias y foráneas), como se especifica en las memorias del proyecto.
- **DTOs (Data Transfer Objects)**: Usar `records` para definir DTOs.
- **Pruebas Unitarias (TDD)**:
  - Seguir la metodología TDD.
  - Nombrar los métodos de prueba siguiendo el patrón: `NombreMetodo_EstadoBajoPrueba_ComportamientoEsperado`.
    Ejemplo: `GetUserByIdAsync_UserExists_ReturnsUserDto`
- **Actualización de Documentos**:
  - `Roadmap.md`: Consultar al finalizar una tarea para determinar la siguiente. Marcar tareas completadas.
  - `Changelog.md`: Actualizar con un resumen de los cambios realizados al finalizar cada tarea.

Este documento es una guía viva y puede ser actualizado a medida que el proyecto evoluciona y surgen nuevas necesidades o mejores prácticas.
