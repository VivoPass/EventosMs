using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using EventsService.Aplicacion.Commands.Zonas.CrearZonaEvento;
using EventsService.Aplicacion.Commands.Zonas.EliminarZonaEvento;
using EventsService.Aplicacion.Commands.Zonas.ModificarZonaEvento;
using EventsService.Aplicacion.Queries.Zona.ListarZonasEvento;
using EventsService.Aplicacion.Queries.Zona.ObtenerZonaEvento;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Infraestructura;
using log4net;

namespace EventsService.Api.Controllers
{
    [ApiController]
    [Route("api/events/{eventId:guid}/zonas")]
    [Produces("application/json")]
    public class ZonasEventoController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILog _log;

        public ZonasEventoController(IMediator mediator, ILog log)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _log = log ?? throw new LoggerNullException();
        }

        /// <summary>Crea una zona dentro de un evento y, si aplica, genera asientos.</summary>
        /// <param name="eventId">Id del evento.</param>
        /// <param name="body">Payload de creación de zona.</param>
        /// <response code="201">Zona creada.</response>
        /// <response code="422">Reglas de dominio inválidas.</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CrearZona(
            Guid eventId,
            [FromBody] CreateZonaEventoCommand body,
            CancellationToken ct)
        {
            _log.Info($"[ZonasEventoController] POST - Crear zona. EventId='{eventId}', Nombre='{body.Nombre}', Tipo='{body.Tipo}'.");

            // Enforce route id
            body.EventId = eventId;

            var zonaId = await _mediator.Send(body, ct);

            _log.Info($"[ZonasEventoController] Zona creada correctamente. EventId='{eventId}', ZonaId='{zonaId}'.");
            return CreatedAtAction(nameof(ObtenerZona), new { eventId, zonaId }, new { zonaId });
        }

        /// <summary>Obtiene una zona específica del evento.</summary>
        /// <param name="eventId">Id del evento.</param>
        /// <param name="zonaId">Id de la zona.</param>
        /// <param name="includeSeats">Incluye asientos en la respuesta.</param>
        /// <response code="200">Zona encontrada.</response>
        /// <response code="404">No existe la zona o el evento.</response>
        [HttpGet("{zonaId:guid}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerZona(
            Guid eventId,
            Guid zonaId,
            [FromQuery] bool includeSeats = false,
            CancellationToken ct = default)
        {
            _log.Info($"[ZonasEventoController] GET - Obtener zona. EventId='{eventId}', ZonaId='{zonaId}', IncludeSeats={includeSeats}.");

            var result = await _mediator.Send(new ObtenerZonaEventoQuery
            {
                EventId = eventId,
                ZonaId = zonaId,
                IncludeSeats = includeSeats
            }, ct);

            if (result is null)
            {
                _log.Warn($"[ZonasEventoController] Zona no encontrada. EventId='{eventId}', ZonaId='{zonaId}'. Lanzando NotFoundException.");
                throw new NotFoundException("ZonaEvento", zonaId);
            }

            _log.Debug($"[ZonasEventoController] Zona encontrada. EventId='{eventId}', ZonaId='{zonaId}'.");
            return Ok(result);
        }

        /// <summary>Lista todas las zonas del evento.</summary>
        /// <param name="eventId">Id del evento.</param>
        /// <param name="tipo">Filtro por tipo.</param>
        /// <param name="estado">Filtro por estado.</param>
        /// <param name="search">Búsqueda textual.</param>
        /// <param name="includeSeats">Incluye asientos en cada zona.</param>
        /// <response code="200">Listado de zonas.</response>
        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarZonas(
            Guid eventId,
            [FromQuery] string? tipo,
            [FromQuery] string? estado,
            [FromQuery] string? search,
            [FromQuery] bool includeSeats = false,
            CancellationToken ct = default)
        {
            _log.Info($"[ZonasEventoController] GET - Listar zonas. EventId='{eventId}', Tipo='{tipo}', Estado='{estado}', Search='{search}', IncludeSeats={includeSeats}.");

            var result = await _mediator.Send(new ListarZonasEventoQuery
            {
                EventId = eventId,
                Tipo = tipo,
                Estado = estado,
                Search = search,
                IncludeSeats = includeSeats
            }, ct);

            _log.Debug($"[ZonasEventoController] ListarZonas retornó {result?.Count ?? 0} zonas. EventId='{eventId}'.");
            return Ok(result);
        }

        /// <summary>Modifica parcialmente una zona.</summary>
        /// <param name="eventId">Id del evento.</param>
        /// <param name="zonaId">Id de la zona.</param>
        /// <param name="body">Campos a modificar.</param>
        /// <response code="204">Actualizada.</response>
        /// <response code="404">No existe.</response>
        /// <response code="422">Reglas de dominio inválidas.</response>
        [HttpPatch("{zonaId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ModificarZona(
            Guid eventId,
            Guid zonaId,
            [FromBody] ModificarZonaEventoCommnand body, // respetamos el typo del tipo si ya existe así
            CancellationToken ct)
        {
            _log.Info($"[ZonasEventoController] PATCH - Modificar zona. EventId='{eventId}', ZonaId='{zonaId}'.");

            body.EventId = eventId;
            body.ZonaId = zonaId;

            var ok = await _mediator.Send(body, ct);

            if (!ok)
            {
                _log.Warn($"[ZonasEventoController] ModificarZona: Zona no encontrada. EventId='{eventId}', ZonaId='{zonaId}'.");
                throw new NotFoundException("ZonaEvento", zonaId);
            }

            _log.Info($"[ZonasEventoController] Zona modificada correctamente. EventId='{eventId}', ZonaId='{zonaId}'.");
            return NoContent();
        }

        /// <summary>Elimina una zona del evento.</summary>
        /// <param name="eventId">Id del evento.</param>
        /// <param name="zonaId">Id de la zona.</param>
        /// <response code="204">Eliminada.</response>
        /// <response code="404">No existe.</response>
        [HttpDelete("{zonaId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarZona(
            Guid eventId,
            Guid zonaId,
            CancellationToken ct)
        {
            _log.Info($"[ZonasEventoController] DELETE - Eliminar zona. EventId='{eventId}', ZonaId='{zonaId}'.");

            var ok = await _mediator.Send(new EliminarZonaEventoCommand
            {
                EventId = eventId,
                ZonaId = zonaId
            }, ct);

            if (!ok)
            {
                _log.Warn($"[ZonasEventoController] EliminarZona: Zona no encontrada. EventId='{eventId}', ZonaId='{zonaId}'.");
                throw new NotFoundException("ZonaEvento", zonaId);
            }

            _log.Info($"[ZonasEventoController] Zona eliminada correctamente. EventId='{eventId}', ZonaId='{zonaId}'.");
            return NoContent();
        }
    }
}
