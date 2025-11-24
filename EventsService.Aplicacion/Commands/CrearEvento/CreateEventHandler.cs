using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.Excepciones;

namespace EventsService.Aplicacion.Commands.CrearEvento
{
    public sealed class CreateEventHandler : IRequestHandler<CreateEventCommand, Guid>
    {
        private readonly IEventRepository _events;
        private readonly ICategoryRepository _cats;
        private readonly IScenarioRepository _scen;

        public CreateEventHandler(IEventRepository eventsRepo, ICategoryRepository cats, IScenarioRepository scen)
            => (_events, _cats, _scen) = (eventsRepo, cats, scen);

        public async Task<Guid> Handle(CreateEventCommand r, CancellationToken ct)
        {
            if (!await _cats.ExistsAsync(r.CategoriaId, ct))
                throw new EventoException("La categoría no existe.");
            if (!await _scen.ExistsAsync(r.EscenarioId, ct))
                throw new EventoException("El escenario no existe.");

            var e = new Dominio.Entidades.Evento
            {
                Id = Guid.NewGuid(),
                Nombre = r.Nombre.Trim(),
                CategoriaId = r.CategoriaId,
                EscenarioId = r.EscenarioId,
                Inicio = r.Inicio,
                Fin = r.Fin,
                AforoMaximo = r.AforoMaximo,
                Tipo = r.Tipo,
                Lugar = r.Lugar,
                Descripcion = r.Descripcion,
                OrganizadorId = r.OrganizadorId
            };

            await _events.InsertAsync(e, ct);
            return e.Id;
        }
    }

}
