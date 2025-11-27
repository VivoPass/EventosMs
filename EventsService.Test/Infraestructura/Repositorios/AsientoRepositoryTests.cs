using System;
using System.Collections.Generic;
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
    public class Repository_AsientoRepository_Tests
    {
        private readonly Mock<IMongoDatabase> _mockDb;
        private readonly Mock<IMongoCollection<Asiento>> _mockAsientoCollection;
        private readonly Mock<IAuditoriaRepository> _mockAuditoria;
        private readonly Mock<ILog> _mockLogger;

        private readonly AsientoRepository _repository;

        private readonly Guid _asientoIdTest = Guid.NewGuid();
        private readonly Guid _eventoIdTest = Guid.NewGuid();
        private readonly Guid _zonaEventoIdTest = Guid.NewGuid();

        private readonly Asiento _asientoTest;

        public Repository_AsientoRepository_Tests()
        {
            _mockDb = new Mock<IMongoDatabase>();
            _mockAsientoCollection = new Mock<IMongoCollection<Asiento>>();
            _mockAuditoria = new Mock<IAuditoriaRepository>();
            _mockLogger = new Mock<ILog>();

            _mockDb.Setup(d =>
                    d.GetCollection<Asiento>("asientos", It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockAsientoCollection.Object);

            _repository = new AsientoRepository(
                _mockDb.Object,
                _mockAuditoria.Object,
                _mockLogger.Object);

            // Instancia de prueba (ajusta si tu entidad no tiene ctor vacío)
            _asientoTest = Activator.CreateInstance<Asiento>();
            typeof(Asiento).GetProperty("Id")?.SetValue(_asientoTest, _asientoIdTest);
            typeof(Asiento).GetProperty("EventId")?.SetValue(_asientoTest, _eventoIdTest);
            typeof(Asiento).GetProperty("ZonaEventoId")?.SetValue(_asientoTest, _zonaEventoIdTest);
            typeof(Asiento).GetProperty("Label")?.SetValue(_asientoTest, "A-1");
            typeof(Asiento).GetProperty("Estado")?.SetValue(_asientoTest, "disponible");
        }

        #region InsertAsync_InvocacionExitosa_DebeInsertarYRegistrarAuditoria
        [Fact]
        public async Task InsertAsync_InvocacionExitosa_DebeInsertarYRegistrarAuditoria()
        {
            // Arrange
            _mockAsientoCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Asiento>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.InsertAsync(_asientoTest, CancellationToken.None);

            // Assert
            _mockAsientoCollection.Verify(c => c.InsertOneAsync(
                    It.Is<Asiento>(a => a.Id == _asientoIdTest),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.NotEqual(default, _asientoTest.CreatedAt);
            Assert.NotEqual(default, _asientoTest.UpdatedAt);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _asientoIdTest.ToString(),
                    "INFO",
                    "ASIENTO_CREADO",
                    It.Is<string>(m => m.Contains("A-1")
                                     && m.Contains(_eventoIdTest.ToString())
                                     && m.Contains(_zonaEventoIdTest.ToString()))),
                Times.Once);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("Asiento creado"))),
                Times.Once);
        }
        #endregion

        #region InsertAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task InsertAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en InsertOneAsync");

            _mockAsientoCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Asiento>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.InsertAsync(_asientoTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al crear asiento")),
                    ex),
                Times.Once);
        }
        #endregion

        #region BulkInsertAsync_ListaConDatos_DebeHacerBulkWriteYAuditar
        [Fact]
        public async Task BulkInsertAsync_ListaConDatos_DebeHacerBulkWriteYAuditar()
        {
            // Arrange
            var lista = new List<Asiento> { _asientoTest };

            _mockAsientoCollection
                .Setup(c => c.BulkWriteAsync(
                    It.IsAny<IEnumerable<WriteModel<Asiento>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((BulkWriteResult<Asiento>)null!); // el repo no inspecciona el resultado

            // Act
            await _repository.BulkInsertAsync(lista, CancellationToken.None);

            // Assert
            _mockAsientoCollection.Verify(c => c.BulkWriteAsync(
                    It.Is<IEnumerable<WriteModel<Asiento>>>(w => w != null),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _eventoIdTest.ToString(),
                    "INFO",
                    "ASIENTOS_BULK_CREADOS",
                    It.Is<string>(m => m.Contains(_eventoIdTest.ToString())
                                     && m.Contains(_zonaEventoIdTest.ToString()))),
                Times.Once);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("BulkInsertAsync -> Insertados '1' asientos"))),
                Times.Once);
        }
        #endregion

        #region BulkInsertAsync_ListaVacia_NoDebeLlamarBulkWriteNiAuditoria
        [Fact]
        public async Task BulkInsertAsync_ListaVacia_NoDebeLlamarBulkWriteNiAuditoria()
        {
            // Arrange
            var lista = new List<Asiento>();

            // Act
            await _repository.BulkInsertAsync(lista, CancellationToken.None);

            // Assert
            _mockAsientoCollection.Verify(c => c.BulkWriteAsync(
                    It.IsAny<IEnumerable<WriteModel<Asiento>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);

            _mockLogger.Verify(l => l.Debug(
                    It.Is<string>(s => s.Contains("BulkInsertAsync llamado sin asientos"))),
                Times.Once);
        }
        #endregion

        #region BulkInsertAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task BulkInsertAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var lista = new List<Asiento> { _asientoTest };
            var ex = new Exception("Error simulado en BulkWriteAsync");

            _mockAsientoCollection
                .Setup(c => c.BulkWriteAsync(
                    It.IsAny<IEnumerable<WriteModel<Asiento>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.BulkInsertAsync(lista, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error en BulkInsertAsync de asientos")),
                    ex),
                Times.Once);
        }
        #endregion

        #region DeleteDisponiblesByZonaAsync_EliminaAlgunos_DebeRetornarCantidadYAuditar
        [Fact]
        public async Task DeleteDisponiblesByZonaAsync_EliminaAlgunos_DebeRetornarCantidadYAuditar()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(5);

            _mockAsientoCollection
                .Setup(c => c.DeleteManyAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var deleted = await _repository.DeleteDisponiblesByZonaAsync(
                _eventoIdTest, _zonaEventoIdTest, CancellationToken.None);

            // Assert
            Assert.Equal(5, deleted);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("DeleteDisponiblesByZonaAsync -> Eliminados '5' asientos disponibles"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _zonaEventoIdTest.ToString(),
                    "INFO",
                    "ASIENTOS_DISPONIBLES_ELIMINADOS",
                    It.Is<string>(m => m.Contains("5")
                                     && m.Contains(_eventoIdTest.ToString())
                                     && m.Contains(_zonaEventoIdTest.ToString()))),
                Times.Once);
        }
        #endregion

        #region DeleteDisponiblesByZonaAsync_NoElimina_DebeRetornarCeroSinAuditoria
        [Fact]
        public async Task DeleteDisponiblesByZonaAsync_NoElimina_DebeRetornarCeroSinAuditoria()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(0);

            _mockAsientoCollection
                .Setup(c => c.DeleteManyAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var deleted = await _repository.DeleteDisponiblesByZonaAsync(
                _eventoIdTest, _zonaEventoIdTest, CancellationToken.None);

            // Assert
            Assert.Equal(0, deleted);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region DeleteDisponiblesByZonaAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task DeleteDisponiblesByZonaAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en DeleteManyAsync");

            _mockAsientoCollection
                .Setup(c => c.DeleteManyAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.DeleteDisponiblesByZonaAsync(
                    _eventoIdTest, _zonaEventoIdTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al eliminar asientos disponibles por zona")),
                    ex),
                Times.Once);
        }
        #endregion

        #region AnyByZonaAsync_Existe_DebeRetornarTrueYLoggearDebug
        [Fact]
        public async Task AnyByZonaAsync_Existe_DebeRetornarTrueYLoggearDebug()
        {
            // Arrange
            _mockAsientoCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(3L);

            // Act
            var existe = await _repository.AnyByZonaAsync(
                _eventoIdTest, _zonaEventoIdTest, CancellationToken.None);

            // Assert
            Assert.True(existe);

            _mockLogger.Verify(l => l.Debug(
                    It.Is<string>(s => s.Contains("AnyByZonaAsync ->"))),
                Times.Once);
        }
        #endregion

        #region AnyByZonaAsync_NoExiste_DebeRetornarFalse
        [Fact]
        public async Task AnyByZonaAsync_NoExiste_DebeRetornarFalse()
        {
            // Arrange
            _mockAsientoCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0L);

            // Act
            var existe = await _repository.AnyByZonaAsync(
                _eventoIdTest, _zonaEventoIdTest, CancellationToken.None);

            // Assert
            Assert.False(existe);
        }
        #endregion

        #region AnyByZonaAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task AnyByZonaAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en CountDocumentsAsync");

            _mockAsientoCollection
                .Setup(c => c.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.AnyByZonaAsync(
                    _eventoIdTest, _zonaEventoIdTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error en AnyByZonaAsync")),
                    ex),
                Times.Once);
        }
        #endregion

        #region DeleteByZonaAsync_Elimina_DebeRetornarCantidadYAuditar
        [Fact]
        public async Task DeleteByZonaAsync_Elimina_DebeRetornarCantidadYAuditar()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(7);

            _mockAsientoCollection
                .Setup(c => c.DeleteManyAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var deleted = await _repository.DeleteByZonaAsync(
                _eventoIdTest, _zonaEventoIdTest, CancellationToken.None);

            // Assert
            Assert.Equal(7, deleted);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("DeleteByZonaAsync -> Eliminados '7' asientos"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _zonaEventoIdTest.ToString(),
                    "INFO",
                    "ASIENTOS_ELIMINADOS_POR_ZONA",
                    It.Is<string>(m => m.Contains("7")
                                     && m.Contains(_eventoIdTest.ToString())
                                     && m.Contains(_zonaEventoIdTest.ToString()))),
                Times.Once);
        }
        #endregion

        #region DeleteByZonaAsync_NoElimina_DebeRetornarCeroSinAuditoria
        [Fact]
        public async Task DeleteByZonaAsync_NoElimina_DebeRetornarCeroSinAuditoria()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(0);

            _mockAsientoCollection
                .Setup(c => c.DeleteManyAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var deleted = await _repository.DeleteByZonaAsync(
                _eventoIdTest, _zonaEventoIdTest, CancellationToken.None);

            // Assert
            Assert.Equal(0, deleted);

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
            var ex = new Exception("Error simulado en DeleteManyAsync");

            _mockAsientoCollection
                .Setup(c => c.DeleteManyAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.DeleteByZonaAsync(
                    _eventoIdTest, _zonaEventoIdTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error en DeleteByZonaAsync")),
                    ex),
                Times.Once);
        }
        #endregion

        #region UpdateParcialAsync_Modifica_DebeRetornarTrueYAuditar
        [Fact]
        public async Task UpdateParcialAsync_Modifica_DebeRetornarTrueYAuditar()
        {
            // Arrange
            var updateResultMock = new Mock<UpdateResult>();
            updateResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            updateResultMock.SetupGet(r => r.ModifiedCount).Returns(1);

            _mockAsientoCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<UpdateDefinition<Asiento>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResultMock.Object);

            // Act
            var result = await _repository.UpdateParcialAsync(
                _asientoIdTest,
                "A-2",
                "ocupado",
                new Dictionary<string, string> { { "key", "value" } },
                CancellationToken.None);

            // Assert
            Assert.True(result);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("Asiento actualizado parcialmente"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _asientoIdTest.ToString(),
                    "INFO",
                    "ASIENTO_MODIFICADO",
                    It.Is<string>(m => m.Contains(_asientoIdTest.ToString()))),
                Times.Once);
        }
        #endregion

        #region UpdateParcialAsync_SinCambios_DebeRetornarFalseYLoggearWarn
        [Fact]
        public async Task UpdateParcialAsync_SinCambios_DebeRetornarFalseYLoggearWarn()
        {
            // Arrange
            var updateResultMock = new Mock<UpdateResult>();
            updateResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            updateResultMock.SetupGet(r => r.ModifiedCount).Returns(0);

            _mockAsientoCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<UpdateDefinition<Asiento>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResultMock.Object);

            // Act
            var result = await _repository.UpdateParcialAsync(
                _asientoIdTest,
                null,
                null,
                null,
                CancellationToken.None);

            // Assert
            Assert.False(result);

            _mockLogger.Verify(l => l.Warn(
                    It.Is<string>(s => s.Contains("Intento de actualización parcial sin cambios"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region UpdateParcialAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task UpdateParcialAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en UpdateOneAsync");

            _mockAsientoCollection
                .Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<UpdateDefinition<Asiento>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.UpdateParcialAsync(
                    _asientoIdTest,
                    "A-2",
                    null,
                    null,
                    CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error en UpdateParcialAsync")),
                    ex),
                Times.Once);
        }
        #endregion

        #region DeleteByIdAsync_Elimina_DebeRetornarTrueYAuditar
        [Fact]
        public async Task DeleteByIdAsync_Elimina_DebeRetornarTrueYAuditar()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(1);

            _mockAsientoCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var ok = await _repository.DeleteByIdAsync(
                _asientoIdTest, CancellationToken.None);

            // Assert
            Assert.True(ok);

            _mockLogger.Verify(l => l.Info(
                    It.Is<string>(s => s.Contains("Asiento eliminado"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    _asientoIdTest.ToString(),
                    "INFO",
                    "ASIENTO_ELIMINADO",
                    It.Is<string>(m => m.Contains(_asientoIdTest.ToString()))),
                Times.Once);
        }
        #endregion

        #region DeleteByIdAsync_NoElimina_DebeRetornarFalseYLoggearWarn
        [Fact]
        public async Task DeleteByIdAsync_NoElimina_DebeRetornarFalseYLoggearWarn()
        {
            // Arrange
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);
            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(0);

            _mockAsientoCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResultMock.Object);

            // Act
            var ok = await _repository.DeleteByIdAsync(
                _asientoIdTest, CancellationToken.None);

            // Assert
            Assert.False(ok);

            _mockLogger.Verify(l => l.Warn(
                    It.Is<string>(s => s.Contains("Intento de eliminar asiento sin resultados"))),
                Times.Once);

            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region DeleteByIdAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task DeleteByIdAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            // Arrange
            var ex = new Exception("Error simulado en DeleteOneAsync");

            _mockAsientoCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Asiento>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act & Assert
            var lanzada = await Assert.ThrowsAsync<Exception>(() =>
                _repository.DeleteByIdAsync(
                    _asientoIdTest, CancellationToken.None));

            Assert.Equal(ex, lanzada);

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error en DeleteByIdAsync")),
                    ex),
                Times.Once);
        }
        #endregion

                #region ListByZonaAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task ListByZonaAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _repository.ListByZonaAsync(
                    _eventoIdTest,
                    _zonaEventoIdTest,
                    CancellationToken.None));

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error al listar asientos por zona")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
        #endregion

        #region GetByCompositeAsync_FalloGeneral_DebeLoggearErrorYLanzar
        [Fact]
        public async Task GetByCompositeAsync_FalloGeneral_DebeLoggearErrorYLanzar()
        {
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _repository.GetByCompositeAsync(
                    _eventoIdTest,
                    _zonaEventoIdTest,
                    "A-1",
                    CancellationToken.None));

            await Assert.ThrowsAnyAsync<Exception>(() =>
                _repository.GetByIdAsync(
                    _asientoIdTest,
                    CancellationToken.None));

            _mockLogger.Verify(l => l.Error(
                    It.Is<string>(s => s.Contains("Error en GetByCompositeAsync")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
        #endregion


    }
}
