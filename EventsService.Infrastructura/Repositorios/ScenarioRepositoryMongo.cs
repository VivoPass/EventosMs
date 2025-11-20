using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Infrastructura.mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventsService.Infraestructura.Repositories;

public sealed class ScenarioRepositoryMongo : IScenarioRepository
{
    private readonly EventCollections _c;
    public ScenarioRepositoryMongo(EventCollections c) => _c = c;

    public async Task<string> CrearAsync(Escenario escenario, CancellationToken ct)
    {
        await _c.Escenarios.InsertOneAsync(escenario, cancellationToken: ct);
        return escenario.Id.ToString();
    }

    public async Task EliminarEscenario(string id, CancellationToken ct)
    {
        var guid = Guid.Parse(id);
        var filtro = Builders<Escenario>.Filter.Eq(x => x.Id, guid);
        await _c.Escenarios.DeleteOneAsync(filtro, ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        var filter = Builders<Escenario>.Filter.Eq(x => x.Id, id);

        var count = await _c.Escenarios.CountDocumentsAsync(
            filter,
            cancellationToken: ct
        );

        return count > 0;
    }


    public async Task ModificarEscenario(string id, Escenario escenario, CancellationToken ct)
    {
        var guid = Guid.Parse(id);
        var filter = Builders<Escenario>.Filter.Eq(x => x.Id, guid);
        var update = Builders<Escenario>.Update
            .Set(x => x.Nombre, escenario.Nombre)
            .Set(x => x.Descripcion, escenario.Descripcion)
            .Set(x => x.Ubicacion, escenario.Ubicacion);

        await _c.Escenarios.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task<Escenario> ObtenerEscenario(string escenarioId, CancellationToken ct)
    {
        var guid = Guid.Parse(escenarioId);
        var filter = Builders<Escenario>.Filter.Eq(x => x.Id, guid);
        return await _c.Escenarios.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<Escenario> items, long total)> SearchAsync(
        string search,
        string ciudad,
        bool? activo,
        int page,
        int pageSize,
        CancellationToken ct)
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
        var docs = await find.Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync(ct);
        return (docs, total);
    }
}