using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.Asiento.EliminarAsiento
{
    public class EliminarAsientoHandler : IRequestHandler<EliminarAsientoCommand, bool>
    {
        private readonly IAsientoRepository _asientos;
        private readonly IZonaEventoRepository _zonas;

        public EliminarAsientoHandler(IAsientoRepository asientos, IZonaEventoRepository zonas)
        {
            _asientos = asientos;
            _zonas = zonas;
        }

        public async Task<bool> Handle(EliminarAsientoCommand r, CancellationToken ct)
        {
            // Validar zona ↔ evento
            var zona = await _zonas.GetAsync(r.EventId, r.ZonaId, ct);
            if (zona is null || zona.EventId != r.EventId) return false;

            // Validar que el asiento pertenezca
            var seat = await _asientos.GetByIdAsync(r.AsientoId, ct);
            if (seat is null || seat.ZonaEventoId != r.ZonaId || seat.EventId != r.EventId) return false;

            return await _asientos.DeleteByIdAsync(r.AsientoId, ct);
        }
    }
}
