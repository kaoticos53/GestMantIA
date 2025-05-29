using System;
using System.Threading.Tasks;
using GestMantIA.Shared.Identity.DTOs;
using GestMantIA.Shared.Identity.DTOs.Responses;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GestMantIA.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UsersController"/>
        /// </summary>
        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene el perfil de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>El perfil del usuario.</returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserResponseDTO>> GetUserProfile(string userId)
        {
            try
            {
                var userProfile = await _userService.GetUserProfileAsync(userId);
                if (userProfile == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return NotFound();
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario con ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Busca usuarios según los criterios especificados.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (opcional).</param>
        /// <param name="pageNumber">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Tamaño de página (por defecto 10).</param>
        /// <returns>Lista paginada de perfiles de usuario.</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<UserResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResult<UserResponseDTO>>> SearchUsers(
            [FromQuery] string? searchTerm = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _userService.SearchUsersAsync(searchTerm, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuarios con el término: {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la búsqueda.");
            }
        }

        /// <summary>
        /// Actualiza el perfil de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario a actualizar.</param>
        /// <param name="updateDto">Datos actualizados del perfil.</param>
        /// <returns>Respuesta sin contenido si la actualización fue exitosa.</returns>
        [HttpPut("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserProfile(
            string userId, 
            [FromBody] UpdateProfileDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Datos de actualización no válidos para el usuario {UserId}", userId);
                    return BadRequest(ModelState);
                }

                var success = await _userService.UpdateUserProfileAsync(userId, updateDto);
                if (!success)
                {
                    _logger.LogWarning("No se pudo actualizar el perfil del usuario con ID: {UserId}", userId);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perfil del usuario con ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al actualizar el perfil.");
            }
        }
    }
}
