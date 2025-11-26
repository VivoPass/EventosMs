using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Infrastructura.mongo;
using EventsService.Infrastructura.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using log4net;

namespace EventsService.Infraestructura.Repositories
{
    public sealed class ScenarioRepositoryMongo : IScenarioRepository
    {
        private readonly EventCollections _c;
        private readonly IAuditoriaRepository _auditoria;
        private readonly ILog _log;

        public ScenarioRepositoryMongo(
            EventCollections c,
            IAuditoriaRepository auditoria,
            ILog log)
        {
            _c = c;
            _auditoria = auditoria;
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<string> CrearAsync(Escenario escenario, CancellationToken ct)
        {
            try
            {
                await _c.Escenarios.InsertOneAsync(escenario, cancellationToken: ct);

                _log.Info($"Escenario creado. ID='{escenario.Id}', Nombre='{escenario.Nombre}'.");

                await _auditoria.InsertarAuditoriaEvento(
                    escenario.Id.ToString(),
                    "INFO",
                    "ESCENARIO_CREADO",
                    $"Se creó el escenario '{escenario.Nombre}' con ID '{escenario.Id}'.");

                return escenario.Id.ToString();
            }
            catch (Exception ex)
            {
                _log.Error($"Error al crear escenario. ID='{escenario.Id}'.", ex);
                throw;
            }
        }

        public async Task EliminarEscenario(string id, CancellationToken ct)
        {
            try
            {
                var guid = Guid.Parse(id);
                var filtro = Builders<Escenario>.Filter.Eq(x => x.Id, guid);

                var result = await _c.Escenarios.DeleteOneAsync(filtro, ct);

                if (result.IsAcknowledged && result.DeletedCount > 0)
                {
                    _log.Info($"Escenario eliminado. ID='{id}'.");

                    await _auditoria.InsertarAuditoriaEvento(
                        id,
                        "INFO",
                        "ESCENARIO_ELIMINADO",
                        $"Se eliminó el escenario con ID '{id}'.");
                }
                else
                {
                    _log.Warn($"Intento de eliminar escenario ID='{id}' sin resultados (no encontrado).");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error al eliminar escenario ID='{id}'.", ex);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        {
            try
            {
                var filter = Builders<Escenario>.Filter.Eq(x => x.Id, id);

                var count = await _c.Escenarios.CountDocumentsAsync(
                    filter,
                    cancellationToken: ct
                );

                return count > 0;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al validar existencia de escenario ID='{id}'.", ex);
                throw;
            }
        }

        public async Task ModificarEscenario(string id, Escenario escenario, CancellationToken ct)
        {
            try
            {
                var guid = Guid.Parse(id);
                var filter = Builders<Escenario>.Filter.Eq(x => x.Id, guid);
                var update = Builders<Escenario>.Update
                    .Set(x => x.Nombre, escenario.Nombre)
                    .Set(x => x.Descripcion, escenario.Descripcion)
                    .Set(x => x.Ubicacion, escenario.Ubicacion);

                var result = await _c.Escenarios.UpdateOneAsync(filter, update, cancellationToken: ct);

                if (result.IsAcknowledged && result.ModifiedCount > 0)
                {
                    _log.Info($"Escenario actualizado. ID='{id}'.");

                    await _auditoria.InsertarAuditoriaEvento(
                        id,
                        "INFO",
                        "ESCENARIO_MODIFICADO",
                        $"Se modificó el escenario con ID '{id}'.");
                }
                else
                {
                    _log.Warn($"Intento de modificar escenario ID='{id}' sin modificaciones (no encontrado o sin cambios).");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error al modificar escenario ID='{id}'.", ex);
                throw;
            }
        }

        public async Task<Escenario> ObtenerEscenario(string escenarioId, CancellationToken ct)
        {
            try
            {
                var guid = Guid.Parse(escenarioId);
                var filter = Builders<Escenario>.Filter.Eq(x => x.Id, guid);

                _log.Debug($"Obteniendo escenario por ID='{escenarioId}'.");

                var escenario = await _c.Escenarios.Find(filter).FirstOrDefaultAsync(ct);

                if (escenario == null)
                    _log.Info($"No se encontró escenario con ID='{escenarioId}'.");

                return escenario;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al obtener escenario ID='{escenarioId}'.", ex);
                throw;
            }
        }

        public async Task<(IReadOnlyList<Escenario> items, long total)> SearchAsync(
            string search,
            string ciudad,
            bool? activo,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            try
            {
                var fb = Builders<Escenario>.Filter;
                var filter = fb.Empty;

                if (!string.IsNullOrWhiteSpace(search))
                    filter &= fb.Or(
                        fb.Regex(x => x.Nombre, new BsonRegularExpression(search, "i")),
                        fb.Regex(x => x.Descripcion, new BsonRegularExpression(search, "i")),
                        fb.Regex(x => x.Ubicacion, new BsonRegularExpression(search, "i"))
                    );

                if (!string.IsNullOrWhiteSpace(ciudad))
                    filter &= fb.Eq("Ciudad", ciudad);

                if (activo.HasValue)
                    filter &= fb.Eq("Activo", activo.Value);

                var find = _c.Escenarios.Find(filter).SortBy(x => x.Nombre);

                var total = await find.CountDocumentsAsync(ct);
                var docs = await find
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync(ct);

                _log.Debug($"Búsqueda de escenarios: search='{search}', ciudad='{ciudad}', activo='{activo}', total='{total}'.");

                return (docs, total);
            }
            catch (Exception ex)
            {
                _log.Error("Error al realizar búsqueda de escenarios.", ex);
                throw;
            }
        }
    }
}
