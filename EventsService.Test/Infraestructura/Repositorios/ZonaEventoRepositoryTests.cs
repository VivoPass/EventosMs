using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Infrastructura.Repositorios;
using EventsService.Infrastructura.Interfaces;
using log4net;
using Moq;
using MongoDB.Driver;
using Xunit;

namespace EventsService.Test.Infraestructura.Repositories
{
    public class Repository_ZonaEventoRepository_Tests
    {
        private readonly Mock<IMongoDatabase> _mockDb;
        private readonly Mock<IMongoCollection<ZonaEvento>> _mockZonaEventoCollection;
        private readonly Mock<IAuditoriaRepository> _mockAuditoria;
        private readonly Mock<ILog> _mockLogger;

        private readonly ZonaEventoRepository _repository;

        private readonly Guid _idZonaEventoTest = Guid.NewGuid();
        private readonly Guid _idEventoTest = Guid.NewGuid();

        private readonly ZonaEvento _zonaEventoTest;

        public Repository_ZonaEventoRepository_Tests()
        {
            _mockDb = new Mock<IMongoDatabase>();
            _mockZonaEventoCollection = new Mock<IMongoCollection<ZonaEvento>>();
            _mockAuditoria = new Mock<IAuditoriaRepository>();
            _mockLogger = new Mock<ILog>();

            _mockDb.Setup(d =>
                    d.GetCollection<ZonaEvento>("zona_evento", It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockZonaEventoCollection.Object);

            _repository = new ZonaEventoRepository(
                _mockDb.Object,
                _mockAuditoria.Object,
                _mockLogger.Object);

            // Instancia de prueba (ajusta según tus constructores/properties reales)
            _zonaEventoTest = Activator.CreateInstance<ZonaEvento>();
            typeof(ZonaEvento).GetProperty("Id")?.SetValue(_zonaEventoTest, _idZonaEventoTest);
            typeof(ZonaEvento).GetProperty("EventId")?.SetValue(_zonaEventoTest, _idEventoTest);
            typeof(ZonaEvento).GetProperty("Nombre")?.SetValue(_zonaEventoTest, "VIP");
        }

        #region AddAsync_InvocacionExitosa_DebeInsertarYRegistrarAuditoria
        [Fact]
        public async Task AddAsync_InvocacionExitosa_DebeInsertarYRegistrarAuditoria()
        {
            // Arrange
            _mockZonaEventoCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<ZonaEvento>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.AddAsync(_zonaEventoTest, CancellationToken.None);

            // Assert
            _mockZonaEventoCollection.Verify(c => c.InsertOneAsync(
                    It.Is<ZonaEvento>(z => z.Id == _idZonaEventoTest),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _idZonaEventoTest.ToString(),
                    "INFO",
                    "ZONA_EVENTO_CREADA",
                    It.Is<string>(m => m.Contains("VIP")
                                     && m.Contains(_idEventoTest.ToString()))),
                Times.Once);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("ZonaEvento creada"))),
                Times.Once);
        }
        #endregion

        #region AddAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task AddAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en InsertOneAsync");

            _mockZonaEventoCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<ZonaEvento>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.AddAsync(_zonaEventoTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al crear ZonaEvento")),
                    ex),
                Times.Once);
        }
        #endregion

        #region UpdateAsync_ActualizacionExitosa_DebeLoggearYAuditar
        [Fact]
        public async Task UpdateAsync_ActualizacionExitosa_DebeLoggearYAuditar()
        {
            // Arrange
            var replaceResultMock = new Mock<ReplaceOneResult>();
            replaceResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            replaceResultMock.SetupGet(r => r.ModifiedCount).Returns(1);

            _mockZonaEventoCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<ZonaEvento>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(replaceResultMock.Object);

            // Act
            await _repository.UpdateAsync(_zonaEventoTest, CancellationToken.None);

            // Assert
            _mockZonaEventoCollection.Verify(c => c.ReplaceOneAsync(
                    It.Is<FilterDefinition<ZonaEvento>>(f => f != null),
                    It.Is<ZonaEvento>(z => z.Id == _idZonaEventoTest),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // UpdatedAt se seteó
            Assert.NotEqual(default, _zonaEventoTest.UpdatedAt);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("ZonaEvento actualizada"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _idZonaEventoTest.ToString(),
                    "INFO",
                    "ZONA_EVENTO_MODIFICADA",
                    It.Is<string>(m => m.Contains("VIP")
                                     && m.Contains(_idEventoTest.ToString()))),
                Times.Once);
        }
        #endregion

        #region UpdateAsync_SinCambios_DebeLoggearWarnSinAuditoria
        [Fact]
        public async Task UpdateAsync_SinCambios_DebeLoggearWarnSinAuditoria()
        {
            // Arrange
            var replaceResultMock = new Mock<ReplaceOneResult>();
            replaceResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            replaceResultMock.SetupGet(r => r.ModifiedCount).Returns(0);

            _mockZonaEventoCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<ZonaEvento>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(replaceResultMock.Object);

            // Act
            await _repository.UpdateAsync(_zonaEventoTest, CancellationToken.None);

            // Assert
            _mockLogger.Verify(l => l.Warn(
                    It.Is<string>(s => s.Contains("Intento de actualizar ZonaEvento"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region UpdateAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task UpdateAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en ReplaceOneAsync");

            _mockZonaEventoCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<ZonaEvento>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.UpdateAsync(_zonaEventoTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al actualizar ZonaEvento")),
                    ex),
                Times.Once);
        }
        #endregion

        #region ExistsByNombreAsync_Existe_DebeRetornarTrueYLoggearDebug
        [Fact]
        public async Task ExistsByNombreAsync_Existe_DebeRetornarTrueYLoggearDebug()
        {
            // Arrange
            _mockZonaEventoCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1L);

            // Act
            var existe = await _repository.ExistsByNombreAsync(
                _idEventoTest, "VIP", CancellationToken.None);

            // Assert
            Assert.True(existe);

            _mockLogger.Verify(l => l.Debug(
                    It.Is<string>(s => s.Contains("ExistsByNombreAsync"))),
                Times.Once);
        }
        #endregion

        #region ExistsByNombreAsync_NoExiste_DebeRetornarFalse
        [Fact]
        public async Task ExistsByNombreAsync_NoExiste_DebeRetornarFalse()
        {
            // Arrange
            _mockZonaEventoCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0L);

            // Act
            var existe = await _repository.ExistsByNombreAsync(
                _idEventoTest, "VIP", CancellationToken.None);

            // Assert
            Assert.False(existe);
        }
        #endregion

        #region ExistsByNombreAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task ExistsByNombreAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en CountDocumentsAsync");

            _mockZonaEventoCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.ExistsByNombreAsync(_idEventoTest, "VIP", CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al validar existencia de ZonaEvento por nombre")),
                    ex),
                Times.Once);
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

            _mockZonaEventoCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var resultado = await _repository.DeleteAsync(
                _idEventoTest, _idZonaEventoTest, CancellationToken.None);

            // Assert
            Assert.True(resultado);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("ZonaEvento eliminada"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _idZonaEventoTest.ToString(),
                    "INFO",
                    "ZONA_EVENTO_ELIMINADA",
                    It.Is<string>(m => m.Contains(_idEventoTest.ToString())
                                     && m.Contains(_idZonaEventoTest.ToString()))),
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

            _mockZonaEventoCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var resultado = await _repository.DeleteAsync(
                _idEventoTest, _idZonaEventoTest, CancellationToken.None);

            // Assert
            Assert.False(resultado);

            _mockLogger.Verify(l => l.Warn(
                    It.Is<string>(s => s.Contains("Intento de eliminar ZonaEvento sin resultados"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region DeleteAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task DeleteAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en DeleteOneAsync");

            _mockZonaEventoCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<ZonaEvento>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.DeleteAsync(
                    _idEventoTest, _idZonaEventoTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al eliminar ZonaEvento")),
                    ex),
                Times.Once);
        }
        #endregion

        #region GetAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task GetAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _repository.GetAsync(
                    _idEventoTest,
                    _idZonaEventoTest,
                    CancellationToken.None));

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al obtener ZonaEvento")),
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
                    It.Is<string>(s => s.Contains("Error al listar ZonasEvento para EventId")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
        #endregion

    }
}
