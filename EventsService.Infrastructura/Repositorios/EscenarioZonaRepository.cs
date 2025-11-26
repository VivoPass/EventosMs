using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.ValueObjects;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Infrastructura.Interfaces;
using MongoDB.Driver;
using log4net;

namespace EventsService.Infrastructura.Repositorios
{
    public class EscenarioZonaRepository : IEscenarioZonaRepository
    {
        private readonly IMongoCollection<EscenarioZona> _col;
        private readonly IAuditoriaRepository _auditoria;
        private readonly ILog _log;

        public EscenarioZonaRepository(
            IMongoDatabase db,
            IAuditoriaRepository auditoria,
            ILog log)
        {
            _col = db.GetCollection<EscenarioZona>("escenario_zona");
            _auditoria = auditoria;
            _log = log ?? throw new LoggerNullException();
        }

        public async Task AddAsync(EscenarioZona entity, CancellationToken ct = default)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                await _col.InsertOneAsync(entity, cancellationToken: ct);

                _log.Info($"EscenarioZona creada. ID='{entity.Id}', EventId='{entity.EventId}', ZonaEventoId='{entity.ZonaEventoId}'.");

                await _auditoria.InsertarAuditoriaEvento(
                    entity.Id.ToString(),
                    "INFO",
                    "ESCENARIO_ZONA_CREADA",
                    $"Se creó el EscenarioZona con ID '{entity.Id}' para el evento '{entity.EventId}' y zona '{entity.ZonaEventoId}'.");
            }
            catch (Exception ex)
            {
                _log.Error($"Error al crear EscenarioZona ID='{entity.Id}' para EventId='{entity.EventId}'.", ex);
                throw;
            }
        }

        public async Task<EscenarioZona?> GetByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            try
            {
                _log.Debug($"Obteniendo EscenarioZona por zona. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.");

                return await _col
                    .Find(x => x.EventId == eventId && x.ZonaEventoId == zonaEventoId)
                    .FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                _log.Error($"Error al obtener EscenarioZona por zona. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<EscenarioZona>> ListByEventAsync(Guid eventId, CancellationToken ct = default)
        {
            try
            {
                _log.Debug($"Listando EscenarioZona por evento. EventId='{eventId}'.");

                var list = await _col
                    .Find(x => x.EventId == eventId)
                    .ToListAsync(ct);

                return list;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al listar EscenarioZona por evento. EventId='{eventId}'.", ex);
                throw;
            }
        }

        public async Task UpdateGridAsync(
            Guid escenarioZonaId,
            GridRef grid,
            string? color = null,
            int? zIndex = null,
            bool? visible = null,
            CancellationToken ct = default)
        {
            try
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

                var result = await _col.UpdateOneAsync(
                    x => x.Id == escenarioZonaId,
                    update,
                    cancellationToken: ct);

                if (result.IsAcknowledged && result.ModifiedCount > 0)
                {
                    _log.Info($"EscenarioZona grid actualizado. ID='{escenarioZonaId}'.");

                    await _auditoria.InsertarAuditoriaEvento(
                        escenarioZonaId.ToString(),
                        "INFO",
                        "ESCENARIO_ZONA_GRID_ACTUALIZADO",
                        $"Se actualizó el grid del EscenarioZona con ID '{escenarioZonaId}'.");
                }
                else
                {
                    _log.Warn($"Intento de actualizar grid de EscenarioZona ID='{escenarioZonaId}' sin modificaciones.");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error al actualizar grid de EscenarioZona ID='{escenarioZonaId}'.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteByZonaAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            try
            {
                var filter = Builders<EscenarioZona>.Filter.Eq(x => x.EventId, eventId) &
                             Builders<EscenarioZona>.Filter.Eq(x => x.ZonaEventoId, zonaEventoId);

                var res = await _col.DeleteOneAsync(filter, ct);

                if (res.IsAcknowledged && res.DeletedCount > 0)
                {
                    _log.Info($"EscenarioZona eliminado por zona. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.");

                    await _auditoria.InsertarAuditoriaEvento(
                        zonaEventoId.ToString(),
                        "INFO",
                        "ESCENARIO_ZONA_ELIMINADO_POR_ZONA",
                        $"Se eliminó EscenarioZona para el evento '{eventId}' y zona '{zonaEventoId}'.");
                    return true;
                }

                _log.Warn($"Intento de eliminar EscenarioZona por zona sin resultados. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.");
                return false;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al eliminar EscenarioZona por zona. EventId='{eventId}', ZonaEventoId='{zonaEventoId}'.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid eventId, Guid zonaEventoId, CancellationToken ct = default)
        {
            // Si tu interfaz te pide DeleteAsync, lo hacemos wrapper de DeleteByZonaAsync
            return await DeleteByZonaAsync(eventId, zonaEventoId, ct);
        }
    }
}
