<#
.SYNOPSIS
    Genera un nuevo componente basado en plantillas.
.DESCRIPTION
    Este script crea los archivos necesarios para un nuevo componente (controlador, DTOs, comandos, consultas, etc.)
    basado en las plantillas predefinidas.
.PARAMETER ComponentName
    Nombre del componente a generar (en singular, ej: "User").
.PARAMETER FeatureName
    Nombre de la característica (opcional, por defecto es igual a ComponentName).
.EXAMPLE
    .\New-Component.ps1 -ComponentName "Product" -FeatureName "Inventory"
#>

param (
    [Parameter(Mandatory=$true)]
    [string]$ComponentName,
    
    [string]$FeatureName = $ComponentName
)

# Configuración
$templatesPath = "$PSScriptRoot\..\.templates"
$solutionRoot = "$PSScriptRoot\.."
$srcPath = "$solutionRoot\src"

# Validar que el componente no exista ya
$featurePath = "$srcPath\GestMantIA.Core\Application\Features\$FeatureName"
if (Test-Path $featurePath) {
    Write-Error "La característica '$FeatureName' ya existe en $featurePath"
    exit 1
}

# Crear directorios necesarios
$directories = @(
    "$featurePath\Commands\Create$ComponentName",
    "$featurePath\Commands\Update$ComponentName",
    "$featurePath\Commands\Delete$ComponentName",
    "$featurePath\Queries\Get$($ComponentName)ById",
    "$featurePath\Queries\Get$($ComponentName)List",
    "$featurePath\Dtos"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "Directorio creado: $dir" -ForegroundColor Green
    }
}

# Función para reemplazar marcadores en las plantillas
function Expand-Template {
    param (
        [string]$templatePath,
        [hashtable]$replacements
    )
    
    if (-not (Test-Path $templatePath)) {
        Write-Error "Plantilla no encontrada: $templatePath"
        return $null
    }
    
    $content = Get-Content -Path $templatePath -Raw
    
    foreach ($key in $replacements.Keys) {
        $content = $content -replace "\{\{$key\}\}", $replacements[$key]
    }
    
    return $content
}

# Valores para reemplazar en las plantillas
$replacements = @{
    "FeatureName" = $FeatureName
    "FeatureNamePlural" = if ($FeatureName.EndsWith("y")) { $FeatureName.Substring(0, $FeatureName.Length-1) + "ies" } else { $FeatureName + "s" }
    "ControllerName" = $ComponentName
    "ControllerNamePlural" = if ($ComponentName.EndsWith("y")) { $ComponentName.Substring(0, $ComponentName.Length-1) + "ies" } else { $ComponentName + "s" }
    "DtoName" = $ComponentName
    "EntityName" = $ComponentName
    "HandlerName" = $ComponentName
    "TestName" = $ComponentName
    "CommandName" = $ComponentName
    "PropertyName" = $ComponentName
    "PropertyType" = "string"
    "PropertyDescription" = "Descripción de la propiedad"
    "Description" = "Descripción del DTO"
    "ExpectedBehavior" = "RetornarExito"
    "Condition" = "SeProporcionanDatosValidos"
    "InvalidCondition" = "SeProporcionanDatosInvalidos"
}

# Generar archivos a partir de plantillas
$templates = @(
    @{
        Template = "$templatesPath\dto.template.cs"
        Output = "$featurePath\Dtos\$($ComponentName)Dto.cs"
    },
    @{
        Template = "$templatesPath\controller.template.cs"
        Output = "$srcPath\GestMantIA.API\Controllers\$($ComponentName)Controller.cs"
    },
    @{
        Template = "$templatesPath\unittest.template.cs"
        Output = "$solutionRoot\tests\GestMantIA.UnitTests\Application\Features\$FeatureName\$($ComponentName)Tests.cs"
    }
)

foreach ($template in $templates) {
    $content = Expand-Template -templatePath $template.Template -replacements $replacements
    
    if ($content) {
        $outputDir = Split-Path -Path $template.Output -Parent
        if (-not (Test-Path $outputDir)) {
            New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
        }
        
        $content | Out-File -FilePath $template.Output -Encoding utf8
        Write-Host "Archivo generado: $($template.Output)" -ForegroundColor Green
    }
}

Write-Host "\n¡Componente '$ComponentName' generado exitosamente!" -ForegroundColor Cyan
Write-Host "Revisa los archivos generados y completa la implementación según sea necesario." -ForegroundColor Cyan
