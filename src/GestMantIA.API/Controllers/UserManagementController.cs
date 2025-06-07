using GestMantIA.Application.Interfaces; // Para IUserProfileService
using GestMantIA.Core.Identity.Interfaces; // Para IUserService
using GestMantIA.Core.Shared;
using GestMantIA.Shared.Identity.DTOs; // Para UserProfileDto
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
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<UserManagementController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserManagementController"/>
        /// </summary>
        public UserManagementController(
            IUserService userService,
            IUserProfileService userProfileService, // Añadido
            ILogger<UserManagementController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService)); // Añadido
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
        public async Task<ActionResult<UserResponseDTO>> GetUserById(Guid userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId); // Asume que IUserService también se actualizará
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
        /// Obtiene el perfil detallado de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>El perfil detallado del usuario.</returns>
        [HttpGet("{userId}/profile")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(Guid userId)
        {
            try
            {
                var userProfile = await _userProfileService.GetUserProfileAsync(userId);
                if (userProfile == null)
                {
                    _logger.LogWarning("No se encontró el perfil para el usuario con ID: {UserId}", userId);
                    return NotFound("Perfil de usuario no encontrado o usuario inexistente.");
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
                _logger.LogInformation("=== INICIO: GetAllUsers ===");
                _logger.LogInformation("Parámetros de la solicitud:");
                _logger.LogInformation("- Término de búsqueda: {SearchTerm}", searchTerm ?? "(ninguno)");
                _logger.LogInformation("- Página: {PageNumber}", pageNumber);
                _logger.LogInformation("- Tamaño de página: {PageSize}", pageSize);
                _logger.LogInformation("- Solo activos: {ActiveOnly}", activeOnly);
                
                _logger.LogInformation("Llamando a _userService.GetAllUsersAsync...");
                try 
                {
                    var result = await _userService.GetAllUsersAsync(
                        pageNumber, 
                        pageSize, 
                        searchTerm, 
                        activeOnly ?? true);
                    
                    _logger.LogInformation("Resultado obtenido exitosamente");
                    _logger.LogInformation("- Total de usuarios: {TotalCount}", result.TotalCount);
                    _logger.LogInformation("- Usuarios en la página: {ItemCount}", result.Items?.Count() ?? 0);
                    _logger.LogInformation("- Página actual: {PageNumber} de {TotalPages} (Tamaño: {PageSize})", 
                        result.PageNumber, 
                        (int)Math.Ceiling((double)result.TotalCount / result.PageSize),
                        result.PageSize);
                        
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener la lista de usuarios");
                    throw; // Relanza la excepción para que el middleware de manejo de errores la procese
                }
                finally
                {
                    _logger.LogInformation("=== FIN: GetAllUsers ===");
                }
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
                // Si CreateUserAsync tiene éxito, user no debería ser null según la implementación revisada.
                // La lógica de conflicto (usuario/email duplicado) o fallo de creación ahora lanza excepciones.
                return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, user);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Error al crear usuario: DTO nulo.");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al crear usuario: {ErrorMessage}", ex.Message);
                // Comprobar si el mensaje indica un conflicto para devolver 409, sino 400.
                if (ex.Message.Contains("ya existe") || ex.Message.Contains("is already taken"))
                {
                    return Conflict(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear el usuario.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado al crear el usuario.");
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
            Guid userId,
            [FromBody] UpdateUserDTO updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Datos de actualización no válidos para el usuario {UserId}", userId);
                    return BadRequest(ModelState);
                }

                if (userId != updateUserDto.Id) // updateUserDto.Id ya es Guid
                {
                    _logger.LogWarning("El ID de la ruta no coincide con el ID del usuario");
                    return BadRequest("El ID de la ruta no coincide con el ID del usuario");
                }

                var user = await _userService.UpdateUserAsync(userId, updateUserDto);
                // Si UpdateUserAsync tiene éxito, user no debería ser null.
                // La lógica de usuario no encontrado, conflicto o fallo de actualización ahora lanza excepciones.
                return Ok(user);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Error al actualizar usuario '{UserId}': DTO nulo.", userId);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex) // Captura la no coincidencia de IDs
            {
                _logger.LogWarning(ex, "Error al actualizar usuario '{UserId}': {ErrorMessage}", userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Error al actualizar usuario '{UserId}': Usuario no encontrado.", userId);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al actualizar usuario '{UserId}': {ErrorMessage}", userId, ex.Message);
                // Comprobar si el mensaje indica un conflicto para devolver 409, sino 400.
                if (ex.Message.Contains("ya está en uso") || ex.Message.Contains("is already taken"))
                {
                    return Conflict(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar el usuario con ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado al actualizar el usuario.");
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
            Guid userId,
            [FromQuery] bool hardDelete = false)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(userId, hardDelete); // Asume que IUserService también se actualizará
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
        /// Obtiene los roles de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Lista de nombres de roles del usuario.</returns>
        [HttpGet("{userId:guid}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(Guid userId)
        {
            try
            {
                var roles = await _userService.GetUserRolesAsync(userId);
                if (roles == null || !roles.Any())
                {
                    // El servicio devuelve Enumerable.Empty si no encuentra usuario o no tiene roles.
                    // Para ser consistentes con GetUserById, si el usuario no existe, devolvemos NotFound.
                    // Verificamos si el usuario existe primero.
                    var userCheck = await _userService.GetUserByIdAsync(userId);
                    if (userCheck == null)
                    {
                        _logger.LogWarning("Usuario con ID: {UserId} no encontrado al intentar obtener roles.", userId);
                        return NotFound($"Usuario con ID '{userId}' no encontrado.");
                    }
                    // Si el usuario existe pero no tiene roles, roles estará vacío, lo cual es OK.
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
