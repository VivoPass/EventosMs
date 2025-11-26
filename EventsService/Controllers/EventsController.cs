using EventsService.Api.Contracs;
using EventsService.Api.DTOs;
using EventsService.Aplicacion.Commands.CrearEvento;
using EventsService.Aplicacion.Commands.EliminarEvento;
using EventsService.Aplicacion.Commands.Evento;
using EventsService.Aplicacion.Commands.ModificarEvento;
using EventsService.Aplicacion.Queries.ObtenerEvento;
using EventsService.Aplicacion.Queries.ObtenerTodosEventos;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Infraestructura;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using log4net;

namespace EventsService.Api.Controllers;

[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILog _log;

    public EventsController(
        IMediator mediator,
        IHttpClientFactory httpClientFactory,
        ILog log)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _log = log ?? throw new LoggerNullException();
    }

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
        _log.Info($"[EventsController] POST /api/events - Creando evento '{req.Nombre}' para OrganizadorId='{req.OrganizadorId}'.");

        var id = await _mediator.Send(new CreateEventCommand(
            req.Nombre,
            req.CategoriaId,
            req.EscenarioId,
            req.Inicio,
            req.Fin,
            req.AforoMaximo,
            req.Tipo,
            req.Lugar,
            req.Descripcion,
            req.OrganizadorId
        ), ct);

        // Publicar actividad en MS de Usuarios
        var client = _httpClientFactory.CreateClient("UsuariosClient");
        var activityBody = new PublishActivityRequest
        {
            idUsuario = req.OrganizadorId.ToString(),
            accion = $"Evento '{req.Nombre}' creado."
        };
        const string endpoint = "/api/Usuarios/publishActivity";

        var httpResponse = await client.PostAsJsonAsync(endpoint, activityBody, ct);
        if (!httpResponse.IsSuccessStatusCode)
        {
            _log.Warn($"[EventsController] Falló la publicación de actividad para usuario '{req.OrganizadorId}'. " +
                      $"Status={(int)httpResponse.StatusCode} ({httpResponse.StatusCode}).");
        }
        else
        {
            _log.Debug($"[EventsController] Actividad publicada correctamente para usuario '{req.OrganizadorId}'.");
        }

        _log.Info($"[EventsController] Evento creado con Id='{id}'.");
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
        _log.Info($"[EventsController] GET /api/events/{id} - Obtener evento por Id.");

        var result = await _mediator.Send(new GetEventByIdQuery(id), ct);

        if (result is null)
        {
            _log.Warn($"[EventsController] Evento no encontrado. Id='{id}'.");
            return NotFound();
        }

        _log.Debug($"[EventsController] Evento encontrado. Id='{id}'.");
        return Ok(result);
    }

    /// <summary>Lista todos los eventos.</summary>
    /// <response code="200">Listado de eventos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        _log.Info("[EventsController] GET /api/events - Listar todos los eventos.");

        var list = await _mediator.Send(new GetAllEventsQuery(), ct);

        _log.Debug($"[EventsController] Se retornan {list.Count} eventos.");
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
        _log.Info($"[EventsController] PUT /api/events/{id} - Modificar evento.");

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

        if (!ok)
        {
            _log.Warn($"[EventsController] No se encontró evento para actualizar. Id='{id}'. Lanzando NotFoundException.");
            throw new NotFoundException("Evento", id);
        }

        // Obtener datos del evento para el mensaje de actividad
        var result = await _mediator.Send(new GetEventByIdQuery(id), ct);

        if (result is null)
        {
            // Esto sería raro, pero lo defendemos por si acaso
            _log.Warn($"[EventsController] Evento actualizado pero no se pudo recuperar para actividad. Id='{id}'.");
        }
        else
        {
            var client = _httpClientFactory.CreateClient("UsuariosClient");
            var activityBody = new PublishActivityRequest
            {
                idUsuario = result.OrganizadorId.ToString(),
                accion = $"Evento con ID '{result.Id}' modificado."
            };
            const string endpoint = "/api/Usuarios/publishActivity";

            var httpResponse = await client.PostAsJsonAsync(endpoint, activityBody, ct);
            if (!httpResponse.IsSuccessStatusCode)
            {
                _log.Warn($"[EventsController] Falló la publicación de actividad para usuario '{result.OrganizadorId}'. " +
                          $"Status={(int)httpResponse.StatusCode} ({httpResponse.StatusCode}).");
            }
            else
            {
                _log.Debug($"[EventsController] Actividad de actualización publicada para usuario '{result.OrganizadorId}'.");
            }
        }

        _log.Info($"[EventsController] Evento actualizado correctamente. Id='{id}'.");
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
        _log.Info($"[EventsController] DELETE /api/events/{id} - Eliminar evento.");

        // Primero verificamos que exista para poder armar bien la actividad
        var existing = await _mediator.Send(new GetEventByIdQuery(id), ct);
        if (existing is null)
        {
            _log.Warn($"[EventsController] No se encontró evento para eliminar. Id='{id}'. Lanzando NotFoundException.");
            throw new NotFoundException("Evento", id);
        }

        var ok = await _mediator.Send(new DeleteEventCommand(id), ct);
        if (!ok)
        {
            _log.Warn($"[EventsController] DeleteEventCommand retornó false. Id='{id}'. Lanzando NotFoundException.");
            throw new NotFoundException("Evento", id);
        }

        // Publicar actividad
        var client = _httpClientFactory.CreateClient("UsuariosClient");
        var activityBody = new PublishActivityRequest
        {
            idUsuario = existing.OrganizadorId.ToString(),
            accion = $"Evento '{existing.Nombre}' con ID '{existing.Id}' eliminado."
        };
        const string endpoint = "/api/Usuarios/publishActivity";

        var httpResponse = await client.PostAsJsonAsync(endpoint, activityBody, ct);
        if (!httpResponse.IsSuccessStatusCode)
        {
            _log.Warn($"[EventsController] Falló la publicación de actividad para usuario '{existing.OrganizadorId}'. " +
                      $"Status={(int)httpResponse.StatusCode} ({httpResponse.StatusCode}).");
        }
        else
        {
            _log.Debug($"[EventsController] Actividad de eliminación publicada para usuario '{existing.OrganizadorId}'.");
        }

        _log.Info($"[EventsController] Evento eliminado correctamente. Id='{id}'.");
        return NoContent();
    }

    /// <summary>Sube una imagen para un evento y guarda la URL en el evento.</summary>
    [HttpPost("{id:guid}/imagen")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubirImagen(
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        _log.Info($"[EventsController] POST /api/events/{id}/imagen - Subir imagen.");

        if (file is null || file.Length == 0)
        {
            _log.Warn("[EventsController] Archivo de imagen inválido o vacío.");
            return BadRequest("El archivo es inválido o está vacío.");
        }

        using var stream = file.OpenReadStream();

        var url = await _mediator.Send(
            new SubirImagenEventoCommand(
                EventoId: id,
                FileStream: stream,
                FileName: file.FileName),
            ct);

        _log.Info($"[EventsController] Imagen subida correctamente para evento '{id}'. Url='{url}'.");

        return Ok(new
        {
            EventoId = id,
            ImagenUrl = url
        });
    }

    /// <summary>Sube un folleto (PDF u otro archivo) para un evento y guarda la URL en el evento.</summary>
    [HttpPost("{id:guid}/folleto")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubirFolleto(
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        _log.Info($"[EventsController] POST /api/events/{id}/folleto - Subir folleto.");

        if (file is null || file.Length == 0)
        {
            _log.Warn("[EventsController] Archivo de folleto inválido o vacío.");
            return BadRequest("El archivo es inválido o está vacío.");
        }

        using var stream = file.OpenReadStream();

        var url = await _mediator.Send(
            new SubirFolletoEventoCommand(
                EventoId: id,
                FileStream: stream,
                FileName: file.FileName),
            ct);

        _log.Info($"[EventsController] Folleto subido correctamente para evento '{id}'. Url='{url}'.");

        return Ok(new
        {
            EventoId = id,
            FolletoUrl = url
        });
    }
}
