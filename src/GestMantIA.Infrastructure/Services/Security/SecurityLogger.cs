using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestMantIA.Infrastructure.Services.Security
{
    /// <summary>
    /// Implementación del servicio de registro de seguridad.
    /// </summary>
    public class SecurityLogger : ISecurityLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SecurityLogger> _logger;

        public SecurityLogger(
            ApplicationDbContext context,
            ILogger<SecurityLogger> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SecurityLog> LogSecurityEventAsync(
            Guid? userId,
            string eventType,
            string description,
            string? ipAddress = null,
            string? userAgent = null,
            string? additionalData = null,
            bool succeeded = true)
        {
            try
            {
                var log = new SecurityLog
                {
                    UserId = userId,
                    EventType = eventType,
                    Description = description,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    AdditionalData = additionalData,
                    Succeeded = succeeded,
                    Timestamp = DateTimeOffset.UtcNow
                };

                _context.SecurityLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Evento de seguridad registrado: {EventType} para el usuario {UserId} - {Succeeded}",
                    eventType, userId.ToString() ?? "system", succeeded ? "Éxito" : "Fallo");

                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el evento de seguridad: {EventType}", eventType);
                throw new InvalidOperationException("No se pudo registrar el evento de seguridad.", ex);
            }
        }

        public async Task<(IEnumerable<SecurityLog> Logs, int TotalCount)> GetUserSecurityLogsAsync(
            Guid userId, int page = 1, int pageSize = 20)
        {
            try
            {
                if (userId == Guid.Empty) // Corrected check for Guid type
                    throw new ArgumentNullException(nameof(userId));

                var query = _context.SecurityLogs
                    .Where(log => log.UserId.HasValue && log.UserId.Value == userId) // Fixed comparison between Guid? and Guid
                    .OrderByDescending(log => log.Timestamp);

                var totalCount = await query.CountAsync();
                var logs = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (logs, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el historial de seguridad para el usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<(IEnumerable<SecurityLog> Logs, int TotalCount)> GetSecurityLogsAsync(
            DateTimeOffset? startDate = null,
            DateTimeOffset? endDate = null,
            string? eventType = null,
            bool? succeeded = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.SecurityLogs.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(log => log.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(log => log.Timestamp <= endDate.Value);


                if (!string.IsNullOrEmpty(eventType))
                    query = query.Where(log => log.EventType == eventType);

                if (succeeded.HasValue)
                    query = query.Where(log => log.Succeeded == succeeded.Value);

                var totalCount = await query.CountAsync();
                var logs = await query
                    .OrderByDescending(log => log.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (logs, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los registros de seguridad");
                throw;
            }
        }

        public async Task<bool> DetectSuspiciousActivityAsync(
            Guid userId, string? ipAddress = null, string? userAgent = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentNullException(nameof(userId));

            try
            {
                // Verificar si hay múltiples intentos fallidos recientes
                var recentFailedAttempts = await _context.SecurityLogs
                    .Where(log => log.UserId == userId &&
                                log.EventType == SecurityEventTypes.LoginFailed &&
                                log.Timestamp > DateTimeOffset.UtcNow.AddMinutes(30))
                    .CountAsync();

                if (recentFailedAttempts >= 3)
                {
                    await LogSecurityEventAsync(
                        userId,
                        SecurityEventTypes.SuspiciousActivity,
                        "Múltiples intentos de inicio de sesión fallidos",
                        ipAddress,
                        userAgent,
                        null,
                        false);
                    return true;
                }

                // Verificar si es un nuevo dispositivo
                if (!string.IsNullOrEmpty(ipAddress) && !string.IsNullOrEmpty(userAgent))
                {
                    var deviceId = GenerateDeviceId(ipAddress, userAgent);
                    var isKnownDevice = await IsKnownDeviceAsync(userId, deviceId);

                    if (!isKnownDevice)
                    {
                        await LogSecurityEventAsync(
                            userId,
                            SecurityEventTypes.NewDeviceLogin,
                            "Inicio de sesión desde un dispositivo no reconocido",
                            ipAddress,
                            userAgent,
                            JsonSerializer.Serialize(new { DeviceId = deviceId }));
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al detectar actividad sospechosa para el usuario {UserId}", userId);
                // En caso de error, asumimos que no hay actividad sospechosa para no bloquear al usuario
                return false;
            }
        }

        public Task<bool> IsKnownDeviceAsync(Guid userId, string deviceId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            try
            {
                // Buscar en los logs recientes para ver si este dispositivo ya ha sido usado
                return _context.SecurityLogs
                    .Where(log => log.UserId == userId &&
                                log.IpAddress != null &&
                                log.UserAgent != null &&
                                log.Timestamp > DateTimeOffset.UtcNow.AddDays(-30) &&
                                GenerateDeviceId(log.IpAddress, log.UserAgent) == deviceId)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar dispositivo conocido para el usuario {UserId}", userId);
                // En caso de error, asumimos que el dispositivo no es conocido
                return Task.FromResult(false);
            }
        }

        public async Task<List<SecurityLog>> GetKnownDevicesAsync(Guid userId)
        {
            try
            {
                var filteredLogs = await _context.SecurityLogs
                    .Where(log => log.UserId == userId &&
                                    log.EventType == SecurityEventTypes.LoginSucceeded && // Corregido
                                    !string.IsNullOrEmpty(log.IpAddress) && // Comprobación más robusta
                                    !string.IsNullOrEmpty(log.UserAgent) &&
                                    log.Timestamp > DateTimeOffset.UtcNow.AddDays(-90))
                    .OrderByDescending(log => log.Timestamp) // Ordenar antes de traer a memoria puede ser útil si solo necesitas los más recientes
                    .ToListAsync(); // Materializar la consulta a la BD aquí

                // Procesamiento en memoria
                var devices = filteredLogs
                    .GroupBy(log => GenerateDeviceId(log.IpAddress!, log.UserAgent!)) // IpAddress y UserAgent ya comprobados que no son nulos/vacíos
                    .Select(g => g.OrderByDescending(l => l.Timestamp).First()!) // Suprimir advertencia de nulabilidad aquí
                    .OrderByDescending(l => l.Timestamp) // Re-ordenar los dispositivos finales si es necesario
                    .ToList();

                return devices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los dispositivos conocidos para el usuario {UserId}", userId);
                return new List<SecurityLog>();
            }
        }

        private static string GenerateDeviceId(string ipAddress, string userAgent)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var input = $"{ipAddress}:{userAgent}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
