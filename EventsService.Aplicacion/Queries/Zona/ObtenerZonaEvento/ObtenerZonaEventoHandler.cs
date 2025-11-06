using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Aplicacion.DTOs.Zonas;
using EventsService.Dominio.Excepciones;

namespace EventsService.Aplicacion.Queries.Zona.ObtenerZonaEvento
{
    public class ObtenerZonaEventoHandler : IRequestHandler<ObtenerZonaEventoQuery, ZonaEventoDto?>
    {
        private readonly IZonaEventoRepository _zonaRepo;
        private readonly IEscenarioZonaRepository _ezRepo;
        private readonly IAsientoRepository _asientoRepo;

        public ObtenerZonaEventoHandler(
            IZonaEventoRepository zonaRepo,
            IEscenarioZonaRepository ezRepo,
            IAsientoRepository asientoRepo)
        {
            _zonaRepo = zonaRepo;
            _ezRepo = ezRepo;
            _asientoRepo = asientoRepo;
        }

        public async Task<ZonaEventoDto?> Handle(ObtenerZonaEventoQuery q, CancellationToken ct)
        {
            var z = await _zonaRepo.GetAsync(q.EventId, q.ZonaId, ct);
            if (z is null) throw new NotFoundException("ZonaEvento", q.EventId);

            var ez = await _ezRepo.GetByZonaAsync(q.EventId, q.ZonaId, ct);

            var vm = new ZonaEventoDto
            {
                Id = z.Id,
                EventId = z.EventId,
                EscenarioId = z.EscenarioId,
                Nombre = z.Nombre,
                Tipo = z.Tipo,
                Capacidad = z.Capacidad,
                Precio = z.Precio,
                Estado = z.Estado,
                CreatedAt = z.CreatedAt,
                UpdatedAt = z.UpdatedAt,
                Grid = new GridDto()
                {
                    StartRow = ez?.Grid.StartRow ?? 0,
                    StartCol = ez?.Grid.StartCol ?? 0,
                    RowSpan = ez?.Grid.RowSpan ?? 0,
                    ColSpan = ez?.Grid.ColSpan ?? 0,
                    Color = ez?.Color,
                    ZIndex = ez?.ZIndex,
                    Visible = ez?.Visible ?? true
                }
            };

            if (q.IncludeSeats && string.Equals(z.Tipo, "sentado", System.StringComparison.OrdinalIgnoreCase))
            {
                var seats = await _asientoRepo.ListByZonaAsync(q.EventId, q.ZonaId, ct);
                vm.Asientos = seats
                    .Select(s => new AsientoDto
                    {
                        Id = s.Id,
                        Label = s.Label,
                        Estado = s.Estado,
                        FilaIndex = s.FilaIndex,
                        ColIndex = s.ColIndex
                    })
                    .ToList();
            }

            return vm;
        }
    }
}
