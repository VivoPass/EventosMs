using EventsService.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Interfaces
{
    public interface IEventRepository
    {
        Task InsertAsync(Evento e, CancellationToken ct);
        Task<Evento?> GetByIdAsync(Guid id, CancellationToken ct);
        //Task<IReadOnlyList<Evento>> SearchAsync(EventSearch criteria, CancellationToken ct);
        Task<bool> UpdateAsync(Evento evento, CancellationToken ct);

        Task<List<Evento>> GetAllAsync(CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }

}
