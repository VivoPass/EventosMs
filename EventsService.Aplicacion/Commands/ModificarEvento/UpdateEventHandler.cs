using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.ModificarEvento
{
    public sealed class UpdateEventHandler : IRequestHandler<UpdateEventCommand, bool>
    {
        private readonly IEventRepository _events;
        private readonly ICategoryRepository _cats;
        private readonly IScenarioRepository _scens;

        public UpdateEventHandler(IEventRepository events, ICategoryRepository cats, IScenarioRepository scens)
        {
            _events = events;
            _cats = cats;
            _scens = scens;
        }

        public async Task<bool> Handle(UpdateEventCommand r, CancellationToken ct)
        {
            var current = await _events.GetByIdAsync(r.Id, ct);
            if (current is null) return false; // lo manejará el controller como 404

            // Validaciones referenciales si vienen cambios
            if (r.CategoriaId.HasValue)
            {
                var ok = await _cats.ExistsAsync(r.CategoriaId.Value, ct);
                if (!ok) throw new EventoException("La categoría no existe.");
                current.CategoriaId = r.CategoriaId.Value;
            }

            if (r.EscenarioId.HasValue)
            {
                var ok = await _scens.ExistsAsync(r.EscenarioId.Value, ct);
                if (!ok) throw new EventoException("El escenario no existe.");
                current.EscenarioId = r.EscenarioId.Value;
            }

            // Merge de campos opcionales
            if (r.Nombre is not null) current.Nombre = r.Nombre;
            if (r.Inicio.HasValue) current.Inicio = r.Inicio.Value;
            if (r.Fin.HasValue) current.Fin = r.Fin.Value;
            if (r.AforoMaximo.HasValue) current.AforoMaximo = r.AforoMaximo.Value;
            if (r.Tipo is not null) current.Tipo = r.Tipo;
            if (r.Lugar is not null) current.Lugar = r.Lugar;
            if (r.Descripcion is not null) current.Descripcion = r.Descripcion;

            return await _events.UpdateAsync(current, ct);
        }
    }
}
