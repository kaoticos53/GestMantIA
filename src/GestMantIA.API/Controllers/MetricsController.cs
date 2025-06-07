using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Meter;
using App.Metrics.Timer;

namespace GestMantIA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly ILogger<MetricsController> _logger;
        private readonly IMetrics _metrics;
        private static readonly Random _random = new Random();

        private static readonly CounterOptions _apiRequestCounter = new CounterOptions
        {
            Name = "api_requests_total",
            Context = "GestMantIA.API",
            MeasurementUnit = Unit.Calls,
            Tags = new MetricTags("environment", "production")
        };

        private static readonly MeterOptions _apiRequestMeter = new MeterOptions
        {
            Name = "api_requests_rate",
            Context = "GestMantIA.API",
            MeasurementUnit = Unit.Calls,
            RateUnit = TimeUnit.Seconds
        };

        private static readonly TimerOptions _apiRequestTimer = new TimerOptions
        {
            Name = "api_request_duration_seconds",
            Context = "GestMantIA.API",
            MeasurementUnit = Unit.Requests,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Seconds
        };

        public MetricsController(ILogger<MetricsController> logger, IMetrics metrics)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestMetrics()
        {
            // Incrementar contador de solicitudes
            _metrics.Measure.Counter.Increment(_apiRequestCounter, "test_route");
            
            // Registrar tasa de solicitudes
            _metrics.Measure.Meter.Mark(_apiRequestMeter, "test_route");
            
            // Medir tiempo de ejecución
            using (_metrics.Measure.Timer.Time(_apiRequestTimer, "test_route"))
            {
                // Simular trabajo
                await Task.Delay(_random.Next(10, 100));
                
                _logger.LogInformation("Métricas de prueba generadas");
                return Ok(new { message = "Métricas de prueba generadas correctamente" });
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            _metrics.Measure.Counter.Increment(_apiRequestCounter, "health_check");
            _metrics.Measure.Meter.Mark(_apiRequestMeter, "health_check");
            
            using (_metrics.Measure.Timer.Time(_apiRequestTimer, "health_check"))
            {
                return Ok(new 
                { 
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    service = "GestMantIA.API"
                });
            }
        }
    }
}
