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
    public class Repository_ScenarioRepositoryMongo_Tests
    {
        private readonly Mock<IMongoDatabase> _mockDb;
        private readonly Mock<IMongoCollection<Escenario>> _mockEscenariosCollection;
        private readonly Mock<IMongoCollection<Evento>> _mockEventosCollection;
        private readonly Mock<IMongoCollection<Categoria>> _mockCategoriasCollection;
        private readonly Mock<IAuditoriaRepository> _mockAuditoria;
        private readonly Mock<ILog> _mockLogger;

        private readonly EventCollections _collections;
        private readonly ScenarioRepositoryMongo _repository;

        private readonly Guid _escenarioIdTest = Guid.NewGuid();
        private readonly Escenario _escenarioTest;

        public Repository_ScenarioRepositoryMongo_Tests()
        {
            _mockDb = new Mock<IMongoDatabase>();
            _mockEscenariosCollection = new Mock<IMongoCollection<Escenario>>();
            _mockEventosCollection = new Mock<IMongoCollection<Evento>>();
            _mockCategoriasCollection = new Mock<IMongoCollection<Categoria>>();
            _mockAuditoria = new Mock<IAuditoriaRepository>();
            _mockLogger = new Mock<ILog>();

            // Configuración de colecciones que usa EventCollections
            _mockDb
                .Setup(d => d.GetCollection<Escenario>("escenarios", It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockEscenariosCollection.Object);

            _mockDb
                .Setup(d => d.GetCollection<Evento>("eventos", It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockEventosCollection.Object);

            _mockDb
                .Setup(d => d.GetCollection<Categoria>("categorias", It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockCategoriasCollection.Object);

            _collections = new EventCollections(_mockDb.Object);

            _repository = new ScenarioRepositoryMongo(
                _collections,
                _mockAuditoria.Object,
                _mockLogger.Object);

            // Crear un Escenario de prueba (ajusta si tu entidad no tiene set públicos)
            _escenarioTest = Activator.CreateInstance<Escenario>();

            typeof(Escenario).GetProperty("Id")?.SetValue(_escenarioTest, _escenarioIdTest);
            typeof(Escenario).GetProperty("Nombre")?.SetValue(_escenarioTest, "Escenario Test");
            typeof(Escenario).GetProperty("Descripcion")?.SetValue(_escenarioTest, "Descripcion test");
            typeof(Escenario).GetProperty("Ubicacion")?.SetValue(_escenarioTest, "Ciudad Test");
        }

        #region CrearAsync_InvocacionExitosa_DebeInsertarYRegistrarAuditoria
        [Fact]
        public async Task CrearAsync_InvocacionExitosa_DebeInsertarYRegistrarAuditoria()
        {
            // Arrange
            _mockEscenariosCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Escenario>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultadoId = await _repository.CrearAsync(_escenarioTest, CancellationToken.None);

            // Assert
            Assert.Equal(_escenarioIdTest.ToString(), resultadoId);

            _mockEscenariosCollection.Verify(c => c.InsertOneAsync(
                    It.Is<Escenario>(e => e.Id == _escenarioIdTest),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _escenarioIdTest.ToString(),
                    "INFO",
                    "ESCENARIO_CREADO",
                    It.Is<string>(m => m.Contains("Escenario Test"))),
                Times.Once);

            _mockLogger.Verify(l => l.Info(
                It.Is<string>(s => s.Contains("Escenario creado"))),
                Times.Once);
        }
        #endregion

        #region CrearAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task CrearAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en InsertOneAsync");

            _mockEscenariosCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Escenario>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.CrearAsync(_escenarioTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al crear escenario")),
                    ex),
                Times.Once);
        }
        #endregion

        #region EliminarEscenario_EliminacionExitosa_DebeLoggearYAuditar
        [Fact]
        public async Task EliminarEscenario_EliminacionExitosa_DebeLoggearYAuditar()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(1);

            _mockEscenariosCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Escenario>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            var idString = _escenarioIdTest.ToString();

            // Act
            await _repository.EliminarEscenario(idString, CancellationToken.None);

            // Assert
            _mockEscenariosCollection.Verify(c => c.DeleteOneAsync(
                    It.Is<FilterDefinition<Escenario>>(f => f != null),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("Escenario eliminado"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    idString,
                    "INFO",
                    "ESCENARIO_ELIMINADO",
                    It.Is<string>(m => m.Contains(idString))),
                Times.Once);
        }
        #endregion

        #region EliminarEscenario_NoEncontrado_DebeLoggearWarnSinAuditoria
        [Fact]
        public async Task EliminarEscenario_NoEncontrado_DebeLoggearWarnSinAuditoria()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(0);

            _mockEscenariosCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Escenario>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            var idString = _escenarioIdTest.ToString();

            // Act
            await _repository.EliminarEscenario(idString, CancellationToken.None);

            // Assert
            _mockLogger.Verify(l => l.Warn(
                    It.Is<string>(s => s.Contains("Intento de eliminar escenario"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region EliminarEscenario_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task EliminarEscenario_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en DeleteOneAsync");

            _mockEscenariosCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Escenario>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            var idString = _escenarioIdTest.ToString();

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.EliminarEscenario(idString, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al eliminar escenario")),
                    ex),
                Times.Once);
        }
        #endregion

        #region ExistsAsync_Existe_DebeRetornarTrue
        [Fact]
        public async Task ExistsAsync_Existe_DebeRetornarTrue()
        {
            // Arrange
            _mockEscenariosCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<Escenario>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1L);

            // Act
            var existe = await _repository.ExistsAsync(_escenarioIdTest, CancellationToken.None);

            // Assert
            Assert.True(existe);
        }
        #endregion

        #region ExistsAsync_NoExiste_DebeRetornarFalse
        [Fact]
        public async Task ExistsAsync_NoExiste_DebeRetornarFalse()
        {
            // Arrange
            _mockEscenariosCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<Escenario>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0L);

            // Act
            var existe = await _repository.ExistsAsync(_escenarioIdTest, CancellationToken.None);

            // Assert
            Assert.False(existe);
        }
        #endregion

        #region ExistsAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task ExistsAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en CountDocumentsAsync");

            _mockEscenariosCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<Escenario>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.ExistsAsync(_escenarioIdTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al validar existencia de escenario")),
                    ex),
                Times.Once);
        }
        #endregion

        #region ModificarEscenario_ActualizacionExitosa_DebeLoggearYAuditar
        [Fact]
        public async Task ModificarEscenario_ActualizacionExitosa_DebeLoggearYAuditar()
        {
            // Arrange
            var updateResultMock = new Mock<UpdateResult>();
            updateResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            updateResultMock.SetupGet(r => r.ModifiedCount).Returns(1);

            _mockEscenariosCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<Escenario>>(),
                    It.IsAny<UpdateDefinition<Escenario>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResultMock.Object);

            var idString = _escenarioIdTest.ToString();

            // Act
            await _repository.ModificarEscenario(idString, _escenarioTest, CancellationToken.None);

            // Assert
            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("Escenario actualizado"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    idString,
                    "INFO",
                    "ESCENARIO_MODIFICADO",
                    It.Is<string>(m => m.Contains(idString))),
                Times.Once);
        }
        #endregion

        #region ModificarEscenario_SinCambios_DebeLoggearWarnSinAuditoria
        [Fact]
        public async Task ModificarEscenario_SinCambios_DebeLoggearWarnSinAuditoria()
        {
            // Arrange
            var updateResultMock = new Mock<UpdateResult>();
            updateResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            updateResultMock.SetupGet(r => r.ModifiedCount).Returns(0);

            _mockEscenariosCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<Escenario>>(),
                    It.IsAny<UpdateDefinition<Escenario>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResultMock.Object);

            var idString = _escenarioIdTest.ToString();

            // Act
            await _repository.ModificarEscenario(idString, _escenarioTest, CancellationToken.None);

            // Assert
            _mockLogger.Verify(l => l.Warn(
                    It.Is<string>(s => s.Contains("Intento de modificar escenario"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region ModificarEscenario_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task ModificarEscenario_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en UpdateOneAsync");

            _mockEscenariosCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<Escenario>>(),
                    It.IsAny<UpdateDefinition<Escenario>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            var idString = _escenarioIdTest.ToString();

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.ModificarEscenario(idString, _escenarioTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al modificar escenario")),
                    ex),
                Times.Once);
        }
        #endregion

        #region ObtenerEscenario_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task ObtenerEscenario_FalloGeneral_DebeLoggearErrorYLanzar()
        {


            await Assert.ThrowsAnyAsync<Exception>(() =>
                _repository.ObtenerEscenario(
                    _escenarioIdTest.ToString(),
                    CancellationToken.None));

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al obtener escenario ID")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
        #endregion

        #region SearchAsync_FalloGeneralConFiltros_DebeLoggearErrorYLanzar
        [Fact]
        public async Task SearchAsync_FalloGeneralConFiltros_DebeLoggearErrorYLanzar()
        {
            // Usamos search, ciudad y activo para ejecutar los tres if de filtros
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _repository.SearchAsync(
                    search: "rock",
                    ciudad: "Caracas",
                    activo: true,
                    page: 1,
                    pageSize: 10,
                    ct: CancellationToken.None));

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al realizar búsqueda de escenarios")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
        #endregion
    }
}
