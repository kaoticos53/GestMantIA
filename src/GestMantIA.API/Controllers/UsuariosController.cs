using Microsoft.AspNetCore.Mvc;
using GestMantIA.Core.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace GestMantIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Proteger el controlador con autenticación
    public class UsuariosController : ControllerBase
    {
        private readonly IValidator<Usuario> _usuarioValidator;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(
            IValidator<Usuario> usuarioValidator,
            ILogger<UsuariosController> logger)
        {
            _usuarioValidator = usuarioValidator ?? throw new ArgumentNullException(nameof(usuarioValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(Usuario usuario)
        {
            try
            {
                // Validación manual del modelo
                var validationResult = await _usuarioValidator.ValidateAsync(usuario);
                
                if (!validationResult.IsValid)
                {
                    // Si la validación falla, devolver los errores
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    return BadRequest(ModelState);
                }

                // Aquí iría la lógica para guardar el usuario en la base de datos
                _logger.LogInformation("Usuario validado correctamente: {Usuario}", usuario.NombreUsuario);
                
                return Ok(new { 
                    mensaje = "Usuario creado correctamente", 
                    usuario = new { 
                        usuario.Id, 
                        usuario.NombreUsuario, 
                        usuario.Email,
                        usuario.Activo,
                        usuario.FechaCreacion
                    } 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario: {MensajeError}", ex.Message);
                return StatusCode(500, "Se produjo un error al procesar la solicitud");
            }
        }
    }
}
