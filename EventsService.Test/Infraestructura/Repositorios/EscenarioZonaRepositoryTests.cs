//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using EventsService.Dominio.Entidades;
//using EventsService.Dominio.ValueObjects;
//using EventsService.Infrastructura.Repositorios;
//using MongoDB.Driver;
//using Moq;
//using Xunit;

//namespace EventsService.Tests.Infraestructura.Repositorios
//{
//    public class EscenarioZonaRepositoryTests
//    {
//        private readonly Mock<IMongoDatabase> _mockDb;
//        private readonly Mock<IMongoCollection<EscenarioZona>> _mockCollection;
//        private readonly EscenarioZonaRepository _sut;

//        private readonly EscenarioZona _ez1;
//        private readonly EscenarioZona _ez2;

//        public EscenarioZonaRepositoryTests()
//        {
//            _mockDb = new Mock<IMongoDatabase>();
//            _mockCollection = new Mock<IMongoCollection<EscenarioZona>>();

//            _mockDb
//                .Setup(d => d.GetCollection<EscenarioZona>("escenario_zona", It.IsAny<MongoCollectionSettings>()))
//                .Returns(_mockCollection.Object);

//            _sut = new EscenarioZonaRepository(_mockDb.Object);

//            var eventId = Guid.NewGuid();
//            var zonaId1 = Guid.NewGuid();
//            var zonaId2 = Guid.NewGuid();
//            var grid = new GridRef
//            {
//                StartRow = 1,
//                StartCol = 1,
//                RowSpan = 2,
//                ColSpan = 3
//            };

//            var grid1 = new GridRef
//            {
//                StartRow = 3,
//                StartCol = 4,
//                RowSpan = 1,
//                ColSpan = 2
//            };

//            _ez1 = new EscenarioZona
//            {
//                Id = Guid.NewGuid(),
//                EventId = eventId,
//                ZonaEventoId = zonaId1,
//                Grid = grid,
//                Color = "#FF0000",
//                ZIndex = 1,
//                Visible = true,
//                CreatedAt = DateTime.UtcNow
//            };

//            _ez2 = new EscenarioZona
//            {
//                Id = Guid.NewGuid(),
//                EventId = eventId,
//                ZonaEventoId = zonaId2,
//                Grid = grid1,
//                Color = "#00FF00",
//                ZIndex = 2,
//                Visible = true,
//                CreatedAt = DateTime.UtcNow
//            };
//        }

//        private static IAsyncCursor<T> BuildCursor<T>(List<T> docs)
//        {
//            var cursor = new Mock<IAsyncCursor<T>>();
//            var called = false;

//            cursor
//                .Setup(c => c.MoveNext(It.IsAny<CancellationToken>()))
//                .Returns(() =>
//                {
//                    if (!called)
//                    {
//                        called = true;
//                        return docs.Count > 0;
//                    }
//                    return false;
//                });

//            cursor
//                .Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(() =>
//                {
//                    if (!called)
//                    {
//                        called = true;
//                        return docs.Count > 0;
//                    }
//                    return false;
//                });

//            cursor.SetupGet(c => c.Current).Returns(docs);

//            return cursor.Object;
//        }

//        // -----------------------
//        // AddAsync
//        // -----------------------
//        [Fact]
//        public async Task AddAsync_Should_Call_InsertOneAsync()
//        {
//            _mockCollection
//                .Setup(c => c.InsertOneAsync(
//                    It.IsAny<EscenarioZona>(),
//                    It.IsAny<InsertOneOptions>(),
//                    It.IsAny<CancellationToken>()))
//                .Returns(Task.CompletedTask);

//            await _sut.AddAsync(_ez1);

//            _mockCollection.Verify(c => c.InsertOneAsync(
//                    It.Is<EscenarioZona>(e => e.Id == _ez1.Id),
//                    It.IsAny<InsertOneOptions>(),
//                    It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        // -----------------------
//        // GetByZonaAsync
//        // -----------------------
//        [Fact]
//        public async Task GetByZonaAsync_Should_Return_Entity_When_Found()
//        {
//            var cursor = BuildCursor(new List<EscenarioZona> { _ez1 });

//            _mockCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<EscenarioZona>>(),
//                    It.IsAny<FindOptions<EscenarioZona, EscenarioZona>>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(cursor);

//            var result = await _sut.GetByZonaAsync(_ez1.EventId, _ez1.ZonaEventoId);

//            Assert.NotNull(result);
//            Assert.Equal(_ez1.Id, result!.Id);
//        }

//        [Fact]
//        public async Task GetByZonaAsync_Should_Return_Null_When_Not_Found()
//        {
//            var cursor = BuildCursor(new List<EscenarioZona>());

//            _mockCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<EscenarioZona>>(),
//                    It.IsAny<FindOptions<EscenarioZona, EscenarioZona>>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(cursor);

//            var result = await _sut.GetByZonaAsync(Guid.NewGuid(), Guid.NewGuid());

//            Assert.Null(result);
//        }

//        // -----------------------
//        // ListByEventAsync
//        // -----------------------
//        [Fact]
//        public async Task ListByEventAsync_Should_Return_All_For_Event()
//        {
//            var cursor = BuildCursor(new List<EscenarioZona> { _ez1, _ez2 });

//            _mockCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<EscenarioZona>>(),
//                    It.IsAny<FindOptions<EscenarioZona, EscenarioZona>>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(cursor);

//            var result = await _sut.ListByEventAsync(_ez1.EventId);

//            Assert.Equal(2, result.Count);
//        }

//        // -----------------------
//        // UpdateGridAsync
//        // -----------------------
//        [Fact]
//        public async Task UpdateGridAsync_Should_Call_UpdateOneAsync()
//        {

//            var grid = new GridRef
//            {
//                StartRow = 5,
//                StartCol = 5,
//                RowSpan = 2,
//                ColSpan = 2
//            };
//            var updateResult = new Mock<UpdateResult>();
//            updateResult.SetupGet(r => r.ModifiedCount).Returns(1);

//            _mockCollection
//                .Setup(c => c.UpdateOneAsync(
//                    It.IsAny<FilterDefinition<EscenarioZona>>(),
//                    It.IsAny<UpdateDefinition<EscenarioZona>>(),
//                    It.IsAny<UpdateOptions>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(updateResult.Object);

//            var newGrid = grid;

//            await _sut.UpdateGridAsync(
//                escenarioZonaId: _ez1.Id,
//                grid: newGrid,
//                color: "#0000FF",
//                zIndex: 10,
//                visible: false,
//                ct: CancellationToken.None);

//            _mockCollection.Verify(c => c.UpdateOneAsync(
//                    It.IsAny<FilterDefinition<EscenarioZona>>(),
//                    It.IsAny<UpdateDefinition<EscenarioZona>>(),
//                    It.IsAny<UpdateOptions>(),
//                    It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        // -----------------------
//        // DeleteByZonaAsync
//        // -----------------------
//        [Fact]
//        public async Task DeleteByZonaAsync_Should_Return_True_When_Deleted()
//        {
//            var deleteResult = new Mock<DeleteResult>();
//            deleteResult.SetupGet(r => r.DeletedCount).Returns(1);

//            _mockCollection
//                .Setup(c => c.DeleteOneAsync(
//                    It.IsAny<FilterDefinition<EscenarioZona>>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(deleteResult.Object);

//            var result = await _sut.DeleteByZonaAsync(_ez1.EventId, _ez1.ZonaEventoId);

//            Assert.True(result);
//        }

//        [Fact]
//        public async Task DeleteByZonaAsync_Should_Return_False_When_Not_Deleted()
//        {
//            var deleteResult = new Mock<DeleteResult>();
//            deleteResult.SetupGet(r => r.DeletedCount).Returns(0);

//            _mockCollection
//                .Setup(c => c.DeleteOneAsync(
//                    It.IsAny<FilterDefinition<EscenarioZona>>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(deleteResult.Object);

//            var result = await _sut.DeleteByZonaAsync(_ez1.EventId, _ez1.ZonaEventoId);

//            Assert.False(result);
//        }
//    }
//}
