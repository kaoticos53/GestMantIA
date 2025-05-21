<#
.SYNOPSIS
    Actualiza el archivo CHANGELOG.md con nuevas entradas de cambios.

.DESCRIPTION
    Este script facilita la actualización del archivo CHANGELOG.md siguiendo el formato de Keep a Changelog.
    Permite agregar entradas para diferentes categorías de cambios.

.PARAMETER Version
    La versión para la que se están registrando los cambios (ej: 1.0.0). Si no se especifica,
    se usará "No Publicado".

.PARAMETER Date
    La fecha de la versión en formato YYYY-MM-DD. Por defecto es la fecha actual.

.PARAMETER Added
    Lista de características nuevas agregadas.

.PARAMETER Changed
    Lista de cambios en funcionalidades existentes.

.PARAMETER Deprecated
    Lista de funcionalidades marcadas como obsoletas.

.PARAMETER Removed
    Lista de funcionalidades eliminadas.

.PARAMETER Fixed
    Lista de errores corregidos.

.PARAMETER Security
    Lista de vulnerabilidades corregidas.

.EXAMPLE
    .\update-changelog.ps1 -Version "1.0.0" -Added "Nueva característica X", "Mejora en Y"

.EXAMPLE
    .\update-changelog.ps1 -Fixed "Corregido error en login", "Solucionado problema de rendimiento"
#>

param(
    [string]$Version = "No Publicado",
    [string]$Date = (Get-Date -Format "yyyy-MM-dd"),
    [string[]]$Added = @(),
    [string[]]$Changed = @(),
    [string[]]$Deprecated = @(),
    [string[]]$Removed = @(),
    [string[]]$Fixed = @(),
    [string[]]$Security = @()
)

# Configuración
$changelogPath = Join-Path $PSScriptRoot "..\CHANGELOG.md"
$tempFile = [System.IO.Path]::GetTempFileName()
$changesFound = $false

# Verificar si el archivo CHANGELOG.md existe
if (-not (Test-Path $changelogPath)) {
    Write-Error "No se encontró el archivo CHANGELOG.md en la ruta: $changelogPath"
    exit 1
}

# Crear la entrada de cambio
$changeEntry = @"
## [$Version] - $Date

"@

# Agregar secciones con cambios
$sections = @{
    "### Agregado" = $Added
    "### Cambiado" = $Changed
    "### Obsoleto" = $Deprecated
    "### Eliminado" = $Removed
    "### Corregido" = $Fixed
    "### Seguridad" = $Security
}

foreach ($section in $sections.GetEnumerator()) {
    $items = $section.Value | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    if ($items.Count -gt 0) {
        $changeEntry += "`n" + $section.Key + "`n"
        $changeEntry += ($items | ForEach-Object { "- $_`n" }) -join ""
        $changesFound = $true
    }
}

if (-not $changesFound) {
    Write-Host "No se especificaron cambios para registrar." -ForegroundColor Yellow
    exit 0
}

try {
    # Leer el archivo original
    $content = Get-Content $changelogPath -Raw
    
    # Encontrar la posición después del encabezado
    $headerPattern = '(?s)^(#.*?\n\n)'
    $headerMatch = [regex]::Match($content, $headerPattern)
    
    if (-not $headerMatch.Success) {
        throw "No se pudo encontrar el encabezado del archivo CHANGELOG.md"
    }
    
    # Insertar la nueva entrada después del encabezado
    $newContent = $content.Insert($headerMatch.Groups[1].Length, $changeEntry)
    
    # Guardar el archivo temporal
    [System.IO.File]::WriteAllText($tempFile, $newContent, [System.Text.Encoding]::UTF8)
    
    # Reemplazar el archivo original
    Move-Item -Path $tempFile -Destination $changelogPath -Force
    
    Write-Host "CHANGELOG.md actualizado exitosamente con los cambios de la versión: $Version" -ForegroundColor Green
}
catch {
    Write-Error "Error al actualizar el CHANGELOG.md: $_"
    if (Test-Path $tempFile) {
        Remove-Item $tempFile -Force
    }
    exit 1
}
