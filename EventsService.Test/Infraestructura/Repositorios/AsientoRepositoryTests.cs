using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Infrastructura.Repositorios;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace EventsService.Tests.Infraestructura.Repositorios
{
    public class AsientoRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _mockDb;
        private readonly Mock<IMongoCollection<Asiento>> _mockCollection;
        private readonly AsientoRepository _sut;

        private readonly Asiento _seat1;
        private readonly Asiento _seat2;

        public AsientoRepositoryTests()
        {
            _mockDb = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<Asiento>>();

            _mockDb
                .Setup(d => d.GetCollection<Asiento>("asiento", It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockCollection.Object);

            _sut = new AsientoRepository(_mockDb.Object);

            var eventId = Guid.NewGuid();
            var zonaId = Guid.NewGuid();

            _seat1 = new Asiento
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                ZonaEventoId = zonaId,
                Label = "A1",
                Estado = "disponible",
                Meta = new Dictionary<string, string> { { "row", "A" }, { "col", "1" } },
                CreatedAt = DateTime.UtcNow
            };

            _seat2 = new Asiento
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                ZonaEventoId = zonaId,
                Label = "A2",
                Estado = "ocupado",
                Meta = new Dictionary<string, string> { { "row", "A" }, { "col", "2" } },
                CreatedAt = DateTime.UtcNow
            };
        }

        private static IAsyncCursor<T> BuildCursor<T>(List<T> docs)
        {
            var cursor = new Mock<IAsyncCursor<T>>();
            var called = false;

            cursor
                .Setup(c => c.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    if (!called)
                    {
                        called = true;
                        return docs.Count > 0;
                    }
                    return false;
                });

            cursor
                .Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    if (!called)
                    {
                        called = true;
                        return docs.Count > 0;
                    }
                    return false;
                });

            cursor.SetupGet(c => c.Current).Returns(docs);

            return cursor.Object;
        }

        // ------------------------------------------------------
        // InsertAsync
        // ------------------------------------------------------
        [Fact]
        public async Task InsertAsync_Should_Call_InsertOneAsync()
        {
            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Asiento>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _sut.InsertAsync(_seat1);

            _mockCollection.Verify(c => c.InsertOneAsync(
                    It.Is<Asiento>(s => s.Id == _seat1.Id),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

       

        [Fact]
        public async Task BulkInsertAsync_Should_Not_Call_BulkWriteAsync_When_Empty()
        {
            await _sut.BulkInsertAsync(Array.Empty<Asiento>());

            _mockCollection.Verify(c => c.BulkWriteAsync(
                    It.IsAny<IEnumerable<WriteModel<Asiento>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ------------------------------------------------------
        // ListByZonaAsync
        // ------------------------------------------------------
        [Fact]
        public async Task ListByZonaAsync_Should_Return_Seats_For_Zona()
        {
            var cursor = BuildCursor(new List<Asiento> { _seat1, _seat2 });

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<FindOptions<Asiento, Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor);

            var result = await _sut.ListByZonaAsync(_seat1.EventId, _seat1.ZonaEventoId);

            Assert.Equal(2, result.Count);
        }

        // ------------------------------------------------------
        // DeleteDisponiblesByZonaAsync
        // ------------------------------------------------------
        [Fact]
        public async Task DeleteDisponiblesByZonaAsync_Should_Return_DeletedCount()
        {
            var deleteResult = new Mock<DeleteResult>();
            deleteResult.SetupGet(r => r.DeletedCount).Returns(5);

            _mockCollection
                .Setup(c => c.DeleteManyAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResult.Object);

            var deleted = await _sut.DeleteDisponiblesByZonaAsync(_seat1.EventId, _seat1.ZonaEventoId);

            Assert.Equal(5, deleted);
        }

        // ------------------------------------------------------
        // AnyByZonaAsync
        // ------------------------------------------------------
        [Fact]
        public async Task AnyByZonaAsync_Should_Return_True_When_Exists()
        {
            _mockCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(3L);

            var exists = await _sut.AnyByZonaAsync(_seat1.EventId, _seat1.ZonaEventoId);

            Assert.True(exists);
        }

        [Fact]
        public async Task AnyByZonaAsync_Should_Return_False_When_Not_Exists()
        {
            _mockCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0L);

            var exists = await _sut.AnyByZonaAsync(_seat1.EventId, _seat1.ZonaEventoId);

            Assert.False(exists);
        }

        // ------------------------------------------------------
        // DeleteByZonaAsync
        // ------------------------------------------------------
        [Fact]
        public async Task DeleteByZonaAsync_Should_Return_DeletedCount()
        {
            var deleteResult = new Mock<DeleteResult>();
            deleteResult.SetupGet(r => r.DeletedCount).Returns(7);

            _mockCollection
                .Setup(c => c.DeleteManyAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResult.Object);

            var deleted = await _sut.DeleteByZonaAsync(_seat1.EventId, _seat1.ZonaEventoId);

            Assert.Equal(7, deleted);
        }

        // ------------------------------------------------------
        // GetByCompositeAsync
        // ------------------------------------------------------
        [Fact]
        public async Task GetByCompositeAsync_Should_Return_Seat_When_Found()
        {
            var cursor = BuildCursor(new List<Asiento> { _seat1 });

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<FindOptions<Asiento, Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor);

            var result = await _sut.GetByCompositeAsync(_seat1.EventId, _seat1.ZonaEventoId, _seat1.Label);

            Assert.NotNull(result);
            Assert.Equal(_seat1.Id, result!.Id);
        }

        [Fact]
        public async Task GetByCompositeAsync_Should_Return_Null_When_Not_Found()
        {
            var cursor = BuildCursor(new List<Asiento>());

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<FindOptions<Asiento, Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor);

            var result = await _sut.GetByCompositeAsync(Guid.NewGuid(), Guid.NewGuid(), "X1");

            Assert.Null(result);
        }

        // ------------------------------------------------------
        // GetByIdAsync
        // ------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_Should_Return_Seat_When_Found()
        {
            var cursor = BuildCursor(new List<Asiento> { _seat1 });

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<FindOptions<Asiento, Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor);

            var result = await _sut.GetByIdAsync(_seat1.Id);

            Assert.NotNull(result);
            Assert.Equal(_seat1.Id, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            var cursor = BuildCursor(new List<Asiento>());

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<FindOptions<Asiento, Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor);

            var result = await _sut.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        // ------------------------------------------------------
        // UpdateParcialAsync
        // ------------------------------------------------------
        [Fact]
        public async Task UpdateParcialAsync_Should_Return_True_When_Modified()
        {
            var updateResult = new Mock<UpdateResult>();
            updateResult.SetupGet(r => r.IsAcknowledged).Returns(true);
            updateResult.SetupGet(r => r.ModifiedCount).Returns(1);

            _mockCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<UpdateDefinition<Asiento>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResult.Object);

            var result = await _sut.UpdateParcialAsync(
                _seat1.Id,
                nuevoLabel: "B1",
                nuevoEstado: "reservado",
                nuevaMeta: new Dictionary<string, string> { { "row", "B" }, { "col", "1" } });

            Assert.True(result);
        }

        [Fact]
        public async Task UpdateParcialAsync_Should_Return_False_When_Not_Modified()
        {
            var updateResult = new Mock<UpdateResult>();
            updateResult.SetupGet(r => r.IsAcknowledged).Returns(true);
            updateResult.SetupGet(r => r.ModifiedCount).Returns(0);

            _mockCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<UpdateDefinition<Asiento>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResult.Object);

            var result = await _sut.UpdateParcialAsync(
                _seat1.Id,
                nuevoLabel: null,
                nuevoEstado: null,
                nuevaMeta: null);

            Assert.False(result);
        }

        // ------------------------------------------------------
        // DeleteByIdAsync
        // ------------------------------------------------------
        [Fact]
        public async Task DeleteByIdAsync_Should_Return_True_When_Deleted()
        {
            var deleteResult = new Mock<DeleteResult>();
            deleteResult.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResult.SetupGet(r => r.DeletedCount).Returns(1);

            _mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResult.Object);

            var result = await _sut.DeleteByIdAsync(_seat1.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteByIdAsync_Should_Return_False_When_Not_Deleted()
        {
            var deleteResult = new Mock<DeleteResult>();
            deleteResult.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResult.SetupGet(r => r.DeletedCount).Returns(0);

            _mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResult.Object);

            var result = await _sut.DeleteByIdAsync(_seat1.Id);

            Assert.False(result);
        }
    }
}
