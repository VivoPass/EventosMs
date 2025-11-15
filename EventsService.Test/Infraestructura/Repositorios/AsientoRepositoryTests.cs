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
    public class AsientoRepositoryTests : IDisposable
    {
        private readonly MongoDbRunner _runner;
        private readonly IMongoDatabase _db;
        private readonly AsientoRepository _sut;

        public AsientoRepositoryTests()
        {
            // Configurar mapeos de Mongo
            MongoMappings.Configure();

            // Mongo embebido para pruebas
            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            _db = client.GetDatabase("events_service_asiento_tests");

            _sut = new AsientoRepository(_db);
        }

        [Fact]
        public async Task InsertAsync_Then_GetByIdAsync_Should_Return_Seat()
        {
            var ct = CancellationToken.None;
            var asiento = CrearAsiento();

            await _sut.InsertAsync(asiento, ct);

            var loaded = await _sut.GetByIdAsync(asiento.Id, ct);

            Assert.NotNull(loaded);
            Assert.Equal(asiento.Id, loaded!.Id);
            Assert.Equal(asiento.Label, loaded.Label);
            Assert.Equal(asiento.Estado, loaded.Estado);
        }

        [Fact]
        public async Task BulkInsertAsync_Should_Insert_Multiple_Seats()
        {
            var ct = CancellationToken.None;
            var eId = Guid.NewGuid();
            var zId = Guid.NewGuid();

            var seats = new[]
            {
                CrearAsiento(eId, zId, "A1"),
                CrearAsiento(eId, zId, "A2"),
                CrearAsiento(eId, zId, "A3")
            };

            await _sut.BulkInsertAsync(seats, ct);

            var list = await _sut.ListByZonaAsync(eId, zId, ct);

            Assert.Equal(3, list.Count);
        }

        [Fact]
        public async Task ListByZonaAsync_Should_Return_Only_Matching_Zone()
        {
            var ct = CancellationToken.None;
            var e1 = Guid.NewGuid();
            var z1 = Guid.NewGuid();
            var e2 = Guid.NewGuid();
            var z2 = Guid.NewGuid();

            await _sut.InsertAsync(CrearAsiento(e1, z1, "A1"), ct);
            await _sut.InsertAsync(CrearAsiento(e1, z1, "A2"), ct);
            await _sut.InsertAsync(CrearAsiento(e2, z2, "B1"), ct);

            var list = await _sut.ListByZonaAsync(e1, z1, ct);

            Assert.Equal(2, list.Count);
            Assert.All(list, x =>
            {
                Assert.Equal(e1, x.EventId);
                Assert.Equal(z1, x.ZonaEventoId);
            });
        }

        [Fact]
        public async Task DeleteDisponiblesByZonaAsync_Should_Delete_Only_Disponibles()
        {
            var ct = CancellationToken.None;
            var eId = Guid.NewGuid();
            var zId = Guid.NewGuid();

            await _sut.InsertAsync(CrearAsiento(eId, zId, "A1", "disponible"), ct);
            await _sut.InsertAsync(CrearAsiento(eId, zId, "A2", "disponible"), ct);
            await _sut.InsertAsync(CrearAsiento(eId, zId, "A3", "ocupado"), ct);

            var deleted = await _sut.DeleteDisponiblesByZonaAsync(eId, zId, ct);
            var remaining = await _sut.ListByZonaAsync(eId, zId, ct);

            Assert.Equal(2, deleted);
            Assert.Single(remaining);
            Assert.Equal("ocupado", remaining[0].Estado);
        }

        [Fact]
        public async Task AnyByZonaAsync_Should_Return_True_When_Seats_Exist()
        {
            var ct = CancellationToken.None;
            var eId = Guid.NewGuid();
            var zId = Guid.NewGuid();

            await _sut.InsertAsync(CrearAsiento(eId, zId, "A1"), ct);

            var any = await _sut.AnyByZonaAsync(eId, zId, ct);

            Assert.True(any);
        }

        [Fact]
        public async Task AnyByZonaAsync_Should_Return_False_When_No_Seats()
        {
            var ct = CancellationToken.None;
            var eId = Guid.NewGuid();
            var zId = Guid.NewGuid();

            var any = await _sut.AnyByZonaAsync(eId, zId, ct);

            Assert.False(any);
        }

        [Fact]
        public async Task DeleteByZonaAsync_Should_Delete_All_Seats_In_Zone()
        {
            var ct = CancellationToken.None;
            var eId = Guid.NewGuid();
            var zId = Guid.NewGuid();

            await _sut.InsertAsync(CrearAsiento(eId, zId, "A1"), ct);
            await _sut.InsertAsync(CrearAsiento(eId, zId, "A2"), ct);

            var deletedCount = await _sut.DeleteByZonaAsync(eId, zId, ct);
            var remaining = await _sut.ListByZonaAsync(eId, zId, ct);

            Assert.Equal(2, deletedCount);
            Assert.Empty(remaining);
        }

        [Fact]
        public async Task GetByCompositeAsync_Should_Return_Correct_Seat()
        {
            var ct = CancellationToken.None;
            var eId = Guid.NewGuid();
            var zId = Guid.NewGuid();

            var a1 = CrearAsiento(eId, zId, "A1");
            var a2 = CrearAsiento(eId, zId, "A2");

            await _sut.InsertAsync(a1, ct);
            await _sut.InsertAsync(a2, ct);

            var loaded = await _sut.GetByCompositeAsync(eId, zId, "A2", ct);

            Assert.NotNull(loaded);
            Assert.Equal("A2", loaded!.Label);
        }

        [Fact]
        public async Task UpdateParcialAsync_Should_Update_Selected_Fields()
        {
            var ct = CancellationToken.None;
            var seat = CrearAsiento();
            await _sut.InsertAsync(seat, ct);

            var nuevaMeta = new Dictionary<string, string>
            {
                ["color"] = "rojo",
                ["fila"] = "A"
            };

            var updated = await _sut.UpdateParcialAsync(
                seat.Id,
                nuevoLabel: "B5",
                nuevoEstado: "reservado",
                nuevaMeta: nuevaMeta,
                ct: ct);

            var loaded = await _sut.GetByIdAsync(seat.Id, ct);

            Assert.True(updated);
            Assert.NotNull(loaded);
            Assert.Equal("B5", loaded!.Label);
            Assert.Equal("reservado", loaded.Estado);
            Assert.Equal("rojo", loaded.Meta["color"]);
            Assert.Equal("A", loaded.Meta["fila"]);
            Assert.True(loaded.UpdatedAt > seat.UpdatedAt);
        }

        [Fact]
        public async Task DeleteByIdAsync_Should_Delete_Seat()
        {
            var ct = CancellationToken.None;
            var seat = CrearAsiento();
            await _sut.InsertAsync(seat, ct);

            var deleted = await _sut.DeleteByIdAsync(seat.Id, ct);
            var loaded = await _sut.GetByIdAsync(seat.Id, ct);

            Assert.True(deleted);
            Assert.Null(loaded);
        }

        private static Asiento CrearAsiento()
        {
            return CrearAsiento(Guid.NewGuid(), Guid.NewGuid(), "A1");
        }

        private static Asiento CrearAsiento(Guid eventId, Guid zonaId, string label, string estado = "disponible")
        {
            return new Asiento
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                ZonaEventoId = zonaId,
                Label = label,
                Estado = estado,
                Meta = new Dictionary<string, string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Dispose()
        {
            _runner?.Dispose();
        }
    }
}
