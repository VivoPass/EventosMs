using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using MediatR;

namespace EventsService.Aplicacion.Commands.CrearEscenario
{
    public class CreateEscenarioHandler : IRequestHandler<CreateEscenarioCommand, string>
    {
        private readonly IScenarioRepository _repo;

        public CreateEscenarioHandler (IScenarioRepository repo)
        {
            _repo = repo;
        }


        public async Task<string> Handle(CreateEscenarioCommand r, CancellationToken ct)
        {
            var escenario = new Escenario
            {
                Nombre = r.Nombre.Trim(),
                Descripcion = r.Descripcion,
                Ubicacion = r.Ubicacion,
                Ciudad = r.Ciudad,
                Estado = r.Estado,
                Pais = r.Pais
            };

            return await _repo.CrearAsync(escenario, ct);
        }
    }
}
