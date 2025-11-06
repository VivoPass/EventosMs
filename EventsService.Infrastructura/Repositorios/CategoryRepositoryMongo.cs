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
}