using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using MongoDB.Driver;

namespace EventsService.Infrastructura.Repositorios
{


    public class ZonaEventoRepository : IZonaEventoRepository
    {
        private readonly IMongoCollection<ZonaEvento> _col;

        public ZonaEventoRepository(IMongoDatabase db)
        {
            _col = db.GetCollection<ZonaEvento>("zona_evento");
        }

        public Task AddAsync(ZonaEvento entity, CancellationToken ct = default)
            => _col.InsertOneAsync(entity, cancellationToken: ct);

        public async Task<ZonaEvento?> GetAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            return await _col.Find(x => x.EventId == eventId && x.Id == zonaEventoId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<ZonaEvento>> ListByEventAsync(Guid eventId, CancellationToken ct = default)
        {
            return await _col.Find(x => x.EventId == eventId).ToListAsync(ct);
        }

        public Task UpdateAsync(ZonaEvento entity, CancellationToken ct = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            return _col.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: ct);
        }

        public async Task<bool> ExistsByNombreAsync(Guid eventId, string nombre, CancellationToken ct = default)
        {
            return await _col.Find(x => x.EventId == eventId && x.Nombre.ToLower() == nombre.ToLower())
                .AnyAsync(ct);
        }

        public async Task<bool> DeleteAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            var filter = Builders<ZonaEvento>.Filter.Eq(x => x.EventId, eventId) &
                         Builders<ZonaEvento>.Filter.Eq(x => x.Id, zonaEventoId);

            var res = await _col.DeleteOneAsync(filter, ct);
            return res.DeletedCount > 0;
        }
    }
}
