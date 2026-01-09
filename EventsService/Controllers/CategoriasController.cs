using Microsoft.AspNetCore.Mvc;
using MediatR;
using EventsService.Aplicacion.Queries.Categoria;

namespace EventsService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriasController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        // ---------------------------------------------------------------
        // GET: api/categorias
        // ---------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _mediator.Send(new ObtenerCategoriasQuery(), ct);
            return Ok(result);
        }

        // ---------------------------------------------------------------
        // GET: api/categorias/{id}
        // ---------------------------------------------------------------
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObtenerCategoriaPorIdQuery(id), ct);
            return Ok(result);
        }


    }
}
