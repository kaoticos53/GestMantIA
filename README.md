# GestMantIA

Sistema de gestión de mantenimiento de equipos electrónicos para canales de televisión.

## Requisitos

- .NET 9.0 SDK o superior
- Docker y Docker Compose
- PostgreSQL 16 o superior
- Node.js 18+ (para el frontend)

## Configuración del entorno

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/tuusuario/gestmantia.git
   cd gestmantia
   ```

2. Configurar la base de datos:
   - Asegúrate de tener PostgreSQL en ejecución
   - Crea una base de datos llamada `GestMantIA_Dev` (o el nombre que prefieras)

3. Configurar el archivo de configuración:
   - Copia el archivo de ejemplo de configuración:
     ```bash
     cp docs/configuration-examples/appsettings.Development.json.example src/GestMantIA.API/appsettings.Development.json
     ```
   - Edita el archivo `appsettings.Development.json` con tus configuraciones

4. Configuración de JWT (opcional):
   - Genera una clave secreta segura para JWT
   - Actualiza la configuración en `appsettings.Development.json`
   - En desarrollo, puedes usar la clave de ejemplo, pero NUNCA en producción

5. Iniciar la aplicación:
   ```bash
   # Usando Docker Compose (recomendado)
   docker-compose up --build
   
   # O ejecutando directamente con .NET
   cd src/GestMantIA.API
   dotnet run --environment=Development
   ```

6. Acceder a la aplicación:
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger
   - Aplicación web: http://localhost:5001
   - pgAdmin: http://localhost:5050 (si usas Docker Compose)

## Registro de Cambios (Changelog)

Este proyecto mantiene un registro detallado de los cambios en [CHANGELOG.md](CHANGELOG.md) siguiendo el estándar [Keep a Changelog](https://keepachangelog.com/).

### Cómo Actualizar el Changelog

1. **Usando el script de actualización** (recomendado):
   ```powershell
   # Ejemplo: Agregar una nueva característica
   .\scripts\update-changelog.ps1 -Version "1.0.0" -Added "Nueva funcionalidad de autenticación"
   
   # Ejemplo: Reportar una corrección
   .\scripts\update-changelog.ps1 -Fixed "Corregido error en el inicio de sesión"
   
   # Ver ayuda del script
   Get-Help .\scripts\update-changelog.ps1 -Detailed
   ```

2. **Manual**: Edita directamente el archivo [CHANGELOG.md](CHANGELOG.md) siguiendo el formato existente.

### Categorías de Cambios

- **Agregado**: Para nuevas características
- **Cambiado**: Para cambios en funcionalidades existentes
- **Obsoleto**: Para funcionalidades que se eliminarán en futuras versiones
- **Eliminado**: Para funcionalidades eliminadas
- **Corregido**: Para corrección de errores
- **Seguridad**: En caso de vulnerabilidades corregidas

## Configuración de Autenticación JWT

La API utiliza autenticación basada en JWT (JSON Web Tokens). Para configurarla:

1. **Generar una clave secreta segura** (mínimo 32 caracteres)
2. **Configurar las opciones de JWT** en `appsettings.Development.json`:
   ```json
   "Jwt": {
     "Key": "TuClaveSecretaMuyLargaYCompleja",
     "Issuer": "GestMantIA-API",
     "Audience": "GestMantIA-Client",
     "ExpireDays": 7,
     "RefreshTokenExpireDays": 30
   }
   ```

3. **Habilitar autenticación** en los controladores con el atributo `[Authorize]`

4. **Obtener un token JWT** mediante el endpoint de autenticación

Para desarrollo, puedes usar la configuración de ejemplo, pero asegúrate de usar una clave segura en producción.

## CI/CD Local

El proyecto incluye un sistema de CI/CD local basado en scripts de PowerShell que te permite automatizar el proceso de construcción, pruebas y despliegue en tu máquina local.

### Características Principales

- **Automatización completa** desde la construcción hasta el despliegue
- **Soporte para múltiples entornos** (desarrollo, pruebas, producción)
- **Verificación automática** de requisitos previos
- **Integración con Docker** para entornos aislados
- **Documentación detallada** en [scripts/README.md](scripts/README.md)

### Uso Básico

1. **Preparación**:
   ```bash
   # Copia la plantilla de configuración
   cp scripts/env.dev.example .env.dev
   
   # Edita el archivo .env.dev con tus configuraciones
   # (puedes usar cualquier editor de texto)
   ```

2. **Despliegue en desarrollo**:
   ```powershell
   # Navega a la raíz del proyecto
   cd /ruta/a/gestmantia
   
   # Ejecuta el script de CI/CD para desarrollo
   .\scripts\local-ci-cd.ps1 -Environment dev
   ```

### Opciones Disponibles

```
-Environment     Especifica el entorno de despliegue (dev, test, prod)
-SkipTests       Omite la ejecución de pruebas
-BuildOnly       Solo construye las imágenes sin desplegar
```

### Ejemplos de Uso

```powershell
# Solo construir las imágenes sin desplegar
.\scripts\local-ci-cd.ps1 -Environment dev -BuildOnly

# Desplegar en entorno de prueba omitiendo las pruebas
.\scripts\local-ci-cd.ps1 -Environment test -SkipTests

# Despliegue en producción (requiere configuración adicional)
.\scripts\local-ci-cd.ps1 -Environment prod
```

### Flujo de CI/CD Local

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

### Configuración de Entornos

Puedes crear diferentes archivos `.env` para cada entorno:

- `.env.dev` - Desarrollo local
- `.env.test` - Entorno de pruebas
- `.env.prod` - Producción

Cada archivo debe contener las variables de entorno específicas para ese entorno.

### Documentación Adicional

Para más detalles, consulta la documentación completa en [scripts/README.md](scripts/README.md).

## Configuración de CI/CD en la Nube (Opcional)

El proyecto también incluye configuración para CI/CD en la nube usando GitHub Actions. Para más información, consulta la documentación en [.github/workflows/README.md](.github/workflows/README.md).

Para que el CI/CD funcione correctamente, debes configurar los siguientes secretos en tu repositorio de GitHub (Settings > Secrets > Actions):

- `DOCKER_HUB_USERNAME`: Tu nombre de usuario de Docker Hub
- `DOCKER_HUB_TOKEN`: Tu token de acceso a Docker Hub
- `SSH_PRIVATE_KEY`: Clave SSH privada para el despliegue
- `SLACK_WEBHOOK_URL` (opcional): URL del webhook de Slack para notificaciones

### Variables de entorno de producción

Para el entorno de producción, asegúrate de configurar las siguientes variables de entorno:

```env
# Base de datos
DB_NAME=gestmantia_prod
DB_USER=gestmantia_prod
DB_PASSWORD=tu_contraseña_segura

# JWT
JWT_KEY=tu_clave_secreta_muy_larga_y_segura
JWT_ISSUER=GestMantIA-Production
JWT_AUDIENCE=GestMantIA-Client
JWT_EXPIRE_DAYS=7

# Otras configuraciones
ASPNETCORE_ENVIRONMENT=Production
```

## Estructura del proyecto

- `src/`: Código fuente de la aplicación
  - `GestMantIA.API/`: API RESTful
  - `GestMantIA.Core/`: Lógica de negocio y entidades de dominio
  - `GestMantIA.Infrastructure/`: Acceso a datos e implementaciones de infraestructura
  - `GestMantIA.Web/`: Aplicación web Blazor
- `tests/`: Pruebas unitarias y de integración
  - `GestMantIA.UnitTests/`
  - `GestMantIA.IntegrationTests/`
- `.github/workflows/`: Flujos de trabajo de GitHub Actions

## Tecnologías utilizadas

- Backend: .NET 9.0, Entity Framework Core, PostgreSQL
- Frontend: Blazor WebAssembly, MudBlazor
- Contenedores: Docker, Docker Compose
- Pruebas: xUnit, Moq, FluentAssertions
- CI/CD: GitHub Actions, Docker Hub

## Despliegue en producción

Para desplegar en producción, sigue estos pasos:

1. Asegúrate de que todas las variables de entorno estén configuradas correctamente
2. Crea un tag de versión:
   ```bash
   git tag -a v1.0.0 -m "Versión 1.0.0"
   git push origin v1.0.0
   ```
3. El flujo de CI/CD se ejecutará automáticamente para la nueva versión

## Monitoreo

Se recomienda configurar un sistema de monitoreo para la aplicación en producción. Algunas opciones recomendadas:

- **APM**: Application Insights o New Relic
- **Logs**: ELK Stack (Elasticsearch, Logstash, Kibana) o Seq
- **Métricas**: Prometheus y Grafana

## Licencia

MIT
