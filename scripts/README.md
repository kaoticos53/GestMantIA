# Scripts de Utilidad para GestMantIA

Este directorio contiene scripts útiles para el desarrollo y mantenimiento del proyecto GestMantIA, incluyendo CI/CD local y generación de componentes.

## Scripts Disponibles

### New-Component.ps1

Genera un nuevo componente basado en plantillas, incluyendo controladores, DTOs, comandos y pruebas unitarias.

**Uso:**
```powershell
.\New-Component.ps1 -ComponentName "Product" -FeatureName "Inventory"
```

**Parámetros:**
- `ComponentName`: Nombre del componente a generar (en singular, ej: "User").
- `FeatureName`: (Opcional) Nombre de la característica. Por defecto es igual a ComponentName.

**Ejemplo:**
```powershell
# Genera un componente de Producto dentro de la característica Inventory
.\New-Component.ps1 -ComponentName "Product" -FeatureName "Inventory"
```

### CI/CD Local

Scripts para ejecutar un flujo de CI/CD local en tu máquina de desarrollo.

## Requisitos

- Windows 10/11 o Linux/macOS
- PowerShell Core 7+ (recomendado) o PowerShell 5.1+
- Docker Desktop (con Docker Compose)
- .NET 9.0 SDK

## Plantillas

Las plantillas se encuentran en el directorio `.templates` en la raíz del proyecto. Incluyen:

- `dto.template.cs`: Plantilla para DTOs
- `controller.template.cs`: Plantilla para controladores de API
- `unittest.template.cs`: Plantilla para pruebas unitarias

## Convenciones

- Los nombres de los componentes deben estar en singular (ej: User, Product, Order)
- Los nombres de las características deben ser descriptivos y estar en singular
- Todas las rutas son relativas a la raíz del proyecto

## Configuración Inicial

1. Copia el archivo de configuración de ejemplo:
   ```bash
   cp scripts/env.dev.example .env.dev
   ```

2. Edita el archivo `.env.dev` con tus configuraciones locales.

## Uso del Script de CI/CD Local

### Desplegar en entorno de desarrollo

```powershell
# Navega a la raíz del proyecto
cd /ruta/a/gestmantia

# Ejecuta el script de CI/CD para desarrollo
.\scripts\local-ci-cd.ps1 -Environment dev
```

### Opciones disponibles

```
-Environment     Especifica el entorno de despliegue (dev, test, prod)
-SkipTests       Omite la ejecución de pruebas
-BuildOnly       Solo construye las imágenes sin desplegar
```

### Ejemplos

```powershell
# Solo construir las imágenes sin desplegar
.\scripts\local-ci-cd.ps1 -Environment dev -BuildOnly

# Desplegar en entorno de prueba omitiendo las pruebas
.\scripts\local-ci-cd.ps1 -Environment test -SkipTests
```

## Flujo de CI/CD Local

El script `local-ci-cd.ps1` realiza las siguientes acciones:

1. **Verificación de requisitos**
   - Comprueba que Docker, Docker Compose y .NET estén instalados
   - Verifica la existencia de los archivos de configuración necesarios

2. **Construcción**
   - Restaura las dependencias de .NET
   - Compila la solución en modo Release
   - Ejecuta pruebas unitarias y de integración (a menos que se omitan)
   - Construye las imágenes de Docker

3. **Despliegue**
   - Detiene y elimina los contenedores existentes
   - Inicia los servicios definidos en los archivos docker-compose
   - Muestra las URLs de acceso a la aplicación

## Configuración de Entornos

Puedes crear diferentes archivos `.env` para cada entorno:

- `.env.dev` - Desarrollo local
- `.env.test` - Entorno de pruebas
- `.env.prod` - Producción

Cada archivo debe contener las variables de entorno específicas para ese entorno.

## Solución de Problemas

### Error de permisos en Linux/macOS

Si estás en Linux o macOS, asegúrate de que el script sea ejecutable:

```bash
chmod +x ./scripts/local-ci-cd.ps1
```

### Problemas con Docker

Si tienes problemas con Docker, verifica que el servicio esté en ejecución:

```bash
docker info
```

### Problemas con las pruebas

Si las pruebas fallan, puedes ejecutarlas manualmente para ver más detalles:

```bash
dotnet test tests/GestMantIA.UnitTests -c Release
```
