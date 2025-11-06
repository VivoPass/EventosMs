using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.ValueObjects;

namespace EventsService.Dominio.Interfaces
{
    /// <summary>
    /// Casos de uso sobre Zonas de un Evento.
    /// Interfaz en Dominio; solo usa Entidades/VOs del Dominio y tipos primitivos.
    /// La implementación vive fuera (Aplicación/Infraestructura).d
    /// </summary>
    public interface IZonaEventoRepository
    {
        Task AddAsync(ZonaEvento entity, CancellationToken ct = default);
        Task<ZonaEvento?> GetAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default);
        Task<IReadOnlyList<ZonaEvento>> ListByEventAsync(Guid eventId, CancellationToken ct = default);
        Task UpdateAsync(ZonaEvento entity, CancellationToken ct = default);
        Task<bool> ExistsByNombreAsync(Guid eventId, string nombre, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default);
    }
}
