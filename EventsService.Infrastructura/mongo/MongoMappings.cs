using EventsService.Dominio.Entidades;
using EventsService.Dominio.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace EventsService.Infrastructura.mongo
{
    public static class MongoMappings
    {
        private static bool _configured = false;

        public static void Configure()
        {
            if (_configured) return;
            _configured = true;


            // Mapeo de Evento
            if (!BsonClassMap.IsClassMapRegistered(typeof(Evento)))
            {
                BsonClassMap.RegisterClassMap<Evento>(cm =>
                {
                    cm.AutoMap();

                    // Aseguramos que todos los Guid se serialicen de forma consistente
                    cm.MapIdProperty(x => x.Id)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(x => x.CategoriaId)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(x => x.EscenarioId)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(x => x.OrganizadorId)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Escenario)))
            {
                BsonClassMap.RegisterClassMap<Escenario>(cm =>
                {
                    cm.AutoMap();

                    cm.MapIdProperty(x => x.Id)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Asiento)))
            {
                BsonClassMap.RegisterClassMap<Asiento>(cm =>
                {
                    cm.AutoMap();

                    cm.MapIdProperty(x => x.Id)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(x => x.EventId)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(x => x.ZonaEventoId)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(ZonaEvento)))
            {
                BsonClassMap.RegisterClassMap<ZonaEvento>(cm =>
                {
                    cm.AutoMap();

                    cm.MapIdProperty(x => x.Id)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(x => x.EventId)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));



                    cm.MapMember(x => x.EscenarioId)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(EscenarioZona)))
            {
                BsonClassMap.RegisterClassMap<EscenarioZona>(cm =>
                {
                    cm.AutoMap();

                    cm.MapIdProperty(x => x.Id)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(x => x.EventId)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(x => x.EscenarioId)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(x => x.ZonaEventoId)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }

            // GridRef (VO embebido)
            if (!BsonClassMap.IsClassMapRegistered(typeof(GridRef)))
            {
                BsonClassMap.RegisterClassMap<GridRef>(cm =>
                {
                    cm.AutoMap();
                });
            }
        }
    }
}

