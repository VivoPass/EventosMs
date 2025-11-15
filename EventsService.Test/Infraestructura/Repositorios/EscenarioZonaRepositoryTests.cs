using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.ValueObjects;
using EventsService.Infrastructura.Repositorios;
using EventsService.Infrastructura.mongo;
using Mongo2Go;
using MongoDB.Driver;
using Xunit;

namespace EventsService.Tests.Infraestructura.Repositorios
{
    public class EscenarioZonaRepositoryTests : IDisposable
    {
        private readonly MongoDbRunner _runner;
        private readonly IMongoDatabase _db;
        private readonly EscenarioZonaRepository _sut;

        public EscenarioZonaRepositoryTests()
        {
            MongoMappings.Configure();

            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            _db = client.GetDatabase("events_service_escenariozona_tests");

            _sut = new EscenarioZonaRepository(_db);
        }

        [Fact]
        public async Task AddAsync_Should_Insert_And_Retrieve()
        {
            var ct = CancellationToken.None;

            var eventId = Guid.NewGuid();
            var zonaId = Guid.NewGuid();

            var entity = CrearEscenarioZona(eventId, zonaId);

            await _sut.AddAsync(entity, ct);

            var loaded = await _sut.GetByZonaAsync(eventId, zonaId, ct);

            Assert.NotNull(loaded);
            Assert.Equal(entity.Id, loaded!.Id);
            Assert.Equal(eventId, loaded.EventId);
            Assert.Equal(zonaId, loaded.ZonaEventoId);
        }

        [Fact]
        public async Task ListByEventAsync_Should_Return_Only_Event_Zones()
        {
            var ct = CancellationToken.None;

            var e1 = Guid.NewGuid();
            var e2 = Guid.NewGuid();

            await _sut.AddAsync(CrearEscenarioZona(e1, Guid.NewGuid()), ct);
            await _sut.AddAsync(CrearEscenarioZona(e1, Guid.NewGuid()), ct);
            await _sut.AddAsync(CrearEscenarioZona(e2, Guid.NewGuid()), ct);

            var list = await _sut.ListByEventAsync(e1, ct);

            Assert.Equal(2, list.Count);
            Assert.All(list, z => Assert.Equal(e1, z.EventId));
        }

        [Fact]
        public async Task UpdateGridAsync_Should_Update_Grid_And_Optional_Fields()
        {
            var ct = CancellationToken.None;

            var eventId = Guid.NewGuid();
            var zonaId = Guid.NewGuid();

            var entity = CrearEscenarioZona(eventId, zonaId);
            await _sut.AddAsync(entity, ct);

            var newGrid = new GridRef
            {
                StartRow = 2,
                StartCol = 3,
                RowSpan = 4,
                ColSpan = 5
            };

            await _sut.UpdateGridAsync(
                entity.Id,
                newGrid,
                color: "red",
                zIndex: 10,
                visible: false,
                ct: ct
            );

            var updated = await _sut.GetByZonaAsync(eventId, zonaId, ct);

            Assert.NotNull(updated);
            Assert.Equal(2, updated!.Grid.StartRow);
            Assert.Equal(3, updated.Grid.StartCol);
            Assert.Equal(4, updated.Grid.RowSpan);
            Assert.Equal(5, updated.Grid.ColSpan);
            Assert.Equal("red", updated.Color);
            Assert.Equal(10, updated.ZIndex);
            Assert.False(updated.Visible);
            Assert.True(updated.UpdatedAt > entity.UpdatedAt);
        }

        [Fact]
        public async Task DeleteByZonaAsync_Should_Remove_Only_Target()
        {
            var ct = CancellationToken.None;

            var e = Guid.NewGuid();

            var z1 = CrearEscenarioZona(e, Guid.NewGuid());
            var z2 = CrearEscenarioZona(e, Guid.NewGuid());

            await _sut.AddAsync(z1, ct);
            await _sut.AddAsync(z2, ct);

            var deleted = await _sut.DeleteByZonaAsync(e, z1.ZonaEventoId, ct);

            var r1 = await _sut.GetByZonaAsync(e, z1.ZonaEventoId, ct);
            var r2 = await _sut.GetByZonaAsync(e, z2.ZonaEventoId, ct);

            Assert.True(deleted);
            Assert.Null(r1);
            Assert.NotNull(r2);
        }

        private static EscenarioZona CrearEscenarioZona(Guid eventId, Guid zonaId)
        {

            var newGrid = new GridRef
            {
                StartRow = 2,
                StartCol = 3,
                RowSpan = 4,
                ColSpan = 5
            };
            return new EscenarioZona
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                ZonaEventoId = zonaId,
                Color = "blue",
                Visible = true,
                ZIndex = 1,
                UpdatedAt = DateTime.UtcNow,
                Grid = newGrid
            };
        }

        public void Dispose()
        {
            _runner?.Dispose();
        }
    }
}
