using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones;

namespace EventsService.Aplicacion.Queries.Asiento.ObtenerAsiento
{
    public class ObtenerAsientoPorIdHandler : IRequestHandler<ObtenerAsientoQuery, AsientoDto?>
    {
        private readonly IAsientoRepository _asientos;
        private readonly IZonaEventoRepository _zonas;

        public ObtenerAsientoPorIdHandler(IAsientoRepository asientos, IZonaEventoRepository zonas)
        {
            _asientos = asientos;
            _zonas = zonas;
        }

        public async Task<AsientoDto?> Handle(ObtenerAsientoQuery q, CancellationToken ct)
        {
            // 1) Validar que la zona pertenezca al evento (opcional pero seguro)
            var zona = await _zonas.GetAsync(q.EventId, q.ZonaId, ct);
            if (zona is null || zona.EventId != q.EventId)
                throw new NotFoundException("ZonaEvento", q.ZonaId);

            // 2) Buscar asiento
            var seat = await _asientos.GetByIdAsync(q.AsientoId, ct);
            if (seat is null || seat.ZonaEventoId != q.ZonaId || seat.EventId != q.EventId)
                throw new NotFoundException("Asiento", q.AsientoId);

            // 3) Mapear a DTO
            return new AsientoDto
            {
                Id = seat.Id,
                Label = seat.Label,
                Estado = seat.Estado,
                FilaIndex = seat.FilaIndex,
                ColIndex = seat.ColIndex
            };
        }
    }
}
