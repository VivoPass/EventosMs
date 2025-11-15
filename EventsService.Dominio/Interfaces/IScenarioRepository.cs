using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;

namespace EventsService.Dominio.Interfaces
{
    public interface IScenarioRepository
    {
        Task<bool> ExistsAsync(Guid id, CancellationToken ct);
        Task<string> CrearAsync(Escenario escenario, CancellationToken ct);
        Task<Escenario> ObtenerEscenario(string escenarioId, CancellationToken ct);


        Task<(IReadOnlyList<Escenario> items, long total)> SearchAsync(
       string search, string ciudad, bool? activo, int page, int pageSize, CancellationToken ct);

        Task ModificarEscenario(string id, Escenario escenario, CancellationToken ct);
        Task EliminarEscenario(string id, CancellationToken ct);
        
    }
}
