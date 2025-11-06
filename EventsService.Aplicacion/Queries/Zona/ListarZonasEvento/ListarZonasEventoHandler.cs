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

namespace EventsService.Aplicacion.Queries.Zona.ListarZonasEvento
{
    public class ListZonasEventoHandler : IRequestHandler<ListarZonasEventoQuery, IReadOnlyList<ZonaEventoDto>>
    {
        private readonly IZonaEventoRepository _zonaRepo;
        private readonly IEscenarioZonaRepository _ezRepo;
        private readonly IAsientoRepository _asientoRepo;

        public ListZonasEventoHandler(
            IZonaEventoRepository zonaRepo,
            IEscenarioZonaRepository ezRepo,
            IAsientoRepository asientoRepo)
        {
            _zonaRepo = zonaRepo;
            _ezRepo = ezRepo;
            _asientoRepo = asientoRepo;
        }

        public async Task<IReadOnlyList<ZonaEventoDto>> Handle(ListarZonasEventoQuery q, CancellationToken ct)
        {
            var zonas = await _zonaRepo.ListByEventAsync(q.EventId, ct);

            // Filtros en memoria (si quieres llevar a Mongo luego, migramos a repo)
            if (!string.IsNullOrWhiteSpace(q.Tipo))
                zonas = zonas.Where(z => string.Equals(z.Tipo, q.Tipo, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(q.Estado))
                zonas = zonas.Where(z => string.Equals(z.Estado, q.Estado, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(q.Search))
                zonas = zonas.Where(z => z.Nombre.Contains(q.Search, StringComparison.OrdinalIgnoreCase)).ToList();

            var list = new List<ZonaEventoDto>(zonas.Count);
            foreach (var z in zonas)
            {
                var ez = await _ezRepo.GetByZonaAsync(z.EventId, z.Id, ct);

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

                if (q.IncludeSeats && string.Equals(z.Tipo, "sentado", StringComparison.OrdinalIgnoreCase))
                {
                    var seats = await _asientoRepo.ListByZonaAsync(z.EventId, z.Id, ct);
                    vm.Asientos = seats.Select(s => new AsientoDto()
                    {
                        Id = s.Id,
                        Label = s.Label,
                        Estado = s.Estado,
                        FilaIndex = s.FilaIndex,
                        ColIndex = s.ColIndex
                    }).ToList();
                }

                list.Add(vm);
            }

            return list;
        }
    }
}
