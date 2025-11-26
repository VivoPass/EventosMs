using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Infrastructura.Interfaces;
using MongoDB.Driver;
using log4net;

namespace EventsService.Infrastructura.Repositorios
{
    public class AsientoRepository : IAsientoRepository
    {
        private readonly IMongoCollection<Asiento> _col;
        private readonly IAuditoriaRepository _auditoria;
        private readonly ILog _log;

        public AsientoRepository(
            IMongoDatabase db,
            IAuditoriaRepository auditoria,
            ILog log)
        {
            _col = db.GetCollection<Asiento>("asientos");
            _auditoria = auditoria;
            _log = log ?? throw new LoggerNullException();
        }

        public async Task InsertAsync(Asiento seat, CancellationToken ct = default)
        {
            try
            {
                seat.CreatedAt = DateTime.UtcNow;
                seat.UpdatedAt = DateTime.UtcNow;

                await _col.InsertOneAsync(seat, cancellationToken: ct);

                _log.Info($"Asiento creado. ID='{seat.Id}', EventId='{seat.EventId}', ZonaEventoId='{seat.ZonaEventoId}', Label='{seat.Label}'.");

                await _auditoria.InsertarAuditoriaEvento(
                    seat.Id.ToString(),
                    "INFO",
                    "ASIENTO_CREADO",
                    $"Se creó el asiento '{seat.Label}' con ID '{seat.Id}' para el evento '{seat.EventId}' y zona '{seat.ZonaEventoId}'.");
            }
            catch (Exception ex)
            {
                _log.Error($"Error al crear asiento ID='{seat.Id}' para EventId='{seat.EventId}'.", ex);
                throw;
            }
        }

        /// <summary>
        /// Inserta múltiples asientos en una sola operación.
        /// Ideal para generación automática o importación.
        /// </summary>
        public async Task BulkInsertAsync(IEnumerable<Asiento> seats, CancellationToken ct = default)
        {
            try
            {
                var models = new List<WriteModel<Asiento>>();
                Asiento? first = null;

                foreach (var seat in seats)
                {
                    if (first == null) first = seat;
                    seat.CreatedAt = DateTime.UtcNow;
                    seat.UpdatedAt = DateTime.UtcNow;
                    models.Add(new InsertOneModel<Asiento>(seat));
                }

                if (models.Count == 0)
                {
                    _log.Debug("BulkInsertAsync llamado sin asientos. No se insertó nada.");
                    return;
                }

                await _col.BulkWriteAsync(models, cancellationToken: ct);

                _log.Info($"BulkInsertAsync -> Insertados '{models.Count}' asientos. EventId='{first?.EventId}', ZonaEventoId='{first?.ZonaEventoId}'.");

                // Auditoría genérica usando el EventId del primer asiento como referencia
                await _auditoria.InsertarAuditoriaEvento(
                    first?.EventId.ToString() ?? "DESCONOCIDO",
                    "INFO",
                    "ASIENTOS_BULK_CREADOS",
                    $"Se crearon '{models.Count}' asientos para el evento '{first?.EventId}' y zona '{first?.ZonaEventoId}'.");
            }
            catch (Exception ex)
            {
                _log.Error("Error en BulkInsertAsync de asientos.", ex);
                throw;
            }
        }

        /// <summary>
        /// Lista todos los asientos asociados a una zona específica.
        /// </summary>
        public async Task<IReadOnlyList<Asiento>> ListByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            try
            {
                _log.Debug($"Listando asientos por zona. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.");

                var list = await _col
                    .Find(x => x.EventId == eventId && x.ZonaEventoId == zonaEventoId)
                    .ToListAsync(ct);

                return list;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al listar asientos por zona. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.", ex);
                throw;
            }
        }

        /// <summary>
        /// Elimina todos los asientos disponibles de una zona,
        /// usado cuando se regeneran los asientos automáticamente.
        /// </summary>
        public async Task<int> DeleteDisponiblesByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            try
            {
                var result = await _col.DeleteManyAsync(
                    x => x.EventId == eventId &&
                         x.ZonaEventoId == zonaEventoId &&
                         x.Estado == "disponible",
                    cancellationToken: ct);

                var deleted = (int)result.DeletedCount;

                _log.Info($"DeleteDisponiblesByZonaAsync -> Eliminados '{deleted}' asientos disponibles. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.");

                if (deleted > 0)
                {
                    await _auditoria.InsertarAuditoriaEvento(
                        zonaEventoId.ToString(),
                        "INFO",
                        "ASIENTOS_DISPONIBLES_ELIMINADOS",
                        $"Se eliminaron '{deleted}' asientos disponibles para el evento '{eventId}' y zona '{zonaEventoId}'.");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al eliminar asientos disponibles por zona. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.", ex);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existen asientos registrados para una zona.
        /// </summary>
        public async Task<bool> AnyByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            try
            {
                var filter = Builders<Asiento>.Filter.Where(x => x.EventId == eventId && x.ZonaEventoId == zonaEventoId);

                var count = await _col.CountDocumentsAsync(
                    filter,
                    cancellationToken: ct
                );

                _log.Debug($"AnyByZonaAsync -> EventId='{eventId}', ZonaEventoId='{zonaEventoId}', Count='{count}'.");

                return count > 0;
            }
            catch (Exception ex)
            {
                _log.Error($"Error en AnyByZonaAsync. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.", ex);
                throw;
            }
        }

        public async Task<long> DeleteByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            try
            {
                var filter = Builders<Asiento>.Filter.Eq(x => x.EventId, eventId) &
                             Builders<Asiento>.Filter.Eq(x => x.ZonaEventoId, zonaEventoId);

                var res = await _col.DeleteManyAsync(filter, ct);
                var deleted = res.DeletedCount;

                _log.Info($"DeleteByZonaAsync -> Eliminados '{deleted}' asientos. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.");

                if (deleted > 0)
                {
                    await _auditoria.InsertarAuditoriaEvento(
                        zonaEventoId.ToString(),
                        "INFO",
                        "ASIENTOS_ELIMINADOS_POR_ZONA",
                        $"Se eliminaron '{deleted}' asientos para el evento '{eventId}' y zona '{zonaEventoId}'.");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _log.Error($"Error en DeleteByZonaAsync. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.", ex);
                throw;
            }
        }

        public async Task<Asiento?> GetByCompositeAsync(Guid eventId, Guid zonaEventoId, string label, CancellationToken ct = default)
        {
            try
            {
                var filter = Builders<Asiento>.Filter.Eq(x => x.EventId, eventId) &
                             Builders<Asiento>.Filter.Eq(x => x.ZonaEventoId, zonaEventoId) &
                             Builders<Asiento>.Filter.Eq(x => x.Label, label);

                _log.Debug($"GetByCompositeAsync -> EventId='{eventId}', ZonaEventoId='{zonaEventoId}', Label='{label}'.");

                return await _col.Find(filter).FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                _log.Error($"Error en GetByCompositeAsync. EventId='{eventId}', ZonaEventoId='{zonaEventoId}', Label='{label}'.", ex);
                throw;
            }
        }

        public async Task<Asiento?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            try
            {
                _log.Debug($"GetByIdAsync -> AsientoId='{id}'.");

                return await _col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                _log.Error($"Error en GetByIdAsync. AsientoId='{id}'.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateParcialAsync(Guid id, string? nuevoLabel, string? nuevoEstado, Dictionary<string, string>? nuevaMeta, CancellationToken ct = default)
        {
            try
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

                var res = await _col.UpdateOneAsync(
                    x => x.Id == id,
                    Builders<Asiento>.Update.Combine(updates),
                    cancellationToken: ct);

                var modified = res.IsAcknowledged && res.ModifiedCount > 0;

                if (modified)
                {
                    _log.Info($"Asiento actualizado parcialmente. ID='{id}'.");

                    await _auditoria.InsertarAuditoriaEvento(
                        id.ToString(),
                        "INFO",
                        "ASIENTO_MODIFICADO",
                        $"Se modificó parcialmente el asiento con ID '{id}'.");
                }
                else
                {
                    _log.Warn($"Intento de actualización parcial sin cambios. AsientoId='{id}'.");
                }

                return modified;
            }
            catch (Exception ex)
            {
                _log.Error($"Error en UpdateParcialAsync. AsientoId='{id}'.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken ct = default)
        {
            try
            {
                var res = await _col.DeleteOneAsync(x => x.Id == id, ct);
                var ok = res.IsAcknowledged && res.DeletedCount > 0;

                if (ok)
                {
                    _log.Info($"Asiento eliminado. ID='{id}'.");

                    await _auditoria.InsertarAuditoriaEvento(
                        id.ToString(),
                        "INFO",
                        "ASIENTO_ELIMINADO",
                        $"Se eliminó el asiento con ID '{id}'.");
                }
                else
                {
                    _log.Warn($"Intento de eliminar asiento sin resultados. ID='{id}'.");
                }

                return ok;
            }
            catch (Exception ex)
            {
                _log.Error($"Error en DeleteByIdAsync. AsientoId='{id}'.", ex);
                throw;
            }
        }
    }
}
