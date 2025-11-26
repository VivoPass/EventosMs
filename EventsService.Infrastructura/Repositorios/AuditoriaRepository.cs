using EventsService.Infrastructura.Interfaces;
using EventsService.Infrastructura.Settings;
using EventsService.Dominio.Excepciones.Infraestructura;
using MongoDB.Bson;
using MongoDB.Driver;
using log4net;

namespace EventsService.Infrastructura.Repositorios
{
    public class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly IMongoCollection<BsonDocument> _auditoriaColeccion;
        private readonly ILog _log;

        public AuditoriaRepository(AuditoriaDbConfig mongoConfig, ILog log)
        {
            _auditoriaColeccion = mongoConfig.Db.GetCollection<BsonDocument>("auditoriaEventos");
            _log = log ?? throw new LoggerNullException();
        }

        public async Task InsertarAuditoriaEvento(string idEntidad, string level, string tipo, string mensaje)
        {
            try
            {
                var documento = new BsonDocument
                {
                    { "_id", Guid.NewGuid().ToString() },
                    { "idEntidad", idEntidad },
                    { "level", level },
                    { "tipo", tipo },
                    { "mensaje", mensaje },
                    { "timestamp", DateTime.UtcNow }
                };

                await _auditoriaColeccion.InsertOneAsync(documento);
                _log.Debug($"Auditoría de Evento insertada: Tipo='{tipo}', ID='{idEntidad}'.");
            }
            catch (MongoException ex)
            {
                _log.Error($"[FATAL DB ERROR] Fallo al insertar auditoría de evento (ID: {idEntidad}). Detalles: {ex.Message}", ex);
                throw;
            }
            catch (Exception ex)
            {
                _log.Fatal($"[FATAL ERROR] Excepción no controlada al insertar auditoría de evento (ID: {idEntidad}).", ex);
                throw new AuditoriaRepositoryException(ex);
            }
        }

        public async Task InsertarAuditoriaHistorial(string idEntidad, string level, string tipo, string mensaje)
        {
            try
            {
                var documento = new BsonDocument
                {
                    { "_id", Guid.NewGuid().ToString() },
                    { "idEntidad", idEntidad },
                    { "level", level },
                    { "tipo", tipo },
                    { "mensaje", mensaje },
                    { "timestamp", DateTime.UtcNow }
                };

                await _auditoriaColeccion.InsertOneAsync(documento);
                _log.Debug($"Auditoría de Historial de Eventos insertada: Tipo='{tipo}', ID='{idEntidad}'.");
            }
            catch (MongoException ex)
            {
                _log.Error($"[FATAL DB ERROR] Fallo al insertar historial de auditoría (ID: {idEntidad}). Detalles: {ex.Message}", ex);
                throw;
            }
            catch (Exception ex)
            {
                _log.Fatal($"[FATAL ERROR] Excepción no controlada al insertar historial de eventos (ID: {idEntidad}).", ex);
                throw new AuditoriaRepositoryException(ex);
            }
        }
    }
}
