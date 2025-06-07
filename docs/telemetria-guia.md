# Guía de Configuración de Telemetría para .NET 9 con InfluxDB y Grafana

## Tabla de Contenidos
1. [Introducción](#introducción)
2. [Requisitos Previos](#requisitos-previos)
3. [Arquitectura de la Solución](#arquitectura-de-la-solución)
4. [Configuración de InfluxDB](#configuración-de-influxdb)
5. [Configuración de la Aplicación .NET](#configuración-de-la-aplicación-net)
6. [Configuración de Telegraf](#configuración-de-telegraf)
7. [Configuración de Grafana](#configuración-de-grafana)
8. [Monitoreo de la Aplicación](#monitoreo-de-la-aplicación)
9. [Solución de Problemas](#solución-de-problemas)
10. [Mejores Prácticas](#mejores-prácticas)

## Introducción

Esta guía explica cómo configurar un sistema de telemetría completo para aplicaciones .NET 9 utilizando InfluxDB como base de datos de series temporales, Telegraf para la recolección de métricas del sistema y Grafana para la visualización de datos.

## Requisitos Previos

- Docker y Docker Compose instalados
- .NET 9 SDK
- Acceso a línea de comandos (PowerShell, Bash, etc.)
- Editor de código (Visual Studio, VS Code, etc.)

## Arquitectura de la Solución

```
+----------------+    +----------------+    +----------------+    +----------------+
|                |    |                |    |                |    |                |
|    Aplicación  |--->|    Telegraf    |--->|    InfluxDB    |<---|    Grafana     |
|    .NET 9      |    |                |    |                |    |                |
|                |    |                |    |                |    |                |
+----------------+    +----------------+    +----------------+    +----------------+
```

## Configuración de InfluxDB

### 1. Crear un archivo `docker-compose.telemetry.yml`

```yaml
version: '3.8'

services:
  influxdb:
    image: influxdb:2.7
    container_name: influxdb
    ports:
      - "8086:8086"
    volumes:
      - influxdb-data:/var/lib/influxdb2
    environment:
      - DOCKER_INFLUXDB_INIT_MODE=setup
      - DOCKER_INFLUXDB_INIT_USERNAME=admin
      - DOCKER_INFLUXDB_INIT_PASSWORD=admin123
      - DOCKER_INFLUXDB_INIT_ORG=gestmantia
      - DOCKER_INFLUXDB_INIT_BUCKET=gestmantia
      - DOCKER_INFLUXDB_INIT_ADMIN_TOKEN=my-super-secret-auth-token
    networks:
      - telemetry-net

  telegraf:
    image: telegraf:latest
    container_name: telegraf
    volumes:
      - ./docker/telemetry/telegraf/telegraf.conf:/etc/telegraf/telegraf.conf
    depends_on:
      - influxdb
    networks:
      - telemetry-net

  grafana:
    image: grafana/grafana:latest
    container_name: grafana
    ports:
      - "3000:3000"
    volumes:
      - grafana-data:/var/lib/grafana
      - ./docker/telemetry/grafana/provisioning:/etc/grafana/provisioning
    environment:
      - GF_SECURITY_ADMIN_USER=admin
      - GF_SECURITY_ADMIN_PASSWORD=admin
    depends_on:
      - influxdb
    networks:
      - telemetry-net

volumes:
  influxdb-data:
  grafana-data:

networks:
  telemetry-net:
    driver: bridge
```

### 2. Configurar Telegraf

Crea el directorio y el archivo de configuración para Telegraf:

```bash
mkdir -p docker/telemetry/telegraf
```

Crea el archivo `docker/telemetry/telegraf/telegraf.conf`:

```ini
[agent]
  interval = "10s"
  round_interval = true
  metric_batch_size = 1000
  metric_buffer_limit = 10000
  collection_jitter = "0s"
  flush_interval = "10s"
  flush_jitter = "0s"
  precision = ""
  debug = false
  quiet = false
  logfile = ""
  hostname = "$HOSTNAME"
  omit_hostname = false

# Salida a InfluxDB v2
[[outputs.influxdb_v2]]
  urls = ["http://influxdb:8086"]
  token = "my-super-secret-auth-token"
  organization = "gestmantia"
  bucket = "gestmantia"

# Entrada de métricas del sistema
[[inputs.cpu]]
  percpu = true
  totalcpu = true
  collect_cpu_time = false
  report_active = false

[[inputs.mem]]

[[inputs.disk]]
  ignore_fs = ["tmpfs", "devtmpfs", "devfs", "iso9660", "overlay", "aufs", "squashfs"]

[[inputs.diskio]]

[[inputs.net]]
  interfaces = ["eth0", "eth1"]
  ignore_protocol_stats = true

[[inputs.system]]

# Métricas de Docker
[[inputs.docker]]
  endpoint = "unix:///var/run/docker.sock"
  container_names = []
  timeout = "5s"
  perdevice = true
  total = false

# Métricas HTTP de la API
[[inputs.http_response]]
  urls = ["http://host.docker.internal:5000/health"]
  response_timeout = "5s"
  method = "GET"
  [inputs.http_response.tags]
    service = "gestmantia-api"
```

## Configuración de la Aplicación .NET

### 1. Instalar paquetes NuGet

```bash
dotnet add package App.Metrics
```

### 2. Configurar la inyección de dependencias

En `Program.cs`:

```csharp
using App.Metrics;
using App.Metrics.Formatters.Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Configuración de métricas
var metrics = new MetricsBuilder()
    .OutputMetrics.AsPrometheusPlainText()
    .Build();

builder.Services.AddMetrics(metrics);
builder.Services.AddMetricsEndpoints(options =>
{
    options.MetricsTextEndpointOutputFormatter = metrics.OutputMetricsFormatters
        .OfType<MetricsPrometheusTextOutputFormatter>().First();
});

var app = builder.Build();

app.UseMetricsAllMiddleware();
app.UseMetricsAllEndpoints();

app.Run();
```

### 3. Configurar el envío de métricas a InfluxDB

Crea una clase `MetricsConfiguration.cs`:

```csharp
using App.Metrics;
using App.Metrics.Filters;
using App.Metrics.Formatters.InfluxDB;
using App.Metrics.Reporting.InfluxDB;
using InfluxDB.Client;

namespace TuProjeto.Configuration;

public static class MetricsConfiguration
{
    public static IServiceCollection AddMetricsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var influxDbSettings = configuration.GetSection("InfluxDB").Get<InfluxDbSettings>();

        // Configurar el cliente de InfluxDB 2.x
        services.AddSingleton<IInfluxDBClient>(sp =>
        {
            var clientOptions = new InfluxDBClientOptions.Builder()
                .Url(influxDbSettings.Url)
                .AuthenticateToken(influxDbSettings.Token.ToCharArray())
                .Org(influxDbSettings.Org)
                .Bucket(influxDbSettings.Bucket)
                .TimeOut(TimeSpan.FromSeconds(10))
                .Build();

            return new InfluxDBClient(clientOptions);
        });

        // Configurar App.Metrics
        var metrics = new MetricsBuilder()
            .Configuration.Configure(options =>
            {
                options.DefaultContextLabel = "GestMantIA.API";
                options.Enabled = true;
                options.ReportingEnabled = true;
            })
            .OutputMetrics.AsJson()
            .OutputMetrics.AsPrometheusPlainText()
            .Report.ToInfluxDb(options =>
            {
                options.InfluxDb.BaseUri = new Uri(influxDbSettings.Url);
                options.InfluxDb.Database = influxDbSettings.Bucket;
                options.InfluxDb.UserName = "api";
                options.InfluxDb.Password = influxDbSettings.Token;
                options.InfluxDb.CreateDataBaseIfNotExists = true;
                options.InfluxDb.RetentionPolicy = "autogen";
                
                options.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                options.HttpPolicy.FailuresBeforeBackoff = 5;
                options.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);

                options.MetricsOutputFormatter = new MetricsInfluxDbLineProtocolOutputFormatter();

                options.Filter = new MetricsFilter()
                    .WhereType(App.Metrics.MetricType.Timer)
                    .WhereType(App.Metrics.MetricType.Counter)
                    .WhereType(App.Metrics.MetricType.Meter)
                    .WhereType(App.Metrics.MetricType.Histogram)
                    .WhereType(App.Metrics.MetricType.Gauge)
                    .WhereType(App.Metrics.MetricType.Apdex);

                options.FlushInterval = TimeSpan.FromSeconds(15);
            })
            .Build();

        services.AddMetrics(metrics);
        services.AddMetricsTrackingMiddleware();
        services.AddMetricsEndpoints();

        return services;
    }
}

public class InfluxDbSettings
{
    public string Url { get; set; } = "http://localhost:8086";
    public string Token { get; set; } = string.Empty;
    public string Org { get; set; } = "gestmantia";
    public string Bucket { get; set; } = "gestmantia";
}
```

### 4. Configurar appsettings.json

```json
{
  "InfluxDB": {
    "Url": "http://localhost:8086",
    "Token": "my-super-secret-auth-token",
    "Org": "gestmantia",
    "Bucket": "gestmantia"
  }
}
```

## Configuración de Grafana

### 1. Configurar el origen de datos

Crea el archivo `docker/telemetry/grafana/provisioning/datasources/influxdb.yml`:

```yaml
apiVersion: 1

datasources:
  - name: InfluxDB
    type: influxdb
    access: proxy
    url: http://influxdb:8086
    jsonData:
      version: Flux
      organization: gestmantia
      defaultBucket: gestmantia
      tlsAuth: false
    secureJsonData:
      token: my-super-secret-auth-token
    editable: true
    isDefault: true
```

### 2. Configurar los dashboards

Crea el archivo `docker/telemetry/grafana/provisioning/dashboards/dashboard.yml`:

```yaml
apiVersion: 1

providers:
  - name: 'GestMantIA'
    orgId: 1
    folder: 'GestMantIA'
    type: file
    disableDeletion: false
    editable: true
    options:
      path: /etc/grafana/provisioning/dashboards
      foldersFromFilesStructure: true
```

## Monitoreo de la Aplicación

### Métricas disponibles

- **HTTP**: Tiempos de respuesta, códigos de estado, tasa de solicitudes
- **Sistema**: Uso de CPU, memoria, disco, red
- **Base de datos**: Consultas, tiempos de respuesta, conexiones
- **Negocio**: Métricas personalizadas específicas de tu aplicación

### Acceso a los dashboards

1. **Grafana**: http://localhost:3000
   - Usuario: admin
   - Contraseña: admin

2. **InfluxDB**: http://localhost:8086
   - Usuario: admin
   - Contraseña: admin123

## Solución de Problemas

### No se ven las métricas en Grafana

1. Verifica que InfluxDB esté recibiendo datos:
   ```bash
   docker logs telegraf
   ```

2. Verifica la conexión de Grafana a InfluxDB:
   - Ve a Configuration > Data Sources en Grafana
   - Prueba la conexión con InfluxDB

3. Revisa los logs de la aplicación:
   ```bash
   docker logs tu_aplicacion
   ```

### Las métricas de la aplicación no aparecen

1. Verifica que la aplicación esté configurada correctamente:
   - Revisa la configuración de InfluxDB en appsettings.json
   - Verifica que los endpoints de métricas estén accesibles

2. Verifica la red entre contenedores:
   ```bash
   docker network inspect gestmantia_telemetry-net
   ```

## Mejores Prácticas

1. **Etiquetado de métricas**: Usa etiquetas consistentes para facilitar el filtrado
2. **Retención de datos**: Configura políticas de retención adecuadas en InfluxDB
3. **Seguridad**: Usa tokens con los mínimos privilegios necesarios
4. **Monitoreo proactivo**: Configura alertas para métricas críticas
5. **Documentación**: Mantén actualizada la documentación de las métricas disponibles

## Recursos Adicionales

- [Documentación de App.Metrics](https://www.app-metrics.io/)
- [Documentación de InfluxDB](https://docs.influxdata.com/influxdb/)
- [Documentación de Grafana](https://grafana.com/docs/)
- [Documentación de Telegraf](https://docs.influxdata.com/telegraf/)

---

Esta guía proporciona una base sólida para implementar la telemetría en aplicaciones .NET. Ajusta las configuraciones según las necesidades específicas de tu proyecto.
