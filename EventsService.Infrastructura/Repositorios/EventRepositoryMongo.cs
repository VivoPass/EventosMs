using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Infrastructura.mongo;
using EventsService.Infrastructura.Interfaces;
using MongoDB.Driver;
using log4net;

namespace EventsService.Infraestructura.Repositories
{
    public sealed class EventRepositoryMongo : IEventRepository
    {
        private readonly EventCollections _c;
        private readonly IAuditoriaRepository _auditoria;
        private readonly ILog _log;

        public EventRepositoryMongo(
            EventCollections collections,
            IAuditoriaRepository auditoria,
            ILog log)
        {
            _c = collections;
            _auditoria = auditoria;
            _log = log ?? throw new LoggerNullException();
        }

        public async Task InsertAsync(Evento e, CancellationToken ct)
        {
            try
            {
                await _c.Eventos.InsertOneAsync(e, cancellationToken: ct);

                _log.Info($"Evento creado en MongoDB. ID='{e.Id}'.");

                await _auditoria.InsertarAuditoriaEvento(
                    e.Id.ToString(),
                    "INFO",
                    "EVENTO_CREADO",
                    $"Se creó el evento con ID '{e.Id}'.");
            }
            catch (Exception ex)
            {
                _log.Error($"Error al crear evento ID='{e.Id}' en MongoDB.", ex);
                throw;
            }
        }

        public async Task<Evento?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            try
            {
                _log.Debug($"Buscando evento por ID='{id}' en MongoDB.");

                /*  var evt = await _c.Eventos
                      .Find(x => x.Id == id)
                      .FirstOrDefaultAsync(ct);*/
                var cursor = await _c.Eventos.FindAsync(x => x.Id == id, cancellationToken: ct);
                var evt = await cursor.FirstOrDefaultAsync(ct);

                if (evt == null)
                    _log.Info($"No se encontró evento con ID='{id}'.");

                return evt;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al obtener evento por ID='{id}' en MongoDB.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Evento evento, CancellationToken ct)
        {
            try
            {
                var result = await _c.Eventos
                    .ReplaceOneAsync(x => x.Id == evento.Id, evento, cancellationToken: ct);

                if (result.IsAcknowledged && result.ModifiedCount > 0)
                {
                    _log.Info($"Evento actualizado en MongoDB. ID='{evento.Id}'.");

                    await _auditoria.InsertarAuditoriaEvento(
                        evento.Id.ToString(),
                        "INFO",
                        "EVENTO_MODIFICADO",
                        $"Se modificó el evento con ID '{evento.Id}'.");

                    return true;
                }

                _log.Warn($"Intento de actualizar evento ID='{evento.Id}' sin modificaciones (no encontrado o sin cambios).");
                return false;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al actualizar evento ID='{evento.Id}' en MongoDB.", ex);
                throw;
            }
        }

        public async Task<List<Evento>> GetAllAsync(CancellationToken ct)
        {
            var cursor = await _c.Eventos.FindAsync(FilterDefinition<Evento>.Empty, cancellationToken: ct);
            return await cursor.ToListAsync(ct);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _c.Eventos.DeleteOneAsync(x => x.Id == id, ct);

                if (result.IsAcknowledged && result.DeletedCount > 0)
                {
                    _log.Info($"Evento eliminado en MongoDB. ID='{id}'.");

                    await _auditoria.InsertarAuditoriaEvento(
                        id.ToString(),
                        "INFO",
                        "EVENTO_ELIMINADO",
                        $"Se eliminó el evento con ID '{id}'.");

                    return true;
                }

                _log.Warn($"Intento de eliminar evento ID='{id}' sin resultados (no encontrado).");
                return false;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al eliminar evento ID='{id}' en MongoDB.", ex);
                throw;
            }
        }
    }
}
