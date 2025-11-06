using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.Asiento.ActualizarAsiento
{
    public class ActualizarAsientoHandler : IRequestHandler<ActualizarAsientoCommand, bool>
    {
        private readonly IAsientoRepository _asientos;
        private readonly IZonaEventoRepository _zonas;

        public ActualizarAsientoHandler(IAsientoRepository asientos, IZonaEventoRepository zonas)
        {
            _asientos = asientos;
            _zonas = zonas;
        }

        public async Task<bool> Handle(ActualizarAsientoCommand r, CancellationToken ct)
        {
            // 1) Validar zona ↔ evento
            var zona = await _zonas.GetAsync(r.EventId, r.ZonaId, ct); // usa GetByIdAsync si es tu contrato
            if (zona is null || zona.EventId != r.EventId) return false;

            // 2) Cargar asiento
            var seat = await _asientos.GetByIdAsync(r.AsientoId, ct);
            if (seat is null || seat.ZonaEventoId != r.ZonaId || seat.EventId != r.EventId) return false;

            // 3) Validaciones puntuales
            if (string.IsNullOrWhiteSpace(r.Label) == false) r = r with { Label = r.Label!.Trim() };

            // 4) Evitar duplicado si cambia Label
            if (!string.IsNullOrWhiteSpace(r.Label) && !r.Label!.Equals(seat.Label, StringComparison.Ordinal))
            {
                var dup = await _asientos.GetByCompositeAsync(r.EventId, r.ZonaId, r.Label!, ct);
                if (dup is not null) throw new EventoException("Ya existe un asiento con ese label en esta zona.");
            }

            // 5) Update parcial
            return await _asientos.UpdateParcialAsync(
                r.AsientoId,
                nuevoLabel: r.Label,
                nuevoEstado: r.Estado,
                nuevaMeta: r.Meta,
                ct: ct
            );
        }
    }
}
