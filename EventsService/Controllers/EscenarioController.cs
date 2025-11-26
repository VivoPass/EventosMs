// Api/Controllers/EscenariosController.cs

using EventsService.Api.Contracs.Escenario;
using EventsService.Aplicacion.Commands.CrearEscenario;
using EventsService.Aplicacion.Commands.EliminarEscenario;
using EventsService.Aplicacion.Commands.ModificarEscenario;
using EventsService.Aplicacion.NewFolder;
using EventsService.Aplicacion.Queries.ObtenerEscenario;
using EventsService.Aplicacion.Queries.ObtenerEscenarios;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Infraestructura;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using log4net;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/events/escenarios")]
    [Produces("application/json")]
    public class EscenariosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILog _log;

        public EscenariosController(IMediator mediator, ILog log)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _log = log ?? throw new LoggerNullException();
        }

        /// <summary>Crea un Escenario.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> Create(
            [FromBody] EscenarioCreateRequest req,
            CancellationToken ct)
        {
            _log.Info($"[EscenariosController] POST /api/escenarios - Creando escenario '{req.Nombre}' en '{req.Ciudad}, {req.Pais}'.");

            var id = await _mediator.Send(new CreateEscenarioCommand(
                req.Nombre,
                req.Descripcion,
                req.Ubicacion,
                req.Ciudad,
                req.Estado,
                req.Pais
            ), ct);

            _log.Info($"[EscenariosController] Escenario creado con Id='{id}'.");
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        /// <summary>Actualiza un Escenario.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(
            [FromRoute] string id,
            [FromBody] EscenarioUpdateRequest req,
            CancellationToken ct)
        {
            _log.Info($"[EscenariosController] PUT /api/escenarios/{id} - Modificar escenario.");

            await _mediator.Send(new ModificarEscenarioCommand(
                id,
                req.Nombre,
                req.Descripcion,
                req.Ubicacion,
                req.Ciudad,
                req.Estado,
                req.Pais
            ), ct);

            _log.Info($"[EscenariosController] Escenario actualizado correctamente. Id='{id}'.");
            return NoContent();
        }

        /// <summary>Elimina (o borra) un Escenario.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            [FromRoute] string id,
            CancellationToken ct)
        {
            _log.Info($"[EscenariosController] DELETE /api/escenarios/{id} - Eliminar escenario.");

            await _mediator.Send(new EliminarEscenarioCommand(id), ct);

            _log.Info($"[EscenariosController] Escenario eliminado correctamente. Id='{id}'.");
            return NoContent();
        }

        /// <summary>Obtiene un Escenario por Id.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EscenarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EscenarioResponse>> GetById(
            [FromRoute] string id,
            CancellationToken ct)
        {
            _log.Info($"[EscenariosController] GET /api/escenarios/{id} - Obtener escenario por Id.");

            var dto = await _mediator.Send(new ObtenerEscenarioQuery(id), ct);
            if (dto is null)
            {
                _log.Warn($"[EscenariosController] Escenario no encontrado. Id='{id}'. Lanzando NotFoundException.");
                throw new NotFoundException("Escenario", id);
            }

            var resp = new EscenarioResponse(
                dto.Id,
                dto.Nombre,
                dto.Descripcion,
                dto.Ubicacion,
                dto.Ciudad,
                dto.Estado,
                dto.Pais,
                dto.CapacidadTotal,
                dto.Activo
            );

            _log.Debug($"[EscenariosController] Escenario encontrado. Id='{id}'.");
            return Ok(resp);
        }

        /// <summary>Busca/pagina Escenarios.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<EscenarioResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<EscenarioResponse>>> Search(
            [FromQuery] string? q,
            [FromQuery] string? ciudad,
            [FromQuery] bool? activo,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            _log.Info($"[EscenariosController] GET /api/escenarios - Search. q='{q}', ciudad='{ciudad}', activo='{activo}', page={page}, pageSize={pageSize}.");

            var result = await _mediator.Send(new ObtenerEscenariosQuery(q, ciudad, activo, page, pageSize), ct);

            var items = result.Items.Select(d => new EscenarioResponse(
                d.Id,
                d.Nombre,
                d.Descripcion,
                d.Ubicacion,
                d.Ciudad,
                d.Estado,
                d.Pais,
                d.CapacidadTotal,
                d.Activo
            )).ToList();

            _log.Debug($"[EscenariosController] Search retornó {items.Count} escenarios de un total de {result.Total}.");
            return Ok(new PagedResult<EscenarioResponse>(items, result.Total, result.Page, result.PageSize));
        }
    }
}
