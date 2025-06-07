using System;
using App.Metrics;
using App.Metrics.Filtering; // Corregido el espacio de nombres
using App.Metrics.Formatters.InfluxDB;
using InfluxDB.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using App.Metrics.Reporting.InfluxDB; // Añadido para ToInfluxDb
using App.Metrics.Formatters.Json; // Para JSON output
using App.Metrics.Formatters.Prometheus; // Para Prometheus output

namespace GestMantIA.API.Configuration
{
    public static class MetricsConfiguration
    {
        public static IServiceCollection AddMetricsConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configuración de InfluxDB
            var influxDbSettings = configuration.GetSection("InfluxDB");
            var url = influxDbSettings["Url"] ?? "http://localhost:8086";
            var token = influxDbSettings["Token"] ?? "my-super-secret-auth-token";
            var org = influxDbSettings["Org"] ?? "gestmantia";
            var bucket = influxDbSettings["Bucket"] ?? "gestmantia";

            // Configurar el cliente de InfluxDB 2.x
            services.AddSingleton<IInfluxDBClient>(sp =>
            {
                var clientOptions = new InfluxDBClientOptions.Builder()
                    .Url(url)
                    .AuthenticateToken(token.ToCharArray())
                    .Org(org)
                    .Bucket(bucket)
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
                    // Configuración básica para InfluxDB
                    options.InfluxDb.BaseUri = new Uri(url);
                    options.InfluxDb.Database = bucket;
                    options.InfluxDb.UserName = "api";      // Usuario dummy
                    options.InfluxDb.Password = token;        // Token como contraseña
                    options.InfluxDb.CreateDataBaseIfNotExists = true;
                    options.InfluxDb.RetentionPolicy = "autogen";
                    
                    // Configuración de reintentos
                    options.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                    options.HttpPolicy.FailuresBeforeBackoff = 5;
                    options.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);
                    
                    // Formato de salida
                    options.MetricsOutputFormatter = new MetricsInfluxDbLineProtocolOutputFormatter();
                    
                    // Filtro de métricas a incluir
                    options.Filter = new MetricsFilter()
                        .WhereType(App.Metrics.MetricType.Timer)
                        .WhereType(App.Metrics.MetricType.Counter)
                        .WhereType(App.Metrics.MetricType.Meter)
                        .WhereType(App.Metrics.MetricType.Histogram)
                        .WhereType(App.Metrics.MetricType.Gauge)
                        .WhereType(App.Metrics.MetricType.Apdex);
                        
                    // Frecuencia de envío de métricas
                    options.FlushInterval = TimeSpan.FromSeconds(15);
                })
                .Build();

            // Agregar servicios de métricas
            services.AddMetrics(metrics)
                .AddMetricsTrackingMiddleware(options =>
                {
                    options.ApdexTrackingEnabled = true;
                    options.ApdexTSeconds = 0.1;
                    options.OAuth2TrackingEnabled = true;
                })
                .AddMetricsEndpoints(options =>
                {
                    options.MetricsTextEndpointOutputFormatter = new App.Metrics.Formatters.Prometheus.MetricsPrometheusTextOutputFormatter();
                    options.MetricsEndpointOutputFormatter = new App.Metrics.Formatters.Json.MetricsJsonOutputFormatter();
                });

            return services;
        }
    }
}
