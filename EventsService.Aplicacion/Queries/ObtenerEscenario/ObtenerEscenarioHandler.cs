using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Escenario;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Interfaces;
using MediatR;

namespace EventsService.Aplicacion.Queries.ObtenerEscenario
{
    public  class ObtenerEscenarioHandler : IRequestHandler<ObtenerEscenarioQuery, EscenarioDto>
    {
        private readonly IScenarioRepository _repo;
        public ObtenerEscenarioHandler(IScenarioRepository repo) => _repo = repo;

        public async Task<EscenarioDto> Handle(ObtenerEscenarioQuery r, CancellationToken ct)
        {
            var e = await _repo.ObtenerEscenario(r.Id, ct)
                    ?? throw new EventoException("Escenario no encontrado");

            return new EscenarioDto(e.Id, e.Nombre, e.Descripcion, e.Ubicacion, e.Ciudad, e.Estado, e.Pais,
                e.CapacidadTotal, e.Activo);
        }
    }
}
