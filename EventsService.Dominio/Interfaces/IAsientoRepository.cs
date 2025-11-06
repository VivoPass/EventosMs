using EventsService.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Interfaces
{
    public interface IAsientoRepository
    {
        // Asientos
        Task<Asiento?> GetByCompositeAsync(Guid eventId, Guid zonaEventoId, string label, CancellationToken ct = default);
        Task InsertAsync(Asiento seat, CancellationToken ct = default);
        Task<Asiento?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> UpdateParcialAsync(Guid id, string? nuevoLabel, string? nuevoEstado, Dictionary<string, string>? nuevaMeta, CancellationToken ct = default);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken ct = default);



        // ZonasAsiento
        Task BulkInsertAsync(IEnumerable<Asiento> seats, CancellationToken ct = default);
        Task<IReadOnlyList<Asiento>> ListByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default);
        Task<int> DeleteDisponiblesByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default);
        Task<bool> AnyByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default);
        Task<long> DeleteByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default);
    }
}
