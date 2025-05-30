using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Shared.Identity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestMantIA.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de roles y permisos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Solo administradores pueden gestionar roles
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="RolesController"/>
        /// </summary>
        public RolesController(
            IRoleService roleService,
            ILogger<RolesController> logger)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene todos los roles disponibles.
        /// </summary>
        /// <returns>Lista de roles.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RoleDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetAllRoles()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los roles");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Obtiene un rol por su ID.
        /// </summary>
        /// <param name="id">ID del rol.</param>
        /// <returns>El rol si existe.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RoleDTO>> GetRoleById(string id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el rol con ID: {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Crea un nuevo rol.
        /// </summary>
        /// <param name="roleDto">Datos del rol a crear.</param>
        /// <returns>El rol creado.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RoleDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RoleDTO>> CreateRole([FromBody] RoleDTO roleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _roleService.CreateRoleAsync(roleDto);
                if (!result.Success)
                {
                    return BadRequest(new { Message = result.Message, Errors = result.Errors });
                }

                var createdRole = await _roleService.GetRoleByIdAsync(roleDto.Id);
                if (createdRole == null)
                {
                    _logger.LogError("No se pudo encontrar el rol recién creado con ID: {RoleId}", roleDto.Id);
                    return Problem("No se pudo recuperar el rol después de la creación.", statusCode: StatusCodes.Status500InternalServerError);
                }
                return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el rol: {RoleName}", roleDto.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Actualiza un rol existente.
        /// </summary>
        /// <param name="id">ID del rol a actualizar.</param>
        /// <param name="roleDto">Datos actualizados del rol.</param>
        /// <returns>Respuesta sin contenido si la actualización fue exitosa.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleDTO roleDto)
        {
            if (id != roleDto.Id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del rol.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _roleService.UpdateRoleAsync(roleDto);
                if (!result.Success)
                {
                    if (result.Message.Contains("no existe") || result.Message.Contains("no encontrado"))
                    {
                        return NotFound(result.Message);
                    }
                    return BadRequest(new { Message = result.Message, Errors = result.Errors });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el rol con ID: {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Elimina un rol por su ID.
        /// </summary>
        /// <param name="id">ID del rol a eliminar.</param>
        /// <returns>Respuesta sin contenido si la eliminación fue exitosa.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(id);
                if (!result.Success)
                {
                    if (result.Message.Contains("no existe") || result.Message.Contains("no encontrado"))
                    {
                        return NotFound(result.Message);
                    }
                    return BadRequest(new { Message = result.Message, Errors = result.Errors });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el rol con ID: {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Asigna un rol a un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="roleName">Nombre del rol a asignar.</param>
        /// <returns>Respuesta sin contenido si la operación fue exitosa.</returns>
        [HttpPost("users/{userId}/roles/{roleName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddUserToRole(string userId, string roleName)
        {
            try
            {
                var result = await _roleService.AddUserToRoleAsync(userId, roleName);
                if (!result.Success)
                {
                    return BadRequest(new { Message = result.Message, Errors = result.Errors });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar el rol '{RoleName}' al usuario {UserId}", roleName, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Elimina un rol de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="roleName">Nombre del rol a eliminar.</param>
        /// <returns>Respuesta sin contenido si la operación fue exitosa.</returns>
        [HttpDelete("users/{userId}/roles/{roleName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string roleName)
        {
            try
            {
                var result = await _roleService.RemoveUserFromRoleAsync(userId, roleName);
                if (!result.Success)
                {
                    return BadRequest(new { Message = result.Message, Errors = result.Errors });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el rol '{RoleName}' del usuario {UserId}", roleName, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Obtiene los roles de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Lista de roles del usuario.</returns>
        [HttpGet("users/{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string userId)
        {
            try
            {
                var roles = await _roleService.GetUserRolesAsync(userId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los roles del usuario {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Obtiene los usuarios que tienen un rol específico.
        /// </summary>
        /// <param name="roleName">Nombre del rol.</param>
        /// <returns>Lista de usuarios con el rol especificado.</returns>
        [HttpGet("{roleName}/users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IList<ApplicationUser>>> GetUsersInRole(string roleName)
        {
            try
            {
                var users = await _roleService.GetUsersInRoleAsync(roleName);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los usuarios con el rol '{RoleName}'", roleName);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }
    }
}
