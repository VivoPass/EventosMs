using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Queries.Asiento.ListarAsientos
{
    public class ListarAsientosHandler : IRequestHandler<ListarAsientosQuery, IReadOnlyList<AsientoDto>>
    {
        private readonly IAsientoRepository _asientos;
        private readonly IZonaEventoRepository _zonas;

        public ListarAsientosHandler(IAsientoRepository asientos, IZonaEventoRepository zonas)
        {
            _asientos = asientos;
            _zonas = zonas;
        }

        public async Task<IReadOnlyList<AsientoDto>> Handle(ListarAsientosQuery q, CancellationToken ct)
        {
            // Validar que la zona pertenece al evento (defensivo)
            var zona = await _zonas.GetAsync(q.EventId, q.ZonaId, ct);
            if (zona is null || zona.EventId != q.EventId)
                return new List<AsientoDto>();

            // Obtener lista de asientos
            var seats = await _asientos.ListByZonaAsync(q.EventId, q.ZonaId, ct);

            // Mapear a DTOs
            return seats.Select(x => new AsientoDto
            {
                Id = x.Id,
                Label = x.Label,
                Estado = x.Estado,
                FilaIndex = x.FilaIndex,
                ColIndex = x.ColIndex
            }).ToList();
        }
    }
}
