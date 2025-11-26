using MediatR;
using Microsoft.AspNetCore.Mvc;

using EventsService.Aplicacion.Commands.Asiento.CrearAsiento;
using EventsService.Aplicacion.Commands.Asiento.ActualizarAsiento;
using EventsService.Aplicacion.Commands.Asiento.EliminarAsiento;

using EventsService.Aplicacion.Queries.Asiento.ObtenerAsiento;
using EventsService.Aplicacion.Queries.Asiento.ListarAsientos;

using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Infraestructura;
using log4net;

namespace EventsService.Api.Controllers
{
    [ApiController]
    [Route("api/events/{eventId:guid}/zonas/{zonaId:guid}/asientos")]
    [Produces("application/json")]
    public class AsientosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILog _log;

        public AsientosController(IMediator mediator, ILog log)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _log = log ?? throw new LoggerNullException();
        }

        /// <summary>Crea un asiento en la zona indicada del evento.</summary>
        /// <param name="eventId">Id del evento</param>
        /// <param name="zonaId">Id de la zona</param>
        /// <param name="dto">Datos del asiento a crear</param>
        /// <remarks>
        /// Lanza <c>DomainException</c> cuando los datos son inválidos y <c>NotFoundException</c> si la zona no existe.
        /// </remarks>
        /// <response code="201">Asiento creado</response>
        /// <response code="404">Evento/Zona no existe</response>
        /// <response code="422">Validación de dominio fallida</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Crear(
            Guid eventId,
            Guid zonaId,
            [FromBody] CrearAsientoDto dto,
            CancellationToken ct)
        {
            _log.Info($"[AsientosController] POST - Crear asiento. EventId='{eventId}', ZonaId='{zonaId}', Label='{dto.Label}'.");

            var result = await _mediator.Send(new CrearAsientoCommand(
                eventId,
                zonaId,
                dto.FilaIndex,
                dto.ColIndex,
                dto.Label,
                dto.Estado,
                dto.Meta
            ), ct);

            _log.Info($"[AsientosController] Asiento creado. AsientoId='{result.AsientoId}', EventId='{eventId}', ZonaId='{zonaId}'.");
            return CreatedAtAction(nameof(ObtenerPorId),
                new { eventId, zonaId, asientoId = result.AsientoId },
                new { id = result.AsientoId });
        }

        /// <summary>Obtiene un asiento por Id dentro de la zona.</summary>
        /// <param name="eventId">Id del evento</param>
        /// <param name="zonaId">Id de la zona</param>
        /// <param name="asientoId">Id del asiento</param>
        /// <response code="200">Asiento encontrado</response>
        /// <response code="404">No existe evento/zona/asiento</response>
        [HttpGet("{asientoId:guid}")]
        [ProducesResponseType(typeof(AsientoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(
            Guid eventId,
            Guid zonaId,
            Guid asientoId,
            CancellationToken ct)
        {
            _log.Info($"[AsientosController] GET - Obtener asiento. EventId='{eventId}', ZonaId='{zonaId}', AsientoId='{asientoId}'.");

            var dto = await _mediator.Send(new ObtenerAsientoQuery(eventId, zonaId, asientoId), ct);

            if (dto is null)
            {
                _log.Warn($"[AsientosController] Asiento no encontrado. EventId='{eventId}', ZonaId='{zonaId}', AsientoId='{asientoId}'.");
                throw new NotFoundException("Asiento", asientoId);
            }

            _log.Debug($"[AsientosController] Asiento encontrado. AsientoId='{asientoId}'.");
            return Ok(dto);
        }

        /// <summary>Lista los asientos de una zona.</summary>
        /// <param name="eventId">Id del evento</param>
        /// <param name="zonaId">Id de la zona</param>
        /// <response code="200">Listado de asientos</response>
        /// <response code="404">Zona no existe</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<AsientoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Listar(
            Guid eventId,
            Guid zonaId,
            CancellationToken ct)
        {
            _log.Info($"[AsientosController] GET - Listar asientos. EventId='{eventId}', ZonaId='{zonaId}'.");

            var result = await _mediator.Send(new ListarAsientosQuery(eventId, zonaId), ct);

            _log.Debug($"[AsientosController] Se retornan {result.Count} asientos. EventId='{eventId}', ZonaId='{zonaId}'.");
            return Ok(result);
        }

        /// <summary>Actualiza datos de un asiento.</summary>
        /// <param name="eventId">Id del evento</param>
        /// <param name="zonaId">Id de la zona</param>
        /// <param name="asientoId">Id del asiento</param>
        /// <param name="dto">Label/Estado/Meta</param>
        /// <remarks>
        /// Lanza <c>NotFoundException</c> si no existe y <c>DomainException</c> ante reglas inválidas.
        /// </remarks>
        /// <response code="204">Actualizado</response>
        /// <response code="404">No existe</response>
        /// <response code="422">Reglas de dominio inválidas</response>
        [HttpPut("{asientoId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Actualizar(
            Guid eventId,
            Guid zonaId,
            Guid asientoId,
            [FromBody] ActualizarAsientoDto dto,
            CancellationToken ct)
        {
            _log.Info($"[AsientosController] PUT - Actualizar asiento. EventId='{eventId}', ZonaId='{zonaId}', AsientoId='{asientoId}'.");

            await _mediator.Send(new ActualizarAsientoCommand(
                eventId,
                zonaId,
                asientoId,
                dto.Label,
                dto.Estado,
                dto.Meta
            ), ct);

            _log.Info($"[AsientosController] Asiento actualizado correctamente. AsientoId='{asientoId}'.");
            return NoContent();
        }

        /// <summary>Elimina un asiento.</summary>
        /// <param name="eventId">Id del evento</param>
        /// <param name="zonaId">Id de la zona</param>
        /// <param name="asientoId">Id del asiento</param>
        /// <response code="204">Eliminado</response>
        /// <response code="404">No existe</response>
        [HttpDelete("{asientoId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Eliminar(
            Guid eventId,
            Guid zonaId,
            Guid asientoId,
            CancellationToken ct)
        {
            _log.Info($"[AsientosController] DELETE - Eliminar asiento. EventId='{eventId}', ZonaId='{zonaId}', AsientoId='{asientoId}'.");

            await _mediator.Send(new EliminarAsientoCommand(eventId, zonaId, asientoId), ct);

            _log.Info($"[AsientosController] Asiento eliminado correctamente. AsientoId='{asientoId}'.");
            return NoContent();
        }
    }
}
