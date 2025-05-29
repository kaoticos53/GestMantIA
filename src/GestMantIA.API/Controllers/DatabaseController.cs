using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestMantIA.Infrastructure.Data;
using System.Threading.Tasks;

namespace GestMantIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class DatabaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DatabaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Verificar si la base de datos puede conectarse
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(500, new { message = "No se pudo conectar a la base de datos." });
                }

                // Obtener información de la migración actual
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                var currentMigration = await _context.Database.GetAppliedMigrationsAsync();

                return Ok(new 
                { 
                    message = "Conexión exitosa a la base de datos.",
                    database = _context.Database.GetDbConnection().Database,
                    dataSource = _context.Database.GetDbConnection().DataSource,
                    migrations = new 
                    { 
                        pending = pendingMigrations,
                        applied = appliedMigrations,
                        current = currentMigration
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al conectar a la base de datos: {ex.Message}" });
            }
        }
    }
}
