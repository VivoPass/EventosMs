using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Infrastructura.Repositorios
{


    public class AsientoRepository : IAsientoRepository
    {
        private readonly IMongoCollection<Asiento> _col;

        public AsientoRepository(IMongoDatabase db)
        {
            _col = db.GetCollection<Asiento>("asiento");
        }


        public async Task InsertAsync(Asiento seat, CancellationToken ct = default)
        {
            await _col.InsertOneAsync(seat, cancellationToken: ct);
        }

        /// <summary>
        /// Inserta múltiples asientos en una sola operación.
        /// Ideal para generación automática o importación.
        /// </summary>
        public async Task BulkInsertAsync(IEnumerable<Asiento> seats, CancellationToken ct = default)
        {
            var models = new List<WriteModel<Asiento>>();
            foreach (var seat in seats)
                models.Add(new InsertOneModel<Asiento>(seat));

            if (models.Count > 0)
                await _col.BulkWriteAsync(models, cancellationToken: ct);
        }

        /// <summary>
        /// Lista todos los asientos asociados a una zona específica.
        /// </summary>
        public async Task<IReadOnlyList<Asiento>> ListByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            return await _col.Find(x => x.EventId == eventId && x.ZonaEventoId == zonaEventoId)
                             .ToListAsync(ct);
        }

        /// <summary>
        /// Elimina todos los asientos disponibles de una zona,
        /// usado cuando se regeneran los asientos automáticamente.
        /// </summary>
        public async Task<int> DeleteDisponiblesByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            var result = await _col.DeleteManyAsync(
                x => x.EventId == eventId &&
                     x.ZonaEventoId == zonaEventoId &&
                     x.Estado == "disponible",
                cancellationToken: ct);

            return (int)result.DeletedCount;
        }

        /// <summary>
        /// Verifica si existen asientos registrados para una zona.
        /// </summary>
        public async Task<bool> AnyByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            var filter = Builders<Asiento>.Filter.Where(x => x.EventId == eventId && x.ZonaEventoId == zonaEventoId);

            var count = await _col.CountDocumentsAsync(
                filter,
                cancellationToken: ct
            );

            return count > 0;
        }

        public async Task<long> DeleteByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            var filter = Builders<Asiento>.Filter.Eq(x => x.EventId, eventId) &
                         Builders<Asiento>.Filter.Eq(x => x.ZonaEventoId, zonaEventoId);

            var res = await _col.DeleteManyAsync(filter, ct);
            return res.DeletedCount;
        }



        public async Task<Asiento?> GetByCompositeAsync(Guid eventId, Guid zonaEventoId, string label, CancellationToken ct = default)
        {
            var filter = Builders<Asiento>.Filter.Eq(x => x.EventId, eventId) &
                         Builders<Asiento>.Filter.Eq(x => x.ZonaEventoId, zonaEventoId) &
                         Builders<Asiento>.Filter.Eq(x => x.Label, label);

            return await _col.Find(filter).FirstOrDefaultAsync(ct);
        }

        public async Task<Asiento?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        }


        public async Task<bool> UpdateParcialAsync(Guid id, string? nuevoLabel, string? nuevoEstado, Dictionary<string, string>? nuevaMeta, CancellationToken ct = default)
        {
            var updates = new List<UpdateDefinition<Asiento>>
            {
                Builders<Asiento>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow)
            };
            if (!string.IsNullOrWhiteSpace(nuevoLabel))
                updates.Add(Builders<Asiento>.Update.Set(x => x.Label, nuevoLabel!));
            if (!string.IsNullOrWhiteSpace(nuevoEstado))
                updates.Add(Builders<Asiento>.Update.Set(x => x.Estado, nuevoEstado!));
            if (nuevaMeta is not null)
                updates.Add(Builders<Asiento>.Update.Set(x => x.Meta, nuevaMeta));

            var res = await _col.UpdateOneAsync(x => x.Id == id, Builders<Asiento>.Update.Combine(updates), cancellationToken: ct);
            return res.IsAcknowledged && res.ModifiedCount > 0;
        }


        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken ct = default)
        {
            var res = await _col.DeleteOneAsync(x => x.Id == id, ct);
            return res.IsAcknowledged && res.DeletedCount > 0;
        }
    }
}
