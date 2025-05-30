using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Core.Shared;
using GestMantIA.Shared.Identity.DTOs.Requests;
using GestMantIA.Shared.Identity.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestMantIA.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión avanzada de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Solo administradores pueden gestionar usuarios
    public class UserManagementController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserManagementController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserManagementController"/>
        /// </summary>
        public UserManagementController(
            IUserService userService,
            ILogger<UserManagementController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>El usuario solicitado.</returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserResponseDTO>> GetUserById(string userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de usuarios.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (opcional).</param>
        /// <param name="activeOnly">Indica si solo se deben devolver usuarios activos (opcional).</param>
        /// <param name="pageNumber">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Tamaño de página (por defecto 10, máximo 100).</param>
        /// <returns>Lista paginada de usuarios.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<UserResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<UserResponseDTO>>> GetAllUsers(
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userService.GetAllUsersAsync(pageNumber, pageSize, searchTerm, activeOnly ?? true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al obtener la lista de usuarios.");
            }
        }

        /// <summary>
        /// Crea un nuevo usuario.
        /// </summary>
        /// <param name="createUserDto">Datos del usuario a crear.</param>
        /// <returns>El usuario creado.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponseDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponseDTO>> CreateUser([FromBody] CreateUserDTO createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Datos de usuario no válidos");
                    return BadRequest(ModelState);
                }

                var user = await _userService.CreateUserAsync(createUserDto);
                if (user == null)
                {
                    _logger.LogWarning("No se pudo crear el usuario. El nombre de usuario o correo electrónico ya está en uso.");
                    return Conflict("El nombre de usuario o correo electrónico ya está en uso.");
                }

                return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al crear el usuario.");
            }
        }

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        /// <param name="userId">ID del usuario a actualizar.</param>
        /// <param name="updateUserDto">Datos actualizados del usuario.</param>
        /// <returns>El usuario actualizado.</returns>
        [HttpPut("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponseDTO>> UpdateUser(
            string userId,
            [FromBody] UpdateUserDTO updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Datos de actualización no válidos para el usuario {UserId}", userId);
                    return BadRequest(ModelState);
                }

                if (userId != updateUserDto.Id)
                {
                    _logger.LogWarning("El ID de la ruta no coincide con el ID del usuario");
                    return BadRequest("El ID de la ruta no coincide con el ID del usuario");
                }

                var user = await _userService.UpdateUserAsync(userId, updateUserDto);
                if (user == null)
                {
                    _logger.LogWarning("No se pudo actualizar el usuario con ID: {UserId}", userId);
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al actualizar el usuario.");
            }
        }

        /// <summary>
        /// Elimina un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar.</param>
        /// <param name="hardDelete">Indica si se debe eliminar físicamente el usuario (false por defecto, eliminación lógica).</param>
        /// <returns>Respuesta sin contenido si la eliminación fue exitosa.</returns>
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(
            string userId,
            [FromQuery] bool hardDelete = false)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(userId, hardDelete);
                if (!success)
                {
                    _logger.LogWarning("No se pudo eliminar el usuario con ID: {UserId}", userId);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario con ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al eliminar el usuario.");
            }
        }

        /// <summary>
        /// Asigna roles a un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="roleNames">Nombres de los roles a asignar.</param>
        /// <returns>Respuesta sin contenido si la operación fue exitosa.</returns>
        [HttpPost("{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRoles(
            string userId,
            [FromBody] IEnumerable<string> roleNames)
        {
            try
            {
                if (roleNames == null || !roleNames.Any())
                {
                    _logger.LogWarning("No se proporcionaron roles para asignar al usuario {UserId}", userId);
                    return BadRequest("Debe proporcionar al menos un rol para asignar.");
                }

                var success = await _userService.AssignRolesToUserAsync(userId, roleNames);
                if (!success)
                {
                    _logger.LogWarning("No se pudo asignar los roles al usuario con ID: {UserId}", userId);
                    return NotFound();
                }


                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar roles al usuario con ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al asignar los roles al usuario.");
            }
        }

        /// <summary>
        /// Elimina roles de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="roleNames">Nombres de los roles a eliminar.</param>
        /// <returns>Respuesta sin contenido si la operación fue exitosa.</returns>
        [HttpDelete("{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRoles(
            string userId,
            [FromBody] IEnumerable<string> roleNames)
        {
            try
            {
                if (roleNames == null || !roleNames.Any())
                {
                    _logger.LogWarning("No se proporcionaron roles para eliminar del usuario {UserId}", userId);
                    return BadRequest("Debe proporcionar al menos un rol para eliminar.");
                }

                var success = await _userService.RemoveRolesFromUserAsync(userId, roleNames);
                if (!success)
                {
                    _logger.LogWarning("No se pudo eliminar los roles del usuario con ID: {UserId}", userId);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar roles del usuario con ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al eliminar los roles del usuario.");
            }
        }

        /// <summary>
        /// Obtiene los roles de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Lista de nombres de roles del usuario.</returns>
        [HttpGet("{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string userId)
        {
            try
            {
                var roles = await _userService.GetUserRolesAsync(userId);
                if (roles == null)
                {
                    _logger.LogWarning("No se encontró el usuario con ID: {UserId}", userId);
                    return NotFound();
                }

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los roles del usuario con ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al obtener los roles del usuario.");
            }
        }
    }
}
