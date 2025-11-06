using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.ValueObjects;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Infrastructura.Repositorios
{

    public class EscenarioZonaRepository : IEscenarioZonaRepository
    {
        private readonly IMongoCollection<EscenarioZona> _col;

        public EscenarioZonaRepository(IMongoDatabase db)
        {
            _col = db.GetCollection<EscenarioZona>("escenario_zona");
        }

        public Task AddAsync(EscenarioZona entity, CancellationToken ct = default)
            => _col.InsertOneAsync(entity, cancellationToken: ct);

        public async Task<EscenarioZona?> GetByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            return await _col.Find(x => x.EventId == eventId && x.ZonaEventoId == zonaEventoId)
                             .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<EscenarioZona>> ListByEventAsync(Guid eventId, CancellationToken ct = default)
        {
            return await _col.Find(x => x.EventId == eventId).ToListAsync(ct);
        }

        public async Task UpdateGridAsync(
            Guid escenarioZonaId,
            GridRef grid,
            string? color = null,
            int? zIndex = null,
            bool? visible = null,
            CancellationToken ct = default)
        {
            var update = Builders<EscenarioZona>.Update
                .Set(x => x.Grid.StartRow, grid.StartRow)
                .Set(x => x.Grid.StartCol, grid.StartCol)
                .Set(x => x.Grid.RowSpan, grid.RowSpan)
                .Set(x => x.Grid.ColSpan, grid.ColSpan)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            if (color != null)
                update = update.Set(x => x.Color, color);
            if (zIndex.HasValue)
                update = update.Set(x => x.ZIndex, zIndex.Value);
            if (visible.HasValue)
                update = update.Set(x => x.Visible, visible.Value);

            await _col.UpdateOneAsync(x => x.Id == escenarioZonaId, update, cancellationToken: ct);
        }

        public async Task<bool> DeleteByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            var filter = Builders<EscenarioZona>.Filter.Eq(x => x.EventId, eventId) &
                         Builders<EscenarioZona>.Filter.Eq(x => x.ZonaEventoId, zonaEventoId);

            var res = await _col.DeleteOneAsync(filter, ct);
            return res.DeletedCount > 0;
        }

        public Task<bool> DeleteAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
