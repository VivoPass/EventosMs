using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;

namespace EventsService.Dominio.Interfaces
{
    public interface ICategoryRepository
    {
        Task<bool> ExistsAsync(Guid id, CancellationToken ct);
        Task<Categoria?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<List<Categoria>> GetAllAsync(CancellationToken ct);

    }
}
