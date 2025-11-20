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
    public class ZonaEventoRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _mockDb;
        private readonly Mock<IMongoCollection<ZonaEvento>> _mockCollection;
        private readonly ZonaEventoRepository _sut;

        private readonly ZonaEvento _zona1;
        private readonly ZonaEvento _zona2;

        public ZonaEventoRepositoryTests()
        {
            _mockDb = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<ZonaEvento>>();

            _mockDb
                .Setup(d => d.GetCollection<ZonaEvento>("zona_evento", It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockCollection.Object);

            _sut = new ZonaEventoRepository(_mockDb.Object);

            _zona1 = new ZonaEvento
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Nombre = "VIP",
                Capacidad = 100,
                CreatedAt = DateTime.UtcNow
            };

            _zona2 = new ZonaEvento
            {
                Id = Guid.NewGuid(),
                EventId = _zona1.EventId,
                Nombre = "General",
                Capacidad = 500,
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
        // AddAsync
        // ------------------------------------------------------
        [Fact]
        public async Task AddAsync_Should_Call_InsertOneAsync()
        {
            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<ZonaEvento>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _sut.AddAsync(_zona1);

            _mockCollection.Verify(c => c.InsertOneAsync(
                It.Is<ZonaEvento>(z => z.Id == _zona1.Id),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ------------------------------------------------------
        // GetAsync
        // ------------------------------------------------------
        [Fact]
        public async Task GetAsync_Should_Return_Zona_When_Found()
        {
            var cursor = BuildCursor(new List<ZonaEvento> { _zona1 });

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<FindOptions<ZonaEvento, ZonaEvento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor);

            var result = await _sut.GetAsync(_zona1.EventId, _zona1.Id);

            Assert.NotNull(result);
            Assert.Equal(_zona1.Id, result!.Id);
        }

        [Fact]
        public async Task GetAsync_Should_Return_Null_When_Not_Found()
        {
            var cursor = BuildCursor(new List<ZonaEvento>());

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<FindOptions<ZonaEvento, ZonaEvento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor);

            var result = await _sut.GetAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.Null(result);
        }

        // ------------------------------------------------------
        // ListByEventAsync
        // ------------------------------------------------------
        [Fact]
        public async Task ListByEventAsync_Should_Return_List()
        {
            var cursor = BuildCursor(new List<ZonaEvento> { _zona1, _zona2 });

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<FindOptions<ZonaEvento, ZonaEvento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor);

            var result = await _sut.ListByEventAsync(_zona1.EventId);

            Assert.Equal(2, result.Count);
        }

        // ------------------------------------------------------
        // UpdateAsync
        // ------------------------------------------------------
        [Fact]
        public async Task UpdateAsync_Should_Call_ReplaceOneAsync()
        {
            var replaceResult = new Mock<ReplaceOneResult>();
            replaceResult.Setup(r => r.IsAcknowledged).Returns(true);
            replaceResult.Setup(r => r.ModifiedCount).Returns(1);

            _mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<ZonaEvento>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(replaceResult.Object);

            await _sut.UpdateAsync(_zona1);

            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<ZonaEvento>>(),
                It.IsAny<ZonaEvento>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ------------------------------------------------------
        // ExistsByNombreAsync
        // ------------------------------------------------------
        [Fact]
        public async Task ExistsByNombreAsync_Should_Return_True()
        {
            _mockCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1L);

            var exists = await _sut.ExistsByNombreAsync(_zona1.EventId, _zona1.Nombre);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByNombreAsync_Should_Return_False()
        {
            _mockCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0L);

            var exists = await _sut.ExistsByNombreAsync(_zona1.EventId, "X");

            Assert.False(exists);
        }

        // ------------------------------------------------------
        // DeleteAsync
        // ------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Deleted()
        {
            var deleteResult = new Mock<DeleteResult>();
            deleteResult.Setup(r => r.DeletedCount).Returns(1);

            _mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResult.Object);

            var result = await _sut.DeleteAsync(_zona1.EventId, _zona1.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_Not_Deleted()
        {
            var deleteResult = new Mock<DeleteResult>();
            deleteResult.Setup(r => r.DeletedCount).Returns(0);

            _mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResult.Object);

            var result = await _sut.DeleteAsync(_zona1.EventId, _zona1.Id);

            Assert.False(result);
        }
    }
}
