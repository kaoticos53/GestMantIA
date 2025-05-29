using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Core.Identity.Interfaces
{
    /// <summary>
    /// Servicio para registrar y consultar eventos de seguridad.
    /// </summary>
    public interface ISecurityLogger
    {
        /// <summary>
        /// Registra un evento de seguridad.
        /// </summary>
        /// <param name="userId">ID del usuario relacionado (opcional).</param>
        /// <param name="eventType">Tipo de evento.</param>
        /// <param name="description">Descripción del evento.</param>
        /// <param name="ipAddress">Dirección IP del origen.</param>
        /// <param name="userAgent">Agente de usuario.</param>
        /// <param name="additionalData">Datos adicionales en formato JSON.</param>
        /// <param name="succeeded">Indica si el evento fue exitoso.</param>
        /// <returns>El registro de seguridad creado.</returns>
        Task<SecurityLog> LogSecurityEventAsync(
            Guid? userId,
            string eventType,
            string description,
            string? ipAddress = null,
            string? userAgent = null,
            string? additionalData = null,
            bool succeeded = true);

        /// <summary>
        /// Obtiene el historial de eventos de seguridad para un usuario específico.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="page">Número de página (comenzando en 1).</param>
        /// <param name="pageSize">Tamaño de la página.</param>
        /// <returns>Lista paginada de eventos de seguridad.</returns>
        Task<(IEnumerable<SecurityLog> Logs, int TotalCount)> GetUserSecurityLogsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Obtiene todos los eventos de seguridad con filtros opcionales.
        /// </summary>
        /// <param name="startDate">Fecha de inicio para filtrar.</param>
        /// <param name="endDate">Fecha de fin para filtrar.</param>
        /// <param name="eventType">Tipo de evento para filtrar.</param>
        /// <param name="succeeded">Estado de éxito para filtrar (opcional).</param>
        /// <param name="page">Número de página (comenzando en 1).</param>
        /// <param name="pageSize">Tamaño de la página.</param>
        /// <returns>Lista paginada de eventos de seguridad.</returns>
        Task<(IEnumerable<SecurityLog> Logs, int TotalCount)> GetSecurityLogsAsync(
            DateTimeOffset? startDate = null,
            DateTimeOffset? endDate = null,
            string? eventType = null,
            bool? succeeded = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Detecta actividades sospechosas para un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="ipAddress">Dirección IP actual.</param>
        /// <param name="userAgent">Agente de usuario actual.</param>
        /// <returns>True si se detectó actividad sospechosa.</returns>
        Task<bool> DetectSuspiciousActivityAsync(
            Guid userId,
            string? ipAddress = null,
            string? userAgent = null);

        /// <summary>
        /// Verifica si un inicio de sesión es desde un dispositivo conocido.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="deviceId">Identificador del dispositivo.</param>
        /// <returns>True si el dispositivo es conocido.</returns>
        Task<bool> IsKnownDeviceAsync(Guid userId, string deviceId);

        /// <summary>
        /// Obtiene los dispositivos conocidos de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Una colección de objetos SecurityLog, donde cada uno representa el último inicio de sesión desde un dispositivo conocido.</returns>
        Task<List<SecurityLog>> GetKnownDevicesAsync(Guid userId);
    }
}
