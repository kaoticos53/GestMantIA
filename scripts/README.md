# Scripts de CI/CD Local para GestMantIA

Este directorio contiene los scripts necesarios para ejecutar un flujo de CI/CD local en tu máquina de desarrollo.

## Requisitos

- Windows 10/11 o Linux/macOS
- PowerShell Core 7+ (recomendado) o PowerShell 5.1+
- Docker Desktop (con Docker Compose)
- .NET 9.0 SDK

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
