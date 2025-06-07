# Script para instalar los paquetes necesarios para métricas

# Navegar al directorio del proyecto API
Set-Location -Path "$PSScriptRoot\..\src\GestMantIA.API"

# Instalar paquetes NuGet
dotnet add package InfluxDB.Client --version 6.0.0
dotnet add package App.Metrics --version 4.3.0
dotnet add package App.Metrics.AspNetCore --version 4.3.0
dotnet add package App.Metrics.Formatters.InfluxDB --version 4.3.0
dotnet add package App.Metrics.Formatters.Prometheus --version 4.3.0

Write-Host "Paquetes instalados correctamente." -ForegroundColor Green
