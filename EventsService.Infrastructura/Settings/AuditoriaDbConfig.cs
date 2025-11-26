using MongoDB.Driver;
using EventsService.Dominio.Excepciones.Infraestructura;
using Microsoft.Extensions.Configuration;

namespace EventsService.Infrastructura.Settings
{
    public class AuditoriaDbConfig
    {
        public IMongoDatabase Db { get; }   // ← PROPIEDAD PÚBLICA

        public AuditoriaDbConfig(IConfiguration configuration)
        {
            try
            {
                var conn = configuration["Mongo:ConnectionString"];
                var dbName = configuration["Mongo:AuditoriasDatabase"];

                if (string.IsNullOrWhiteSpace(conn))
                    throw new ConexionBdInvalida();

                if (string.IsNullOrWhiteSpace(dbName))
                    throw new NombreBdInvalido();

                var settings = MongoClientSettings.FromConnectionString(conn);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);

                var client = new MongoClient(settings);
                Db = client.GetDatabase(dbName);   // ← AQUÍ
            }
            catch (MongoException ex)
            {
                throw new MongoDBConnectionException(ex);
            }
            catch (Exception ex)
            {
                throw new MongoDBUnnexpectedException(ex);
            }
        }
    }
}