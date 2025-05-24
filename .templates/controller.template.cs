using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GestMantIA.Core.Application.Features.{{FeatureName}}.Commands;
using GestMantIA.Core.Application.Features.{{FeatureName}}.Queries;

namespace GestMantIA.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de {{FeatureNamePlural}}.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class {{ControllerName}}Controller : ControllerBase
    {
        private readonly ILogger<{{ControllerName}}Controller> _logger;
        private readonly IMediator _mediator;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="{{ControllerName}}Controller"/>
        /// </summary>
        /// <param name="logger">Logger para el controlador.</param>
        /// <param name="mediator">Mediador para enviar comandos y consultas.</param>
        public {{ControllerName}}Controller(
            ILogger<{{ControllerName}}Controller> logger,
            IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Obtiene todos los {{FeatureNamePlural}}.
        /// </summary>
        /// <returns>Lista de {{FeatureNamePlural}}.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<{{DtoName}}Dto>>> GetAll()
        {
            var query = new Get{{ControllerNamePlural}}Query();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un {{FeatureName}} por su identificador único.
        /// </summary>
        /// <param name="id">Identificador único del {{FeatureName}}.</param>
        /// <returns>El {{FeatureName}} solicitado.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<{{DtoName}}Dto>> GetById(Guid id)
        {
            var query = new Get{{ControllerName}}ByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
                return NotFound();
                
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo {{FeatureName}}.
        /// </summary>
        /// <param name="command">Datos para crear el {{FeatureName}}.</param>
        /// <returns>El {{FeatureName}} creado.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<{{DtoName}}Dto>> Create([FromBody] Create{{ControllerName}}Command command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Actualiza un {{FeatureName}} existente.
        /// </summary>
        /// <param name="id">Identificador único del {{FeatureName}}.</param>
        /// <param name="command">Datos actualizados del {{FeatureName}}.</param>
        /// <returns>Sin contenido.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Update{{ControllerName}}Command command)
        {
            if (id != command.Id)
                return BadRequest("El ID de la ruta no coincide con el ID del comando.");

            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Elimina un {{FeatureName}} existente.
        /// </summary>
        /// <param name="id">Identificador único del {{FeatureName}}.</param>
        /// <returns>Sin contenido.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new Delete{{ControllerName}}Command { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
