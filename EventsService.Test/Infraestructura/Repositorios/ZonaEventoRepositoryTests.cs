using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Infrastructura.Repositorios;
using EventsService.Infrastructura.mongo;
using Mongo2Go;
using MongoDB.Driver;
using Xunit;

namespace EventsService.Tests.Infraestructura.Repositorios
{
    public class ZonaEventoRepositoryTests : IDisposable
    {
        private readonly MongoDbRunner _runner;
        private readonly IMongoDatabase _db;
        private readonly ZonaEventoRepository _sut;

        public ZonaEventoRepositoryTests()
        {
            // Configuración de mapeos Mongo
            MongoMappings.Configure();

            // Mongo embebido
            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            _db = client.GetDatabase("events_service_zonaevento_tests");

            _sut = new ZonaEventoRepository(_db);
        }

        [Fact]
        public async Task AddAsync_Then_GetAsync_Should_Return_Zone()
        {
            var ct = CancellationToken.None;
            var eventId = Guid.NewGuid();
            var zona = CrearZonaEvento(eventId, "VIP");

            await _sut.AddAsync(zona, ct);

            var loaded = await _sut.GetAsync(eventId, zona.Id, ct);

            Assert.NotNull(loaded);
            Assert.Equal(zona.Id, loaded!.Id);
            Assert.Equal(eventId, loaded.EventId);
            Assert.Equal("VIP", loaded.Nombre);
        }

        [Fact]
        public async Task GetAsync_Should_Return_Null_When_Not_Found()
        {
            var ct = CancellationToken.None;
            var eventId = Guid.NewGuid();
            var zonaId = Guid.NewGuid();

            var loaded = await _sut.GetAsync(eventId, zonaId, ct);

            Assert.Null(loaded);
        }

        [Fact]
        public async Task ListByEventAsync_Should_Return_Only_Zones_For_Event()
        {
            var ct = CancellationToken.None;
            var e1 = Guid.NewGuid();
            var e2 = Guid.NewGuid();

            await _sut.AddAsync(CrearZonaEvento(e1, "VIP"), ct);
            await _sut.AddAsync(CrearZonaEvento(e1, "General"), ct);
            await _sut.AddAsync(CrearZonaEvento(e2, "Otra Zona"), ct);

            var list = await _sut.ListByEventAsync(e1, ct);

            Assert.Equal(2, list.Count);
            Assert.All(list, z => Assert.Equal(e1, z.EventId));
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_Zone_And_Set_UpdatedAt()
        {
            var ct = CancellationToken.None;
            var eventId = Guid.NewGuid();
            var zona = CrearZonaEvento(eventId, "Lateral");
            await _sut.AddAsync(zona, ct);

            var originalUpdatedAt = zona.UpdatedAt;

            zona.Nombre = "Lateral Norte";

            await _sut.UpdateAsync(zona, ct);
            var loaded = await _sut.GetAsync(eventId, zona.Id, ct);

            Assert.NotNull(loaded);
            Assert.Equal("Lateral Norte", loaded!.Nombre);
            Assert.True(loaded.UpdatedAt > originalUpdatedAt);
        }

        [Fact]
        public async Task ExistsByNombreAsync_Should_Be_Case_Insensitive()
        {
            var ct = CancellationToken.None;
            var eventId = Guid.NewGuid();
            var zona = CrearZonaEvento(eventId, "Palco");
            await _sut.AddAsync(zona, ct);

            var existsLower = await _sut.ExistsByNombreAsync(eventId, "palco", ct);
            var existsUpper = await _sut.ExistsByNombreAsync(eventId, "PALCO", ct);

            Assert.True(existsLower);
            Assert.True(existsUpper);
        }

        [Fact]
        public async Task ExistsByNombreAsync_Should_Return_False_When_Not_Exists()
        {
            var ct = CancellationToken.None;
            var eventId = Guid.NewGuid();

            var exists = await _sut.ExistsByNombreAsync(eventId, "Inexistente", ct);

            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Only_Specific_Zone()
        {
            var ct = CancellationToken.None;
            var eventId = Guid.NewGuid();

            var z1 = CrearZonaEvento(eventId, "VIP");
            var z2 = CrearZonaEvento(eventId, "General");

            await _sut.AddAsync(z1, ct);
            await _sut.AddAsync(z2, ct);

            var deleted = await _sut.DeleteAsync(eventId, z1.Id, ct);

            var loadedZ1 = await _sut.GetAsync(eventId, z1.Id, ct);
            var loadedZ2 = await _sut.GetAsync(eventId, z2.Id, ct);

            Assert.True(deleted);
            Assert.Null(loadedZ1);
            Assert.NotNull(loadedZ2);
        }

        private static ZonaEvento CrearZonaEvento(Guid eventId, string nombre)
        {
            return new ZonaEvento
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Nombre = nombre,
                UpdatedAt = DateTime.UtcNow
                // agrega aquí otras props obligatorias si tu entidad las tiene
            };
        }

        public void Dispose()
        {
            _runner?.Dispose();
        }
    }
}
