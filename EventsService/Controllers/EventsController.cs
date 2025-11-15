using EventsService.Api.DTOs;
using EventsService.Aplicacion.Commands.CrearEvento;
using EventsService.Aplicacion.Commands.EliminarEvento;
using EventsService.Aplicacion.Commands.Evento;
using EventsService.Aplicacion.Commands.ModificarEvento;
using EventsService.Aplicacion.Queries.ObtenerEvento;
using EventsService.Aplicacion.Queries.ObtenerTodosEventos;
using EventsService.Dominio.Excepciones;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventsService.Api.Controllers;

[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    public EventsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Crea un evento.</summary>
    /// <param name="req">Datos del evento a crear.</param>
    /// <remarks>
    /// Si hay reglas inválidas se lanzará <c>DomainException</c>.
    /// </remarks>
    /// <response code="201">Evento creado.</response>
    /// <response code="422">Reglas de dominio inválidas.</response>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest req, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateEventCommand(
            req.Nombre,
            req.CategoriaId,
            req.EscenarioId,
            req.Inicio,
            req.Fin,
            req.AforoMaximo,
            req.Tipo,
            req.Lugar,
            req.Descripcion
        ), ct);

        return CreatedAtAction(nameof(GetEventById), new { id }, new { id });
    }

    /// <summary>Obtiene un evento por su Id.</summary>
    /// <param name="id">Id del evento.</param>
    /// <response code="200">Evento encontrado.</response>
    /// <response code="404">No existe el evento.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEventByIdQuery(id), ct);

        if (result is null)
            return NotFound();  // ✅ este es el fix

        return Ok(result);
    }

    /// <summary>Lista todos los eventos.</summary>
    /// <response code="200">Listado de eventos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _mediator.Send(new GetAllEventsQuery(), ct);
        return Ok(list);
    }

    /// <summary>Modifica un evento.</summary>
    /// <param name="id">Id del evento.</param>
    /// <param name="req">Campos a actualizar.</param>
    /// <remarks>
    /// Lanza <c>NotFoundException</c> si el evento no existe y <c>DomainException</c> ante reglas inválidas.
    /// </remarks>
    /// <response code="204">Actualizado.</response>
    /// <response code="404">No existe.</response>
    /// <response code="422">Reglas de dominio inválidas.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest req, CancellationToken ct)
    {
        var ok = await _mediator.Send(new UpdateEventCommand(
            id,
            req.Nombre,
            req.CategoriaId,
            req.EscenarioId,
            req.Inicio,
            req.Fin,
            req.AforoMaximo,
            req.Tipo,
            req.Lugar,
            req.Descripcion
        ), ct);

        // Puente temporal si tu handler devuelve bool:
        if (!ok) throw new NotFoundException("Evento", id);

        return NoContent();
    }

    /// <summary>Elimina un evento.</summary>
    /// <param name="id">Id del evento.</param>
    /// <response code="204">Eliminado.</response>
    /// <response code="404">No existe.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _mediator.Send(new DeleteEventCommand(id), ct);

        // Puente temporal si tu handler devuelve bool:
        if (!ok) throw new NotFoundException("Evento", id);

        return NoContent();
    }


    /// <summary>
    /// Sube una imagen para un evento y guarda la URL en el evento.
    /// </summary>
    [HttpPost("{id:guid}/imagen")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubirImagen(
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("El archivo es inválido o está vacío.");

        using var stream = file.OpenReadStream();

        var url = await _mediator.Send(
            new SubirImagenEventoCommand(
                EventoId: id,
                FileStream: stream,
                FileName: file.FileName),
            ct);

        return Ok(new
        {
            EventoId = id,
            ImagenUrl = url
        });
    }

    /// <summary>
    /// Sube un folleto (PDF u otro archivo) para un evento y guarda la URL en el evento.
    /// </summary>
    [HttpPost("{id:guid}/folleto")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubirFolleto(
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("El archivo es inválido o está vacío.");

        using var stream = file.OpenReadStream();

        var url = await _mediator.Send(
            new SubirFolletoEventoCommand(
                EventoId: id,
                FileStream: stream,
                FileName: file.FileName),
            ct);

        return Ok(new
        {
            EventoId = id,
            FolletoUrl = url
        });
    }
}
