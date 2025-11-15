// Api/Controllers/EscenariosController.cs

using EventsService.Api.Contracs.Escenario;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// Usings de tus Commands/Queries/DTOs en Application

using EventsService.Aplicacion.Commands.CrearEscenario;
using EventsService.Aplicacion.Commands.EliminarEscenario;
using EventsService.Aplicacion.Commands.ModificarEscenario;
using EventsService.Aplicacion.NewFolder;
using EventsService.Aplicacion.Queries.ObtenerEscenario;
using EventsService.Aplicacion.Queries.ObtenerEscenarios;
using EventsService.Dominio.Excepciones; // EscenarioDto

namespace Api.Controllers;

[ApiController]
[Route("api/escenarios")]
public class EscenariosController : ControllerBase
{
    private readonly IMediator _mediator;

    public EscenariosController(IMediator mediator) => _mediator = mediator;

    /// <summary>Crea un Escenario.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> Create(
        [FromBody] EscenarioCreateRequest req,
        CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateEscenarioCommand(
            req.Nombre, req.Descripcion, req.Ubicacion, req.Ciudad, req.Estado, req.Pais
        ), ct);

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
        await _mediator.Send(new ModificarEscenarioCommand(
            id, req.Nombre, req.Descripcion, req.Ubicacion, req.Ciudad, req.Estado, req.Pais
        ), ct);

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
        await _mediator.Send(new EliminarEscenarioCommand(id), ct);
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
        var dto = await _mediator.Send(new ObtenerEscenarioQuery(id), ct);
        if (dto is null) throw new NotFoundException("Escenario", id);

        var resp = new EscenarioResponse(
            dto.Id, dto.Nombre, dto.Descripcion, dto.Ubicacion,
            dto.Ciudad, dto.Estado, dto.Pais, dto.CapacidadTotal, dto.Activo
        );

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
        var result = await _mediator.Send(new ObtenerEscenariosQuery(q, ciudad, activo, page, pageSize), ct);
        var items = result.Items.Select(d => new EscenarioResponse(
            d.Id, d.Nombre, d.Descripcion, d.Ubicacion,
            d.Ciudad, d.Estado, d.Pais, d.CapacidadTotal, d.Activo
        )).ToList();

        return Ok(new PagedResult<EscenarioResponse>(items, result.Total, result.Page, result.PageSize));
    }
}
