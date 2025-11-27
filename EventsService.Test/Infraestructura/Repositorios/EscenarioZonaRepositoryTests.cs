using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.ValueObjects;
using EventsService.Infrastructura.Repositorios;
using EventsService.Infrastructura.Interfaces;
using log4net;
using Moq;
using MongoDB.Driver;
using Xunit;

namespace EventsService.Test.Infraestructura.Repositories
{
    public class Repository_EscenarioZonaRepository_Tests
    {
        private readonly Mock<IMongoDatabase> _mockDb;
        private readonly Mock<IMongoCollection<EscenarioZona>> _mockEscenarioZonaCollection;
        private readonly Mock<IAuditoriaRepository> _mockAuditoria;
        private readonly Mock<ILog> _mockLogger;

        private readonly EscenarioZonaRepository _repository;

        private readonly Guid _idEscenarioZonaTest = Guid.NewGuid();
        private readonly Guid _idEventoTest = Guid.NewGuid();
        private readonly Guid _idZonaEventoTest = Guid.NewGuid();

        private readonly EscenarioZona _escenarioZonaTest;
        private readonly GridRef _gridTest;

        public Repository_EscenarioZonaRepository_Tests()
        {
            _mockDb = new Mock<IMongoDatabase>();
            _mockEscenarioZonaCollection = new Mock<IMongoCollection<EscenarioZona>>();
            _mockAuditoria = new Mock<IAuditoriaRepository>();
            _mockLogger = new Mock<ILog>();

            // Configurar la colección que el repositorio va a pedir en el constructor
            _mockDb.Setup(d =>
                    d.GetCollection<EscenarioZona>("escenario_zona", It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockEscenarioZonaCollection.Object);

            _repository = new EscenarioZonaRepository(
                _mockDb.Object,
                _mockAuditoria.Object,
                _mockLogger.Object);

            // Instancia de EscenarioZona de prueba (ajusta según tu dominio)
            _escenarioZonaTest = Activator.CreateInstance<EscenarioZona>();
            typeof(EscenarioZona).GetProperty("Id")?.SetValue(_escenarioZonaTest, _idEscenarioZonaTest);
            typeof(EscenarioZona).GetProperty("EventId")?.SetValue(_escenarioZonaTest, _idEventoTest);
            typeof(EscenarioZona).GetProperty("ZonaEventoId")?.SetValue(_escenarioZonaTest, _idZonaEventoTest);

            _gridTest = new GridRef
            {
                StartRow = 1,
                StartCol = 2,
                RowSpan = 3,
                ColSpan = 4
            };
        }

        #region AddAsync_InvocacionExitosa_DebeInsertarYRegistrarAuditoria
        [Fact]
        public async Task AddAsync_InvocacionExitosa_DebeInsertarYRegistrarAuditoria()
        {
            // Arrange
            _mockEscenarioZonaCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<EscenarioZona>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.AddAsync(_escenarioZonaTest, CancellationToken.None);

            // Assert: InsertOneAsync se llamó una sola vez con el mismo objeto
            _mockEscenarioZonaCollection.Verify(c => c.InsertOneAsync(
                    It.Is<EscenarioZona>(e => e.Id == _idEscenarioZonaTest),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // CreatedAt y UpdatedAt deberían estar seteados (no en MinValue)
            Assert.NotEqual(default, _escenarioZonaTest.CreatedAt);
            Assert.NotEqual(default, _escenarioZonaTest.UpdatedAt);

            // Auditoría
            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _idEscenarioZonaTest.ToString(),
                    "INFO",
                    "ESCENARIO_ZONA_CREADA",
                    It.Is<string>(m => m.Contains(_idEventoTest.ToString())
                                    && m.Contains(_idZonaEventoTest.ToString()))),
                Times.Once);

            // Log
            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("EscenarioZona creada"))),
                Times.Once);
        }
        #endregion

        #region AddAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task AddAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en InsertOneAsync");

            _mockEscenarioZonaCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<EscenarioZona>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.AddAsync(_escenarioZonaTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al crear EscenarioZona")),
                    ex),
                Times.Once);
        }
        #endregion

        #region UpdateGridAsync_ActualizacionExitosa_DebeLoggearYAuditar
        [Fact]
        public async Task UpdateGridAsync_ActualizacionExitosa_DebeLoggearYAuditar()
        {
            // Arrange
            var updateResultMock = new Mock<UpdateResult>();
            updateResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            updateResultMock.SetupGet(r => r.ModifiedCount).Returns(1);

            _mockEscenarioZonaCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<EscenarioZona>>(),
                    It.IsAny<UpdateDefinition<EscenarioZona>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResultMock.Object);

            // Act
            await _repository.UpdateGridAsync(
                _idEscenarioZonaTest,
                _gridTest,
                color: "#FFFFFF",
                zIndex: 10,
                visible: true,
                ct: CancellationToken.None);

            // Assert
            _mockEscenarioZonaCollection.Verify(c => c.UpdateOneAsync(
                    It.Is<FilterDefinition<EscenarioZona>>(f => f != null),
                    It.Is<UpdateDefinition<EscenarioZona>>(u => u != null),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("EscenarioZona grid actualizado"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _idEscenarioZonaTest.ToString(),
                    "INFO",
                    "ESCENARIO_ZONA_GRID_ACTUALIZADO",
                    It.Is<string>(m => m.Contains(_idEscenarioZonaTest.ToString()))),
                Times.Once);
        }
        #endregion

        #region UpdateGridAsync_SinCambios_DebeLoggearWarnSinAuditoria
        [Fact]
        public async Task UpdateGridAsync_SinCambios_DebeLoggearWarnSinAuditoria()
        {
            // Arrange
            var updateResultMock = new Mock<UpdateResult>();
            updateResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            updateResultMock.SetupGet(r => r.ModifiedCount).Returns(0);

            _mockEscenarioZonaCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<EscenarioZona>>(),
                    It.IsAny<UpdateDefinition<EscenarioZona>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResultMock.Object);

            // Act
            await _repository.UpdateGridAsync(
                _idEscenarioZonaTest,
                _gridTest,
                color: null,
                zIndex: null,
                visible: null,
                ct: CancellationToken.None);

            // Assert
            _mockLogger.Verify(l => l.Warn(
                    It.Is<string>(s => s.Contains("Intento de actualizar grid de EscenarioZona"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region UpdateGridAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task UpdateGridAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en UpdateOneAsync");

            _mockEscenarioZonaCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<EscenarioZona>>(),
                    It.IsAny<UpdateDefinition<EscenarioZona>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.UpdateGridAsync(
                    _idEscenarioZonaTest,
                    _gridTest,
                    ct: CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al actualizar grid de EscenarioZona")),
                    ex),
                Times.Once);
        }
        #endregion

        #region DeleteByZonaAsync_EliminacionExitosa_DebeRetornarTrueYAuditar
        [Fact]
        public async Task DeleteByZonaAsync_EliminacionExitosa_DebeRetornarTrueYAuditar()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(1);

            _mockEscenarioZonaCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<EscenarioZona>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var resultado = await _repository.DeleteByZonaAsync(
                _idEventoTest, _idZonaEventoTest, CancellationToken.None);

            // Assert
            Assert.True(resultado);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("EscenarioZona eliminado por zona"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _idZonaEventoTest.ToString(),
                    "INFO",
                    "ESCENARIO_ZONA_ELIMINADO_POR_ZONA",
                    It.Is<string>(m => m.Contains(_idEventoTest.ToString())
                                    && m.Contains(_idZonaEventoTest.ToString()))),
                Times.Once);
        }
        #endregion

        #region DeleteByZonaAsync_NoEncontrado_DebeRetornarFalseYLoggearWarn
        [Fact]
        public async Task DeleteByZonaAsync_NoEncontrado_DebeRetornarFalseYLoggearWarn()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(0);

            _mockEscenarioZonaCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<EscenarioZona>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var resultado = await _repository.DeleteByZonaAsync(
                _idEventoTest, _idZonaEventoTest, CancellationToken.None);

            // Assert
            Assert.False(resultado);

            _mockLogger.Verify(l => l.Warn(
                    It.Is<string>(s => s.Contains("Intento de eliminar EscenarioZona por zona sin resultados"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region DeleteByZonaAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task DeleteByZonaAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en DeleteOneAsync");

            _mockEscenarioZonaCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<EscenarioZona>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.DeleteByZonaAsync(
                    _idEventoTest, _idZonaEventoTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al eliminar EscenarioZona por zona")),
                    ex),
                Times.Once);
        }
        #endregion

        #region DeleteAsync_EnvuelveDeleteByZonaAsync
        [Fact]
        public async Task DeleteAsync_EnvuelveDeleteByZonaAsync()
        {
            // Este método solo llama a DeleteByZonaAsync, así que verificamos el comportamiento observable
            // usando la colección mockeada igual que en el caso exitoso.

            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(1);

            _mockEscenarioZonaCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<EscenarioZona>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var resultado = await _repository.DeleteAsync(
                _idEventoTest, _idZonaEventoTest, CancellationToken.None);

            // Assert
            Assert.True(resultado);
        }
        #endregion

        #region GetByZonaAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task GetByZonaAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // No configuramos Find/FirstOrDefaultAsync para forzar una excepción interna
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _repository.GetByZonaAsync(
                    _idEventoTest,
                    _idZonaEventoTest,
                    CancellationToken.None));

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al obtener EscenarioZona por zona")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
        #endregion

        #region ListByEventAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task ListByEventAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _repository.ListByEventAsync(
                    _idEventoTest,
                    CancellationToken.None));

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al listar EscenarioZona por evento")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
        #endregion

    }
}
