//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using EventsService.Dominio.Entidades;
//using EventsService.Dominio.Interfaces;
//using EventsService.Infraestructura.Repositories;
//using EventsService.Infrastructura.Interfaces;
//using EventsService.Infrastructura.mongo;
//using log4net;
//using MongoDB.Bson;
//using MongoDB.Driver;
//using Moq;
//using Xunit;

//namespace EventsService.Test.Infraestructura.Repositories
//{
//    public class EventRepositoryMongo_Tests
//    {
//        private readonly Mock<IMongoCollection<Evento>> _mockEventosCollection;
//        private readonly Mock<IAuditoriaRepository> _mockAuditoria;
//        private readonly Mock<ILog> _mockLog;
//        private readonly Mock<EventCollections> _mockCollections;
//        private readonly EventRepositoryMongo _repository;

//        private readonly Evento _eventoBase;

//        public EventRepositoryMongo_Tests()
//        {
//            _mockEventosCollection = new Mock<IMongoCollection<Evento>>();
//            _mockAuditoria = new Mock<IAuditoriaRepository>();
//            _mockLog = new Mock<ILog>();
//            _mockCollections = new Mock<EventCollections>();

//            // IMPORTANTE: esto asume que la propiedad Eventos es virtual o al menos tiene getter
//            _mockCollections
//                .SetupGet(c => c.Eventos)
//                .Returns(_mockEventosCollection.Object);

//            _repository = new EventRepositoryMongo(
//                _mockCollections.Object,
//                _mockAuditoria.Object,
//                _mockLog.Object);

//            _eventoBase = new Evento
//            {
//                Id = Guid.NewGuid(),
//                Nombre = "Concierto Prueba",
//                CategoriaId = Guid.NewGuid(),
//                EscenarioId = Guid.NewGuid(),
//                Inicio = DateTime.UtcNow.AddDays(1),
//                Fin = DateTime.UtcNow.AddDays(1).AddHours(2),
//                AforoMaximo = 100,
//                Tipo = "concierto",
//                Lugar = "Caracas",
//                Descripcion = "Evento de prueba",
//                OrganizadorId = Guid.NewGuid().ToString()
//            };
//        }

//        #region InsertAsync_Exito_DebeInsertarYAuditar
//        [Fact]
//        public async Task InsertAsync_Exito_DebeInsertarYAuditar()
//        {
//            // Arrange
//            _mockEventosCollection
//                .Setup(c => c.InsertOneAsync(
//                    It.IsAny<Evento>(),
//                    It.IsAny<InsertOneOptions>(),
//                    It.IsAny<CancellationToken>()))
//                .Returns(Task.CompletedTask);

//            _mockAuditoria
//                .Setup(a => a.InsertarAuditoriaEvento(
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>()))
//                .Returns(Task.CompletedTask);

//            // Act
//            await _repository.InsertAsync(_eventoBase, CancellationToken.None);

//            // Assert
//            _mockEventosCollection.Verify(c => c.InsertOneAsync(
//                    It.Is<Evento>(e => e.Id == _eventoBase.Id),
//                    It.IsAny<InsertOneOptions>(),
//                    It.IsAny<CancellationToken>()),
//                Times.Once);

//            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
//                    _eventoBase.Id.ToString(),
//                    "INFO",
//                    "EVENTO_CREADO",
//                    It.Is<string>(m => m.Contains(_eventoBase.Id.ToString()))),
//                Times.Once);
//        }
//        #endregion

//        #region GetByIdAsync_Encontrado_DebeRetornarEvento
//        [Fact]
//        public async Task GetByIdAsync_Encontrado_DebeRetornarEvento()
//        {
//            // Arrange
//            var findFluentMock = new Mock<IFindFluent<Evento, Evento>>();

//            findFluentMock
//                .Setup(f => f.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_eventoBase);

//            _mockEventosCollection
//                .Setup(c => c.Find(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<FindOptions<Evento, Evento>>()))
//                .Returns(findFluentMock.Object);

//            // Act
//            var result = await _repository.GetByIdAsync(_eventoBase.Id, CancellationToken.None);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(_eventoBase.Id, result!.Id);
//        }
//        #endregion

//        #region GetByIdAsync_NoEncontrado_DebeRetornarNull
//        [Fact]
//        public async Task GetByIdAsync_NoEncontrado_DebeRetornarNull()
//        {
//            // Arrange
//            var findFluentMock = new Mock<IFindFluent<Evento, Evento>>();

//            findFluentMock
//                .Setup(f => f.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Evento?)null);

//            _mockEventosCollection
//                .Setup(c => c.Find(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<FindOptions<Evento, Evento>>()))
//                .Returns(findFluentMock.Object);

//            // Act
//            var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

//            // Assert
//            Assert.Null(result);
//        }
//        #endregion

//        #region UpdateAsync_Modificado_DebeRetornarTrueYAuditar
//        [Fact]
//        public async Task UpdateAsync_Modificado_DebeRetornarTrueYAuditar()
//        {
//            // Arrange
//            var replaceResult = ReplaceOneResult.Acknowledged(
//                matchedCount: 1,
//                modifiedCount: 1,
//                upsertedId: BsonNull.Value);

//            _mockEventosCollection
//                .Setup(c => c.ReplaceOneAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<Evento>(),
//                    It.IsAny<ReplaceOptions>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(replaceResult);

//            _mockAuditoria
//                .Setup(a => a.InsertarAuditoriaEvento(
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>()))
//                .Returns(Task.CompletedTask);

//            // Act
//            var result = await _repository.UpdateAsync(_eventoBase, CancellationToken.None);

//            // Assert
//            Assert.True(result);

//            _mockEventosCollection.Verify(c => c.ReplaceOneAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.Is<Evento>(e => e.Id == _eventoBase.Id),
//                    It.IsAny<ReplaceOptions>(),
//                    It.IsAny<CancellationToken>()),
//                Times.Once);

//            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
//                    _eventoBase.Id.ToString(),
//                    "INFO",
//                    "EVENTO_MODIFICADO",
//                    It.Is<string>(m => m.Contains(_eventoBase.Id.ToString()))),
//                Times.Once);
//        }
//        #endregion

//        #region UpdateAsync_NoModificado_DebeRetornarFalseYNoAuditar
//        [Fact]
//        public async Task UpdateAsync_NoModificado_DebeRetornarFalseYNoAuditar()
//        {
//            // Arrange
//            var replaceResult = ReplaceOneResult.Acknowledged(
//                matchedCount: 0,
//                modifiedCount: 0,
//                upsertedId: null);

//            _mockEventosCollection
//                .Setup(c => c.ReplaceOneAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<Evento>(),
//                    It.IsAny<ReplaceOptions>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(replaceResult);

//            // Act
//            var result = await _repository.UpdateAsync(_eventoBase, CancellationToken.None);

//            // Assert
//            Assert.False(result);

//            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>()),
//                Times.Never);
//        }
//        #endregion

//        #region GetAllAsync_DevuelveLista_DebeRetornarEventos
//        [Fact]
//        public async Task GetAllAsync_DevuelveLista_DebeRetornarEventos()
//        {
//            // Arrange
//            var lista = new List<Evento>
//            {
//                _eventoBase,
//                new Evento
//                {
//                    Id = Guid.NewGuid(),
//                    Nombre = "Otro evento",
//                    CategoriaId = Guid.NewGuid(),
//                    EscenarioId = Guid.NewGuid(),
//                    Inicio = DateTime.UtcNow.AddDays(2),
//                    Fin = DateTime.UtcNow.AddDays(2).AddHours(3),
//                    AforoMaximo = 50,
//                    Tipo = "teatro",
//                    Lugar = "Madrid",
//                    Descripcion = "Otro",
//                    OrganizadorId = Guid.NewGuid().ToString()
//                }
//            };

//            var findFluentMock = new Mock<IFindFluent<Evento, Evento>>();
//            findFluentMock
//                .Setup(f => f.ToListAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(lista);

//            _mockEventosCollection
//                .Setup(c => c.Find(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<FindOptions<Evento, Evento>>()))
//                .Returns(findFluentMock.Object);

//            // Act
//            var result = await _repository.GetAllAsync(CancellationToken.None);

//            // Assert
//            Assert.Equal(2, result.Count);
//        }
//        #endregion

//        #region DeleteAsync_Elimina_DebeRetornarTrueYAuditar
//        [Fact]
//        public async Task DeleteAsync_Elimina_DebeRetornarTrueYAuditar()
//        {
//            // Arrange
//            var deleteResult = DeleteResult.Acknowledged(deletedCount: 1);

//            _mockEventosCollection
//                .Setup(c => c.DeleteOneAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(deleteResult);

//            _mockAuditoria
//                .Setup(a => a.InsertarAuditoriaEvento(
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>()))
//                .Returns(Task.CompletedTask);

//            var id = _eventoBase.Id;

//            // Act
//            var result = await _repository.DeleteAsync(id, CancellationToken.None);

//            // Assert
//            Assert.True(result);

//            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
//                    id.ToString(),
//                    "INFO",
//                    "EVENTO_ELIMINADO",
//                    It.Is<string>(m => m.Contains(id.ToString()))),
//                Times.Once);
//        }
//        #endregion

//        #region DeleteAsync_NoEncuentra_DebeRetornarFalseSinAuditar
//        [Fact]
//        public async Task DeleteAsync_NoEncuentra_DebeRetornarFalseSinAuditar()
//        {
//            // Arrange
//            var deleteResult = DeleteResult.Acknowledged(deletedCount: 0);

//            _mockEventosCollection
//                .Setup(c => c.DeleteOneAsync(
//                    It.IsAny<FilterDefinition<Evento>>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(deleteResult);

//            var id = Guid.NewGuid();

//            // Act
//            var result = await _repository.DeleteAsync(id, CancellationToken.None);

//            // Assert
//            Assert.False(result);

//            _mockAuditoria.Verify(a => a.InsertarAuditoriaEvento(
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>()),
//                Times.Never);
//        }
//        #endregion
//    }
//}
