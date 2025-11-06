using EventsService.Dominio.Entidades;
using EventsService.Dominio.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Interfaces
{
    public interface IEscenarioZonaRepository
    {
        Task AddAsync(EscenarioZona entity, CancellationToken ct = default);
        Task<EscenarioZona?> GetByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default);
        Task<IReadOnlyList<EscenarioZona>> ListByEventAsync(Guid eventId, CancellationToken ct = default);
        Task UpdateGridAsync(Guid escenarioZonaId, GridRef grid, string? color, int? zIndex, bool? visible, CancellationToken ct = default);
        Task<bool> DeleteByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default);

        Task<bool> DeleteAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default);
    }
}

