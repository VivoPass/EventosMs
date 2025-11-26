//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using EventsService.Dominio.Entidades;
//using EventsService.Infraestructura.Repositories;
//using EventsService.Infrastructura.mongo;
//using MongoDB.Driver;
//using Moq;
//using Xunit;

//namespace EventsService.Tests.Infraestructura.Repositories
//{
//    public class Repository_EventRepositoryMongo_Tests
//    {
//        private readonly Mock<IMongoDatabase> _mockDb;
//        private readonly Mock<IMongoCollection<Evento>> _mockEventosCollection;
//        private readonly EventRepositoryMongo _repository;

//        // --- DATOS DE PRUEBA ---
//        private readonly Evento _evento1;
//        private readonly Evento _evento2;
//        private readonly List<Evento> _listaEventos;

//        public Repository_EventRepositoryMongo_Tests()
//        {
//            _mockDb = new Mock<IMongoDatabase>();
//            _mockEventosCollection = new Mock<IMongoCollection<Evento>>();

//            // IMPORTANTE: debe coincidir el nombre de colección que uses en EventCollections
//            _mockDb
//                .Setup(d => d.GetCollection<Evento>("eventos", It.IsAny<MongoCollectionSettings>()))
//                .Returns(_mockEventosCollection.Object);

//            var collections = new EventCollections(_mockDb.Object);
//            _repository = new EventRepositoryMongo(collections);

//            _evento1 = CrearEventoDePrueba();
//            _evento2 = CrearEventoDePrueba();
//            _evento2.Id = Guid.NewGuid();
//            _evento2.Nombre = "Otro evento";

//            _listaEventos = new List<Evento> { _evento1, _evento2 };
//        }

//        private static Evento CrearEventoDePrueba()
//        {
//            return new Evento
//            {
//                Id = Guid.NewGuid(),
//                Nombre = "Concierto Sinfónico",
//                CategoriaId = Guid.NewGuid(),
//                EscenarioId = Guid.NewGuid(),
//                OrganizadorId = Guid.NewGuid(),
//                Inicio = DateTimeOffset.UtcNow.AddDays(7),
//                Fin = DateTimeOffset.UtcNow.AddDays(7).AddHours(2),
//                AforoMaximo = 500,
//                Estado = "Draft",
//                Tipo = "Concierto",
//                Lugar = "Teatro Municipal",
//                Descripcion = "Evento de prueba para tests unitarios."
//            };
//        }

//        #region InsertAsync_InvocacionExitosa_DebeLlamarInsertOneAsyncUnaVez
//        [Fact]
//        public async Task InsertAsync_InvocacionExitosa_DebeLlamarInsertOneAsyncUnaVez()
//        {
//            // Arrange
//            _mockEventosCollection
//                .Setup(c => c.InsertOneAsync(
//                    It.IsAny<Evento>(),
//                    It.IsAny<InsertOneOptions>(),
//                    It.IsAny<CancellationToken>()))
//                .Returns(Task.CompletedTask);

//            var ct = CancellationToken.None;

//            // Act
//            await _repository.InsertAsync(_evento1, ct);

//            // Assert
//            _mockEventosCollection.Verify(c => c.InsertOneAsync(
//                    It.Is<Evento>(e => e.Id == _evento1.Id),
//                    It.IsAny<InsertOneOptions>(),
//                    It.IsAny<CancellationToken>()),
//                Times.Once);
//        }
//        #endregion

//        #region GetByIdAsync_EventoEncontrado_DebeRetornarEvento
//        [Fact]
//        public async Task GetByIdAsync_EventoEncontrado_DebeRetornarEvento()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var cursorMock = new Mock<IAsyncCursor<Evento>>();
//            cursorMock
//                .SetupSequence(c => c.MoveNextAsync(ct))
//                .ReturnsAsync(true)
//                .ReturnsAsync(false);
//            cursorMock
//                .SetupGet(c => c.Current)
//                .Returns(new List<Evento> { _evento1 });

//            _mockEventosCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<FindOptions<Evento, Evento>>(),
//                    ct))
//                .ReturnsAsync(cursorMock.Object);

//            // Act
//            var result = await _repository.GetByIdAsync(_evento1.Id, ct);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(_evento1.Id, result!.Id);
//            Assert.Equal(_evento1.Nombre, result.Nombre);
//        }
//        #endregion

//        #region GetByIdAsync_EventoNoEncontrado_DebeRetornarNull
//        [Fact]
//        public async Task GetByIdAsync_EventoNoEncontrado_DebeRetornarNull()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var cursorMock = new Mock<IAsyncCursor<Evento>>();
//            cursorMock
//                .SetupSequence(c => c.MoveNextAsync(ct))
//                .ReturnsAsync(false);

//            _mockEventosCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<FindOptions<Evento, Evento>>(),
//                    ct))
//                .ReturnsAsync(cursorMock.Object);

//            // Act
//            var result = await _repository.GetByIdAsync(Guid.NewGuid(), ct);

//            // Assert
//            Assert.Null(result);
//        }
//        #endregion

//        #region GetAllAsync_DocumentosEncontrados_DebeRetornarListaEventos
//        [Fact]
//        public async Task GetAllAsync_DocumentosEncontrados_DebeRetornarListaEventos()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var cursorMock = new Mock<IAsyncCursor<Evento>>();
//            cursorMock
//                .SetupSequence(c => c.MoveNextAsync(ct))
//                .ReturnsAsync(true)
//                .ReturnsAsync(false);
//            cursorMock
//                .SetupGet(c => c.Current)
//                .Returns(_listaEventos);

//            _mockEventosCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<FindOptions<Evento, Evento>>(),
//                    ct))
//                .ReturnsAsync(cursorMock.Object);

//            // Act
//            var result = await _repository.GetAllAsync(ct);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(2, result.Count);
//            Assert.Contains(result, e => e.Id == _evento1.Id);
//            Assert.Contains(result, e => e.Id == _evento2.Id);
//        }
//        #endregion

//        #region GetAllAsync_ColeccionVacia_DebeRetornarListaVacia
//        [Fact]
//        public async Task GetAllAsync_ColeccionVacia_DebeRetornarListaVacia()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var cursorMock = new Mock<IAsyncCursor<Evento>>();
//            cursorMock
//                .SetupSequence(c => c.MoveNextAsync(ct))
//                .ReturnsAsync(false);

//            _mockEventosCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<FindOptions<Evento, Evento>>(),
//                    ct))
//                .ReturnsAsync(cursorMock.Object);

//            // Act
//            var result = await _repository.GetAllAsync(ct);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Empty(result);
//        }
//        #endregion

//        #region UpdateAsync_ActualizacionExitosa_DebeRetornarTrue
//        [Fact]
//        public async Task UpdateAsync_ActualizacionExitosa_DebeRetornarTrue()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var replaceResultMock = new Mock<ReplaceOneResult>();
//            replaceResultMock.SetupGet(r => r.MatchedCount).Returns(1);
//            replaceResultMock.SetupGet(r => r.ModifiedCount).Returns(1);
//            replaceResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);

//            _mockEventosCollection
//                .Setup(c => c.ReplaceOneAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<Evento>(),
//                    It.IsAny<ReplaceOptions>(),
//                    ct))
//                .ReturnsAsync(replaceResultMock.Object);

//            _evento1.Nombre = "Nombre Actualizado";
//            _evento1.Estado = "Published";

//            // Act
//            var updated = await _repository.UpdateAsync(_evento1, ct);

//            // Assert
//            Assert.True(updated);
//        }
//        #endregion

//        #region UpdateAsync_EventoNoEncontrado_DebeRetornarFalse
//        [Fact]
//        public async Task UpdateAsync_EventoNoEncontrado_DebeRetornarFalse()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var replaceResultMock = new Mock<ReplaceOneResult>();
//            replaceResultMock.SetupGet(r => r.MatchedCount).Returns(0);
//            replaceResultMock.SetupGet(r => r.ModifiedCount).Returns(0);
//            replaceResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);

//            _mockEventosCollection
//                .Setup(c => c.ReplaceOneAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<Evento>(),
//                    It.IsAny<ReplaceOptions>(),
//                    ct))
//                .ReturnsAsync(replaceResultMock.Object);

//            // Act
//            var updated = await _repository.UpdateAsync(_evento1, ct);

//            // Assert
//            Assert.False(updated);
//        }

//        #endregion

//        #region DeleteAsync_EliminacionExitosa_DebeRetornarTrue
//        [Fact]
//        public async Task DeleteAsync_EliminacionExitosa_DebeRetornarTrue()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var deleteResultMock = new Mock<DeleteResult>();
//            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(1);
//            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);

//            _mockEventosCollection
//                .Setup(c => c.DeleteOneAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    ct))
//                .ReturnsAsync(deleteResultMock.Object);

//            // Act
//            var deleted = await _repository.DeleteAsync(_evento1.Id, ct);

//            // Assert
//            Assert.True(deleted);
//        }

//        #endregion

//        #region DeleteAsync_EventoNoEncontrado_DebeRetornarFalse
//       [Fact]
//public async Task DeleteAsync_EventoNoEncontrado_DebeRetornarFalse()
//{
//    // Arrange
//    var ct = CancellationToken.None;

//    var deleteResultMock = new Mock<DeleteResult>();
//    deleteResultMock.SetupGet(r => r.DeletedCount).Returns(0);
//    deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);

//    _mockEventosCollection
//        .Setup(c => c.DeleteOneAsync(
//            It.IsAny<FilterDefinition<Evento>>(),
//            ct))
//        .ReturnsAsync(deleteResultMock.Object);

//    // Act
//    var deleted = await _repository.DeleteAsync(Guid.NewGuid(), ct);

//    // Assert
//    Assert.False(deleted);
//}

//        #endregion
//    }
//}
