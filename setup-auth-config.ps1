# Script para configurar la autenticación en GestMantIA

# Directorio base del proyecto
$projectDir = "$PSScriptRoot"
$apiDir = Join-Path $projectDir "src\GestMantIA.API"

# Crear directorio de configuración si no existe
$configDir = Join-Path $apiDir "Config"
if (-not (Test-Path $configDir)) {
    New-Item -ItemType Directory -Path $configDir | Out-Null
}

# Configuración para Development
$devConfig = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GestMantIA;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "clave-secreta-muy-larga-y-segura-para-desarrollo-cambiar-en-produccion",
    "Issuer": "GestMantIA.API",
    "Audience": "GestMantIA.Clients",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "EmailSettings": {
    "FromEmail": "noreply@gestmantia.com",
    "FromName": "GestMantIA",
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "tucorreo@gmail.com",
    "Password": "tucontraseña"
  },
  "Application": {
    "AppUrl": "https://localhost:5001",
    "ApiUrl": "https://localhost:5001"
  }
}
"@

# Escribir configuración de desarrollo
$devConfigPath = Join-Path $apiDir "appsettings.Development.json"
$devConfig | Out-File -FilePath $devConfigPath -Encoding utf8

# Configuración de producción (sin datos sensibles)
$prodConfig = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
"@

# Escribir configuración de producción
$prodConfigPath = Join-Path $apiDir "appsettings.Production.json"
$prodConfig | Out-File -FilePath $prodConfigPath -Encoding utf8

Write-Host "Configuración de autenticación generada correctamente." -ForegroundColor Green
Write-Host "Por favor, configura las variables sensibles en los siguientes archivos:" -ForegroundColor Yellow
Write-Host "- $devConfigPath" -ForegroundColor Cyan
Write-Host "- $prodConfigPath" -ForegroundColor Cyan

# Crear archivo .env para desarrollo (si se usa)
$envPath = Join-Path $projectDir ".env"
if (-not (Test-Path $envPath)) {
    @"
# Configuración de conexión a la base de datos
CONNECTION_STRING=Server=localhost;Database=GestMantIA;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True

# Configuración JWT
JWT_SECRET=TuClaveSecretaMuySeguraParaDesarrollo
JWT_ISSUER=GestMantIA.API
JWT_AUDIENCE=GestMantIA.Clients
JWT_EXPIRE_MINUTES=60
JWT_REFRESH_EXPIRE_DAYS=7

# Configuración de correo
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=tucorreo@gmail.com
SMTP_PASSWORD=tucontraseña
SMTP_FROM=noreply@gestmantia.com
SMTP_FROM_NAME=GestMantIA

# Configuración de la aplicación
APP_URL=https://localhost:3000
API_URL=https://localhost:5001
"@ | Out-File -FilePath $envPath -Encoding utf8
    
    Write-Host "Archivo .env de ejemplo creado en: $envPath" -ForegroundColor Cyan
}

# Instalar herramientas de desarrollo de .NET (si no están instaladas)
$dotnetTools = @("dotnet-ef")
foreach ($tool in $dotnetTools) {
    $toolInstalled = dotnet tool list --global | Select-String $tool
    if (-not $toolInstalled) {
        Write-Host "Instalando herramienta global de .NET: $tool" -ForegroundColor Yellow
        dotnet tool install --global $tool
    }
}

# Aplicar migraciones
Write-Host "Aplicando migraciones de base de datos..." -ForegroundColor Yellow
Set-Location $apiDir
dotnet ef database update

Write-Host "¡Configuración completada con éxito!" -ForegroundColor Green
Write-Host "Puedes iniciar la aplicación con: dotnet run --project src/GestMantIA.API" -ForegroundColor Cyan
