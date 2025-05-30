# AutoMigrateToVerticalSlice.ps1
# Script para automatizar completamente la migración vertical slice de los servicios y repositorios identificados
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
    return $targetFile
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
    return $targetFile
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

# --- MIGRACIÓN AUTOMÁTICA ---

# 1. Servicios
$UserService = Join-Path $SolutionRoot "GestMantIA.Infrastructure\Services\UserService.cs"
$RoleService = Join-Path $SolutionRoot "GestMantIA.Infrastructure\Services\RoleService.cs"

$UserServiceNew = Move-Service $UserService "UserManagement"
Update-Namespaces $UserServiceNew "GestMantIA.Infrastructure.Services" "GestMantIA.Application.Features.UserManagement.Services"

$RoleServiceNew = Move-Service $RoleService "UserManagement"
Update-Namespaces $RoleServiceNew "GestMantIA.Infrastructure.Services" "GestMantIA.Application.Features.UserManagement.Services"

# 2. Repositorios
$Repository = Join-Path $SolutionRoot "GestMantIA.Infrastructure\Data\Repositories\Repository.cs"
$UserProfileRepository = Join-Path $SolutionRoot "GestMantIA.Infrastructure\Data\Repositories\UserProfileRepository.cs"
$UserRepository = Join-Path $SolutionRoot "GestMantIA.Infrastructure\Data\Repositories\UserRepository.cs"

$RepositoryNew = Move-Repository $Repository "UserManagement"
Update-Namespaces $RepositoryNew "GestMantIA.Infrastructure.Data.Repositories" "GestMantIA.Infrastructure.Features.UserManagement.Repositories"

$UserProfileRepositoryNew = Move-Repository $UserProfileRepository "UserManagement"
Update-Namespaces $UserProfileRepositoryNew "GestMantIA.Infrastructure.Data.Repositories" "GestMantIA.Infrastructure.Features.UserManagement.Repositories"

$UserRepositoryNew = Move-Repository $UserRepository "UserManagement"
Update-Namespaces $UserRepositoryNew "GestMantIA.Infrastructure.Data.Repositories" "GestMantIA.Infrastructure.Features.UserManagement.Repositories"

Log-Info "Migración vertical slice completada automáticamente. Revisa los archivos consumidores para actualizar referencias si es necesario. Ejecuta los tests para validar la solución."
