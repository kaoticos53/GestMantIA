<#
.SYNOPSIS
    Script de CI/CD local para GestMantIA

.DESCRIPTION
    Este script automatiza el proceso de construcción, pruebas y despliegue local de la aplicación GestMantIA.
    Soporta diferentes entornos: desarrollo, pruebas y producción.

.PARAMETER Environment
    Especifica el entorno de despliegue. Valores posibles: dev, test, prod

.PARAMETER SkipTests
    Omite la ejecución de pruebas durante el despliegue

.PARAMETER BuildOnly
    Solo construye las imágenes sin desplegar

.EXAMPLE
    .\local-ci-cd.ps1 -Environment dev
    Despliega la aplicación en el entorno de desarrollo

.EXAMPLE
    .\local-ci-cd.ps1 -Environment test -SkipTests
    Despliega en el entorno de prueba omitiendo las pruebas
#>


param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("dev", "test", "prod")]
    [string]$Environment,
    
    [switch]$SkipTests,
    [switch]$BuildOnly
)

# Configuración
$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Join-Path $scriptPath ".." | Resolve-Path
$envFiles = @{
    "dev"  = ".env.dev"
    "test" = ".env.test"
    "prod" = ".env.prod"
}
$composeFiles = @(
    "docker-compose.yml"
    "docker-compose.$Environment.yml"
)

# Funciones
function Write-Header($message) {
    Write-Host "`n=== $($message.ToUpper()) ===`n" -ForegroundColor Cyan
}

function Write-Success($message) {
    Write-Host "[OK] $message" -ForegroundColor Green
}

function Write-Info($message) {
    Write-Host "[INFO] $message" -ForegroundColor Blue
}

function Write-Warning($message) {
    Write-Host "[ADVERTENCIA] $message" -ForegroundColor Yellow
}

function Write-Error($message) {
    Write-Host "[ERROR] $message" -ForegroundColor Red
    exit 1
}

function Test-CommandExists($command) {
    try {
        Get-Command $command -ErrorAction Stop | Out-Null
        return $true
    } catch {
        return $false
    }
}

# Verificar requisitos
Write-Header "Verificando requisitos"

$requiredCommands = @("docker", "docker-compose", "dotnet")
foreach ($cmd in $requiredCommands) {
    if (-not (Test-CommandExists $cmd)) {
        Write-Error "Se requiere $cmd pero no está instalado o no está en el PATH"
    }
    Write-Success "$cmd está disponible"
}

# Verificar archivos de configuración
Write-Header "Verificando archivos de configuración"

$envFile = Join-Path $rootPath $envFiles[$Environment]
if (-not (Test-Path $envFile)) {
    Write-Error "No se encontró el archivo de variables de entorno: $($envFiles[$Environment])"
}
Write-Success "Archivo de entorno encontrado: $($envFiles[$Environment])"

# Cargar variables de entorno
Write-Header "Cargando variables de entorno"
Get-Content $envFile | ForEach-Object {
    if ($_ -match '^([^#][^=]+)=(.*)') {
        $name = $matches[1].Trim()
        $value = $matches[2].Trim()
        [Environment]::SetEnvironmentVariable($name, $value, 'Process')
        Write-Info "Cargada variable: $name"
    }
}

# Construir la aplicación
Write-Header "Construyendo la aplicación"

try {
    # Restaurar dependencias
    Write-Info "Restaurando dependencias..."
    Set-Location $rootPath
    dotnet restore
    
    # Construir la solución
    Write-Info "Compilando la solución..."
    dotnet build -c Release --no-restore
    
    # Ejecutar pruebas (a menos que se indique lo contrario)
    if (-not $SkipTests) {
        Write-Header "Ejecutando pruebas"
        
        $testProjects = @(
            (Join-Path $rootPath "tests\GestMantIA.UnitTests\GestMantIA.UnitTests.csproj"),
            (Join-Path $rootPath "tests\GestMantIA.IntegrationTests\GestMantIA.IntegrationTests.csproj")
        )
        
        foreach ($testProject in $testProjects) {
            if (Test-Path $testProject) {
                Write-Info "Ejecutando pruebas en: $(Split-Path $testProject -Leaf)"
                dotnet test $testProject -c Release --no-build --verbosity normal --logger "trx;LogFileName=$(Split-Path $testProject -Leaf).trx"
                if ($LASTEXITCODE -ne 0) {
                    Write-Error "Las pruebas han fallado"
                }
            } else {
                Write-Warning "No se encontró el proyecto de pruebas: $testProject"
            }
        }
    } else {
        Write-Warning "Se omitieron las pruebas (parámetro -SkipTests)"
    }
    
    # Construir imágenes Docker
    Write-Header "Construyendo imágenes Docker"
    
    $composeArgs = @("--project-directory", $rootPath)
    foreach ($file in $composeFiles) {
        $composeArgs += "-f"
        $composeArgs += (Join-Path $rootPath $file)
    }
    
    docker-compose $composeArgs build --no-cache
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al construir las imágenes Docker"
    }
    
    Write-Success "Imágenes construidas correctamente"
    
    # Si solo es construcción, terminar aquí
    if ($BuildOnly) {
        Write-Header "Modo solo construcción completado"
        exit 0
    }
    
    # Desplegar la aplicación
    Write-Header "Desplegando la aplicación en entorno $Environment"
    
    # Detener y eliminar contenedores existentes
    Write-Info "Deteniendo contenedores existentes..."
    docker-compose $composeArgs down --remove-orphans
    
    # Iniciar los servicios
    Write-Info "Iniciando servicios..."
    docker-compose $composeArgs up -d --force-recreate
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al iniciar los servicios"
    }
    
    # Mostrar información del despliegue
    Write-Header "Despliegue completado exitosamente"
    Write-Success "Aplicación desplegada en el entorno: $Environment"
    Write-Info "Servicios en ejecución:"
    docker-compose $composeArgs ps
    
    # Mostrar URLs de acceso
    $apiPort = if ($Environment -eq "prod") { "80" } else { "5000" }
    $webPort = if ($Environment -eq "prod") { "80" } else { "5001" }
    
    Write-Info "`nURLs de acceso:"
    Write-Host "  - API:   http://localhost:$apiPort"
    Write-Host "  - Web:   http://localhost:$webPort"
    Write-Host "  - Health: http://localhost:$apiPort/health"
    
} catch {
    Write-Error "Error durante el despliegue: $_"
}
