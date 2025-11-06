using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.EliminarEscenario
{
    public class EliminarEscenarioHandler : IRequestHandler<EliminarEscenarioCommand, Unit>
    {

        private readonly IScenarioRepository _repo;
        public EliminarEscenarioHandler(IScenarioRepository repo) => _repo = repo;

        public async Task<Unit> Handle(EliminarEscenarioCommand r, CancellationToken ct)
        {
            await _repo.EliminarEscenario(r.Id, ct);
            return Unit.Value;
        }
    }
}
