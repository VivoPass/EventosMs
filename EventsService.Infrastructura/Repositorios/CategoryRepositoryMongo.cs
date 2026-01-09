using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Infrastructura.mongo;
using MongoDB.Driver;

namespace EventsService.Infraestructura.Repositories;

public sealed class CategoryRepositoryMongo : ICategoryRepository
{
    private readonly EventCollections _c;
    public CategoryRepositoryMongo(EventCollections c) => _c = c;

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        => await _c.Categorias.Find(x => x.Id == id).AnyAsync(ct);

    public async Task<Categoria?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var filter = Builders<Categoria>.Filter.Eq(x => x.Id, id);
        return await _c.Categorias.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<List<Categoria>> GetAllAsync(CancellationToken ct)
    {
        return await _c.Categorias.Find(_ => true).ToListAsync(ct);
    }
}