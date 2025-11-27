using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Infraestructura.Repositories;
using EventsService.Infrastructura.mongo;
using log4net;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;
using EventsService.Infrastructura.Interfaces;

namespace EventsService.Test.Infraestructura.Repositories
{
    public class Repository_EventRepositoryMongo_Tests
    {
        private readonly Mock<IMongoDatabase> _mockDb;
        private readonly Mock<IMongoCollection<Evento>> _mockEventosCollection;
        private readonly Mock<IAuditoriaRepository> _mockAuditoria;
        private readonly Mock<ILog> _mockLogger;

        private readonly EventCollections _collections;
        private readonly EventRepositoryMongo _repository;

        private readonly Guid _eventoIdTest = Guid.NewGuid();
        private readonly Evento _eventoTest;

        public Repository_EventRepositoryMongo_Tests()
        {
            _mockDb = new Mock<IMongoDatabase>();
            _mockEventosCollection = new Mock<IMongoCollection<Evento>>();
            _mockAuditoria = new Mock<IAuditoriaRepository>();
            _mockLogger = new Mock<ILog>();

            // Cuando EventCollections pida la colección "eventos", devolvemos el mock
            _mockDb
                .Setup(d => d.GetCollection<Evento>("eventos", It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockEventosCollection.Object);

            _collections = new EventCollections(_mockDb.Object);
            _repository = new EventRepositoryMongo(_collections, _mockAuditoria.Object, _mockLogger.Object);

            // Crea un Evento de prueba – ajusta según tu entidad real
            _eventoTest = Activator.CreateInstance<Evento>();
            typeof(Evento).GetProperty("Id")?.SetValue(_eventoTest, _eventoIdTest);
        }

        // Helper para IFindFluent<Evento, Evento>
        private Mock<IFindFluent<Evento, Evento>> CrearFindFluentMock(Evento? eventoResultado)
        {
            var findFluentMock = new Mock<IFindFluent<Evento, Evento>>();

            if (eventoResultado != null)
            {
                findFluentMock
                    .Setup(f => f.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(eventoResultado);

                findFluentMock
                    .Setup(f => f.ToListAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Evento> { eventoResultado });
            }
            else
            {
                findFluentMock
                    .Setup(f => f.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Evento?)null);

                findFluentMock
                    .Setup(f => f.ToListAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Evento>());
            }

            return findFluentMock;
        }

        private IFindFluent<Evento, Evento> CrearFindFluentConEventos(List<Evento> eventos)
        {
            var cursorMock = new Mock<IAsyncCursor<Evento>>();

            // Secuencia de iteración del cursor
            if (eventos.Count > 0)
            {
                cursorMock
                    .SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                    .Returns(true)
                    .Returns(false);

                cursorMock
                    .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true)
                    .ReturnsAsync(false);
            }
            else
            {
                cursorMock
                    .Setup(c => c.MoveNext(It.IsAny<CancellationToken>()))
                    .Returns(false);

                cursorMock
                    .Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
            }

            cursorMock
                .SetupGet(c => c.Current)
                .Returns(eventos);

            var findFluentMock = new Mock<IFindFluent<Evento, Evento>>();

            // Lo importante: mockear ToCursorAsync, NO ToListAsync
            findFluentMock
                .As<IAsyncCursorSource<Evento>>()
                .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            return findFluentMock.Object;
        }

        private Mock<IAsyncCursor<Evento>> CrearCursorMock(List<Evento> eventos)
        {
            var cursorMock = new Mock<IAsyncCursor<Evento>>();

            if (eventos.Count > 0)
            {
                cursorMock
                    .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true)
                    .ReturnsAsync(false);
            }
            else
            {
                cursorMock
                    .Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
            }

            cursorMock
                .SetupGet(c => c.Current)
                .Returns(eventos);

            return cursorMock;
        }

        #region InsertAsync_InvocacionExitosa_DebeInsertarEventoYRegistrarAuditoria
        [Fact]
        public async Task InsertAsync_InvocacionExitosa_DebeInsertarEventoYRegistrarAuditoria()
        {
            // Arrange
            _mockEventosCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Evento>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.InsertAsync(_eventoTest, CancellationToken.None);

            // Assert
            _mockEventosCollection.Verify(c => c.InsertOneAsync(
                    It.Is<Evento>(e => e.Id == _eventoIdTest),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _eventoIdTest.ToString(),
                    "INFO",
                    "EVENTO_CREADO",
                    It.Is<string>(m => m.Contains(_eventoIdTest.ToString()))),
                Times.Once);

            _mockLogger.Verify(l => l.Info(It.Is<string>(s => s.Contains("Evento creado en MongoDB"))), Times.Once);
        }
        #endregion

        #region InsertAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task InsertAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en InsertOneAsync");

            _mockEventosCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Evento>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.InsertAsync(_eventoTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al crear evento")),
                    ex),
                Times.Once);
        }
        #endregion

        #region GetByIdAsync_EventoEncontrado_DebeRetornarEventoYLoggearDebug
        [Fact]
        public async Task GetByIdAsync_EventoEncontrado_DebeRetornarEventoYLoggearDebug()
        {
            // Arrange
            var cursorMock = CrearCursorMock(new List<Evento> { _eventoTest });

            _mockEventosCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Evento>>(),   // o Expression si quieres
                    It.IsAny<FindOptions<Evento, Evento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            // Act
            var resultado = await _repository.GetByIdAsync(_eventoIdTest, CancellationToken.None);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(_eventoIdTest, resultado!.Id);

            _mockLogger.Verify(l => l.Debug(
                    It.Is<string>(s => s.Contains("Buscando evento por ID"))),
                Times.Once);
        }

        #endregion

        #region GetByIdAsync_EventoNoEncontrado_DebeRetornarNullYLoggearInfo
        [Fact]
        public async Task GetByIdAsync_EventoNoEncontrado_DebeRetornarNullYLoggearInfo()
        {
            // Arrange
            var cursorMock = CrearCursorMock(new List<Evento>()); // vacío

            _mockEventosCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Evento>>(),
                    It.IsAny<FindOptions<Evento, Evento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            // Act
            var resultado = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.Null(resultado);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("No se encontró evento"))),
                Times.Once);
        }

        #endregion

        #region GetByIdAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task GetByIdAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en FindAsync");

            _mockEventosCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Evento>>(),
                    It.IsAny<FindOptions<Evento, Evento>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.GetByIdAsync(_eventoIdTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al obtener evento por ID")),
                    ex),
                Times.Once);
        }

        #endregion

        #region UpdateAsync_ActualizacionExitosa_DebeRetornarTrueYAuditar
        [Fact]
        public async Task UpdateAsync_ActualizacionExitosa_DebeRetornarTrueYAuditar()
        {
            // Arrange
            var replaceResultMock = new Mock<ReplaceOneResult>();
            replaceResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            replaceResultMock.SetupGet(r => r.ModifiedCount).Returns(1);

            _mockEventosCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Evento>>(),
                    It.IsAny<Evento>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(replaceResultMock.Object);

            // Act
            var resultado = await _repository.UpdateAsync(_eventoTest, CancellationToken.None);

            // Assert
            Assert.True(resultado);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _eventoIdTest.ToString(),
                    "INFO",
                    "EVENTO_MODIFICADO",
                    It.Is<string>(m => m.Contains(_eventoIdTest.ToString()))),
                Times.Once);

            _mockLogger.Verify(l => l.Info(
                It.Is<string>(s => s.Contains("Evento actualizado en MongoDB"))),
                Times.Once);
        }
        #endregion

        #region UpdateAsync_SinCambios_DebeRetornarFalseYLoggearWarn
        [Fact]
        public async Task UpdateAsync_SinCambios_DebeRetornarFalseYLoggearWarn()
        {
            // Arrange
            var replaceResultMock = new Mock<ReplaceOneResult>();
            replaceResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            replaceResultMock.SetupGet(r => r.ModifiedCount).Returns(0);

            _mockEventosCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Evento>>(),
                    It.IsAny<Evento>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(replaceResultMock.Object);

            // Act
            var resultado = await _repository.UpdateAsync(_eventoTest, CancellationToken.None);

            // Assert
            Assert.False(resultado);

            _mockLogger.Verify(l => l.Warn(
                It.Is<string>(s => s.Contains("Intento de actualizar evento"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region UpdateAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task UpdateAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en ReplaceOneAsync");

            _mockEventosCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Evento>>(),
                    It.IsAny<Evento>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.UpdateAsync(_eventoTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al actualizar evento")),
                    ex),
                Times.Once);
        }
        #endregion

        #region GetAllAsync_EventosEncontrados_DebeRetornarLista
        [Fact]
        public async Task GetAllAsync_EventosEncontrados_DebeRetornarLista()
        {
            // Arrange
            var listaEventos = new List<Evento> { _eventoTest };
            var cursorMock = CrearCursorMock(listaEventos);

            _mockEventosCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Evento>>(),
                    It.IsAny<FindOptions<Evento, Evento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            // Act
            var resultado = await _repository.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.Single(resultado);
            Assert.Equal(_eventoIdTest, resultado[0].Id);

        }


        #endregion

        #region DeleteAsync_EliminacionExitosa_DebeRetornarTrueYAuditar
        [Fact]
        public async Task DeleteAsync_EliminacionExitosa_DebeRetornarTrueYAuditar()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(1);

            _mockEventosCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Evento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var resultado = await _repository.DeleteAsync(_eventoIdTest, CancellationToken.None);

            // Assert
            Assert.True(resultado);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _eventoIdTest.ToString(),
                    "INFO",
                    "EVENTO_ELIMINADO",
                    It.Is<string>(m => m.Contains(_eventoIdTest.ToString()))),
                Times.Once);

            _mockLogger.Verify(l => l.Info(
                It.Is<string>(s => s.Contains("Evento eliminado en MongoDB"))),
                Times.Once);
        }
        #endregion

        #region DeleteAsync_NoEncontrado_DebeRetornarFalseYLoggearWarn
        [Fact]
        public async Task DeleteAsync_NoEncontrado_DebeRetornarFalseYLoggearWarn()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(0);

            _mockEventosCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Evento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var resultado = await _repository.DeleteAsync(_eventoIdTest, CancellationToken.None);

            // Assert
            Assert.False(resultado);

            _mockLogger.Verify(l => l.Warn(
                It.Is<string>(s => s.Contains("Intento de eliminar evento"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region DeleteAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task DeleteAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en DeleteOneAsync");

            _mockEventosCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Evento>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.DeleteAsync(_eventoIdTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al eliminar evento")),
                    ex),
                Times.Once);
        }
        #endregion
    }
}
