# MigrateToVerticalSlice.ps1
# Script para automatizar la migración a arquitectura vertical slice en GestMantIA
# Mensajes y logs en español

param(
    [string]$SolutionRoot = "..\src"
)

function Log-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message"
}

function Move-Service {
    param(
        [string]$ServiceFile,
        [string]$FeatureName
    )
    $targetDir = Join-Path -Path $SolutionRoot -ChildPath "GestMantIA.Application\Features\$FeatureName\Services"
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir | Out-Null
        Log-Info "Carpeta creada: $targetDir"
    }
    $targetFile = Join-Path -Path $targetDir -ChildPath (Split-Path $ServiceFile -Leaf)
    Move-Item $ServiceFile $targetFile -Force
    Log-Info "Servicio movido: $ServiceFile -> $targetFile"
}

function Move-Repository {
    param(
        [string]$RepoFile,
        [string]$FeatureName
    )
    $targetDir = Join-Path -Path $SolutionRoot -ChildPath "GestMantIA.Infrastructure\Features\$FeatureName\Repositories"
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir | Out-Null
        Log-Info "Carpeta creada: $targetDir"
    }
    $targetFile = Join-Path -Path $targetDir -ChildPath (Split-Path $RepoFile -Leaf)
    Move-Item $RepoFile $targetFile -Force
    Log-Info "Repositorio movido: $RepoFile -> $targetFile"
}

function Update-Namespaces {
    param(
        [string]$FilePath,
        [string]$OldNamespace,
        [string]$NewNamespace
    )
    (Get-Content $FilePath) -replace $OldNamespace, $NewNamespace | Set-Content $FilePath
    Log-Info "Namespace actualizado en $FilePath"
}

# Ejemplo de uso manual:
# Move-Service "$SolutionRoot\GestMantIA.Infrastructure\Services\UserService.cs" "UserManagement"
# Move-Repository "$SolutionRoot\GestMantIA.Infrastructure\Data\Repositories\UserRepository.cs" "UserManagement"
# Update-Namespaces "<ruta archivo>" "GestMantIA.Infrastructure.Services" "GestMantIA.Application.Features.UserManagement.Services"

Log-Info "Script de migración vertical slice preparado. Personaliza las llamadas a las funciones según las necesidades del proyecto."
