using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GestMantIA.Core.Configuration;
using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Infrastructure.Data;

public interface IDatabaseInitializer
{
    Task InitializeDatabaseAsync(CancellationToken cancellationToken = default);
    Task SeedDataAsync(CancellationToken cancellationToken = default);
}

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly SeedDataSettings _seedDataSettings;

    public DatabaseInitializer(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration,
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _seedDataSettings = configuration.GetSection("SeedData").Get<SeedDataSettings>() ?? new();
    }

    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_context.Database.IsNpgsql())
            {
                await _context.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Migraciones aplicadas correctamente.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al inicializar la base de datos");
            throw;
        }
    }

    public async Task SeedDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedRegularUserAsync();
            
            if (_seedDataSettings.SampleData.Enable)
            {
                await SeedSampleDataAsync();
            }
            
            _logger.LogInformation("Datos iniciales insertados correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al insertar los datos iniciales");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        var adminRole = new ApplicationRole("Administrator")
        {
            Description = "Administrador del sistema con acceso completo"
        };

        var userRole = new ApplicationRole("User")
        {
            Description = "Usuario estándar con permisos limitados"
        };

        await CreateRoleIfNotExistsAsync(adminRole);
        await CreateRoleIfNotExistsAsync(userRole);
    }

    private async Task CreateRoleIfNotExistsAsync(ApplicationRole role)
    {
        if (!await _roleManager.RoleExistsAsync(role.Name!))
        {
            await _roleManager.CreateAsync(role);
            _logger.LogInformation("Rol {RoleName} creado correctamente", role.Name);
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var adminUser = await _userManager.FindByEmailAsync(_seedDataSettings.AdminUser.Email);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = _seedDataSettings.AdminUser.UserName,
                Email = _seedDataSettings.AdminUser.Email,
                FirstName = _seedDataSettings.AdminUser.FirstName,
                LastName = _seedDataSettings.AdminUser.LastName,
                PhoneNumber = _seedDataSettings.AdminUser.PhoneNumber,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(adminUser, _seedDataSettings.AdminUser.Password);
            
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Administrator");
                _logger.LogInformation("Usuario administrador creado correctamente: {Email}", adminUser.Email);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Error al crear el usuario administrador: {Errors}", errors);
            }
        }
    }

    private async Task SeedRegularUserAsync()
    {
        var regularUser = await _userManager.FindByEmailAsync(_seedDataSettings.RegularUser.Email);
        
        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = _seedDataSettings.RegularUser.UserName,
                Email = _seedDataSettings.RegularUser.Email,
                FirstName = _seedDataSettings.RegularUser.FirstName,
                LastName = _seedDataSettings.RegularUser.LastName,
                PhoneNumber = _seedDataSettings.RegularUser.PhoneNumber,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(regularUser, _seedDataSettings.RegularUser.Password);
            
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(regularUser, "User");
                _logger.LogInformation("Usuario regular creado correctamente: {Email}", regularUser.Email);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Error al crear el usuario regular: {Errors}", errors);
            }
        }
    }

    private Task SeedSampleDataAsync()
    {
        _logger.LogInformation("Iniciando la inserción de datos de ejemplo...");
        
        // Aquí puedes agregar la lógica para insertar datos de ejemplo adicionales
        // como clientes, órdenes de trabajo, etc.
        
        _logger.LogInformation("Datos de ejemplo insertados correctamente");
        return Task.CompletedTask;
    }
}
