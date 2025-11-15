using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Interfaces;
using MediatR;

namespace EventsService.Aplicacion.Commands.ModificarEscenario
{
    public class ModificarEscenarioHandler : IRequestHandler<ModificarEscenarioCommand, Unit>
    {
        private readonly IScenarioRepository _repo;

        public ModificarEscenarioHandler(IScenarioRepository repo)
        {
            _repo = repo;
        }

        public async Task<Unit> Handle(ModificarEscenarioCommand r, CancellationToken ct)
        {
            var current = await _repo.ObtenerEscenario(r.Id, ct)
                          ?? throw new EventoException("Escenario no encontrado");

            var changes = new Escenario
            {
                Id = current.Id,
                Nombre = r.Nombre.Trim(),
                Descripcion = r.Descripcion,
                Ubicacion = r.Ubicacion,
                Ciudad = r.Ciudad,
                Estado = r.Estado,
                Pais = r.Pais
            };

            await _repo.ModificarEscenario(r.Id, changes, ct);
            return Unit.Value;
        }
    }
}
