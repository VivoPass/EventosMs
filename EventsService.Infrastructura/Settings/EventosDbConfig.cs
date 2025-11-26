using Microsoft.Extensions.Options;
using MongoDB.Driver;
using EventsService.Dominio.Excepciones.Infraestructura;

namespace EventsService.Infrastructura.Settings
{
    public class EventosDbConfig
    {
        public IMongoDatabase Database { get; }

        public EventosDbConfig(IOptions<MongoDbSettings> options)
        {
            var cfg = options.Value;

            if (string.IsNullOrWhiteSpace(cfg.ConnectionString))
                throw new ConexionBdInvalida();

            if (string.IsNullOrWhiteSpace(cfg.Database))
                throw new NombreBdInvalido();

            var settings = MongoClientSettings.FromConnectionString(cfg.ConnectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            var client = new MongoClient(settings);
            Database = client.GetDatabase(cfg.Database);
        }
    }
}