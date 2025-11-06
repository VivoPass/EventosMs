using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Escenario;
using EventsService.Aplicacion.NewFolder;
using EventsService.Dominio.Interfaces;
using MediatR;

namespace EventsService.Aplicacion.Queries.ObtenerEscenarios
{
    public class ObtenerEscenariosHandler : IRequestHandler<ObtenerEscenariosQuery, PagedResult<EscenarioDto>>
    {
        private readonly IScenarioRepository _repo;
        public ObtenerEscenariosHandler(IScenarioRepository repo) => _repo = repo;

        public async Task<PagedResult<EscenarioDto>> Handle(ObtenerEscenariosQuery r, CancellationToken ct)
        {
            var (items, total) = await _repo.SearchAsync(r.Q ?? "", r.Ciudad ?? "", r.Activo, r.Page, r.PageSize, ct);
            var dtos = items.Select(e => new EscenarioDto(e.Id, e.Nombre, e.Descripcion, e.Ubicacion, e.Ciudad,
                    e.Estado, e.Pais, e.CapacidadTotal, e.Activo))
                .ToList();
            return new PagedResult<EscenarioDto>(dtos, total, r.Page, r.PageSize);
        }
    }
}
