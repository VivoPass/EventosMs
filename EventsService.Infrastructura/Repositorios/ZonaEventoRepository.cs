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
    public class ZonaEventoRepository : IZonaEventoRepository
    {
        private readonly IMongoCollection<ZonaEvento> _col;
        private readonly IAuditoriaRepository _auditoria;
        private readonly ILog _log;

        public ZonaEventoRepository(
            IMongoDatabase db,
            IAuditoriaRepository auditoria,
            ILog log)
        {
            _col = db.GetCollection<ZonaEvento>("zona_evento");
            _auditoria = auditoria;
            _log = log ?? throw new LoggerNullException();
        }

        public async Task AddAsync(ZonaEvento entity, CancellationToken ct = default)
        {
            try
            {
                await _col.InsertOneAsync(entity, cancellationToken: ct);

                _log.Info($"ZonaEvento creada. ID='{entity.Id}', EventId='{entity.EventId}', Nombre='{entity.Nombre}'.");

                await _auditoria.InsertarAuditoriaEvento(
                    entity.Id.ToString(),
                    "INFO",
                    "ZONA_EVENTO_CREADA",
                    $"Se creó la zona de evento '{entity.Nombre}' con ID '{entity.Id}' para el evento '{entity.EventId}'.");
            }
            catch (Exception ex)
            {
                _log.Error($"Error al crear ZonaEvento ID='{entity.Id}' para EventId='{entity.EventId}'.", ex);
                throw;
            }
        }

        public async Task<ZonaEvento?> GetAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            try
            {
                _log.Debug($"Obteniendo ZonaEvento. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.");

                return await _col
                    .Find(x => x.EventId == eventId && x.Id == zonaEventoId)
                    .FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                _log.Error($"Error al obtener ZonaEvento. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<ZonaEvento>> ListByEventAsync(Guid eventId, CancellationToken ct = default)
        {
            try
            {
                _log.Debug($"Listando ZonasEvento para EventId='{eventId}'.");

                var result = await _col
                    .Find(x => x.EventId == eventId)
                    .ToListAsync(ct);

                return result;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al listar ZonasEvento para EventId='{eventId}'.", ex);
                throw;
            }
        }

        public async Task UpdateAsync(ZonaEvento entity, CancellationToken ct = default)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;

                var result = await _col.ReplaceOneAsync(
                    x => x.Id == entity.Id,
                    entity,
                    cancellationToken: ct);

                if (result.IsAcknowledged && result.ModifiedCount > 0)
                {
                    _log.Info($"ZonaEvento actualizada. ID='{entity.Id}', EventId='{entity.EventId}'.");

                    await _auditoria.InsertarAuditoriaEvento(
                        entity.Id.ToString(),
                        "INFO",
                        "ZONA_EVENTO_MODIFICADA",
                        $"Se modificó la zona de evento '{entity.Nombre}' con ID '{entity.Id}' para el evento '{entity.EventId}'.");
                }
                else
                {
                    _log.Warn($"Intento de actualizar ZonaEvento ID='{entity.Id}' sin modificaciones (no encontrada o sin cambios).");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error al actualizar ZonaEvento ID='{entity.Id}' para EventId='{entity.EventId}'.", ex);
                throw;
            }
        }

        public async Task<bool> ExistsByNombreAsync(Guid eventId, string nombre, CancellationToken ct = default)
        {
            try
            {
                var filter = Builders<ZonaEvento>.Filter.And(
                    Builders<ZonaEvento>.Filter.Eq(x => x.EventId, eventId),
                    Builders<ZonaEvento>.Filter.Eq(x => x.Nombre, nombre)
                );

                var count = await _col.CountDocumentsAsync(filter, cancellationToken: ct);

                _log.Debug($"ExistsByNombreAsync -> EventId='{eventId}', Nombre='{nombre}', Count='{count}'.");

                return count > 0;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al validar existencia de ZonaEvento por nombre. EventId='{eventId}', Nombre='{nombre}'.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            try
            {
                var filter = Builders<ZonaEvento>.Filter.Eq(x => x.EventId, eventId) &
                             Builders<ZonaEvento>.Filter.Eq(x => x.Id, zonaEventoId);

                var res = await _col.DeleteOneAsync(filter, ct);

                if (res.IsAcknowledged && res.DeletedCount > 0)
                {
                    _log.Info($"ZonaEvento eliminada. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.");

                    await _auditoria.InsertarAuditoriaEvento(
                        zonaEventoId.ToString(),
                        "INFO",
                        "ZONA_EVENTO_ELIMINADA",
                        $"Se eliminó la zona de evento con ID '{zonaEventoId}' para el evento '{eventId}'.");
                    return true;
                }

                _log.Warn($"Intento de eliminar ZonaEvento sin resultados. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.");
                return false;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al eliminar ZonaEvento. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.", ex);
                throw;
            }
        }
    }
}
