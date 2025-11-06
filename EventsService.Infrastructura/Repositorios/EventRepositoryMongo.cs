using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;

using EventsService.Dominio.Interfaces;
using EventsService.Infrastructura.mongo;
using MongoDB.Driver;

namespace EventsService.Infraestructura.Repositories;

public sealed class EventRepositoryMongo : IEventRepository
{
    private readonly EventCollections _c;
    public EventRepositoryMongo(EventCollections c) => _c = c;

    public async Task InsertAsync(Evento e, CancellationToken ct)
        => await _c.Eventos.InsertOneAsync(e, cancellationToken: ct);

    public async Task<Evento?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _c.Eventos.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    /*  public async Task<IReadOnlyList<Evento>> SearchAsync(EventSearch criteria, CancellationToken ct)
      {
          var f = Builders<Evento>.Filter.Empty;

          if (!string.IsNullOrWhiteSpace(criteria.Estado))
              f &= Builders<Evento>.Filter.Eq(x => x.Estado, criteria.Estado);

          if (criteria.From is not null)
              f &= Builders<Evento>.Filter.Gte(x => x.Inicio, criteria.From.Value);

          if (criteria.To is not null)
              f &= Builders<Evento>.Filter.Lte(x => x.Inicio, criteria.To.Value);

          var list = await _c.Eventos.Find(f).SortBy(x => x.Inicio).ToListAsync(ct);
          return list;
      }*/

    public async Task<bool> UpdateAsync(Evento evento, CancellationToken ct) // ← NUEVO
    {
        var result = await _c.Eventos.ReplaceOneAsync(x => x.Id == evento.Id, evento, cancellationToken: ct);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public Task<List<Evento>> GetAllAsync(CancellationToken ct) 
        => _c.Eventos.Find(FilterDefinition<Evento>.Empty).ToListAsync(ct);

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) // ← NUEVO
    {
        var result = await _c.Eventos.DeleteOneAsync(x => x.Id == id, ct);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }



}