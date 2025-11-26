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
//    public class ScenarioRepositoryMongoTests
//    {
//        private readonly Mock<IMongoDatabase> _mockDb;
//        private readonly Mock<IMongoCollection<Escenario>> _mockCollection;
//        private readonly ScenarioRepositoryMongo _sut;
//        private readonly Escenario _e1;
//        private readonly Escenario _e2;
//        private readonly Escenario _escenario1;

//        public ScenarioRepositoryMongoTests()
//        {
//            _mockDb = new Mock<IMongoDatabase>();
//            _mockCollection = new Mock<IMongoCollection<Escenario>>();

//            // Debe coincidir con el nombre de colección que usa EventCollections
//            _mockDb
//                .Setup(d => d.GetCollection<Escenario>("escenarios", It.IsAny<MongoCollectionSettings>()))
//                .Returns(_mockCollection.Object);

//            var collections = new EventCollections(_mockDb.Object);
//            _sut = new ScenarioRepositoryMongo(collections);
//            _e1 = CrearEscenario("Teatro Municipal", "Caracas", true);
//            _e2 = CrearEscenario("Teatro Nacional", "Caracas", false);
//            _escenario1 = CrearEscenario("Escenario Principal", "Caracas", true);
//        }

//        private static Escenario CrearEscenario(string nombre, string ciudad, bool activo)
//        {
//            return new Escenario
//            {
//                Id = Guid.NewGuid(),
//                Nombre = nombre,
//                Descripcion = $"Descripción de {nombre}",
//                Ubicacion = "Ubicación X",
//                Ciudad = ciudad,
//                Activo = activo
//            };
//        }

//        // -----------------------
//        // CrearAsync
//        // -----------------------

//        [Fact]
//        public async Task CrearAsync_Should_Insert_Scenario_And_Return_Id()
//        {
//            // Arrange
//            _mockCollection
//                .Setup(c => c.InsertOneAsync(
//                    It.IsAny<Escenario>(),
//                    It.IsAny<InsertOneOptions>(),
//                    It.IsAny<CancellationToken>()))
//                .Returns(Task.CompletedTask);

//            var ct = CancellationToken.None;

//            // Act
//            var idString = await _sut.CrearAsync(_escenario1, ct);

//            // Assert
//            Assert.Equal(_escenario1.Id.ToString(), idString);

//            _mockCollection.Verify(c => c.InsertOneAsync(
//                    It.Is<Escenario>(e => e.Id == _escenario1.Id),
//                    It.IsAny<InsertOneOptions>(),
//                    It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        // -----------------------
//        // ExistsAsync
//        // -----------------------

//        [Fact]
//        public async Task ExistsAsync_Should_Return_True_When_Scenario_Exists()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            _mockCollection
//                .Setup(c => c.CountDocumentsAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<CountOptions>(),
//                    ct))
//                .ReturnsAsync(1L);

//            // Act
//            var exists = await _sut.ExistsAsync(_escenario1.Id, ct);

//            // Assert
//            Assert.True(exists);
//        }

//        [Fact]
//        public async Task ExistsAsync_Should_Return_False_When_Scenario_Does_Not_Exist()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            _mockCollection
//                .Setup(c => c.CountDocumentsAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<CountOptions>(),
//                    ct))
//                .ReturnsAsync(0L);

//            // Act
//            var exists = await _sut.ExistsAsync(Guid.NewGuid(), ct);

//            // Assert
//            Assert.False(exists);
//        }

//        // -----------------------
//        // ObtenerEscenario
//        // -----------------------

//        [Fact]
//        public async Task ObtenerEscenario_Should_Return_Scenario_When_Found()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var cursor = BuildCursor(new List<Escenario> { _escenario1 });

//            _mockCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<FindOptions<Escenario, Escenario>>(),
//                    ct))
//                .ReturnsAsync(cursor);

//            // Act
//            var result = await _sut.ObtenerEscenario(_escenario1.Id.ToString(), ct);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(_escenario1.Id, result!.Id);
//            Assert.Equal(_escenario1.Nombre, result.Nombre);
//        }

//        [Fact]
//        public async Task ObtenerEscenario_Should_Return_Null_When_Not_Found()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var cursor = BuildCursor(new List<Escenario>()); // vacío

//            _mockCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<FindOptions<Escenario, Escenario>>(),
//                    ct))
//                .ReturnsAsync(cursor);

//            // Act
//            var result = await _sut.ObtenerEscenario(Guid.NewGuid().ToString(), ct);

//            // Assert
//            Assert.Null(result);
//        }

//        // -----------------------
//        // ModificarEscenario
//        // -----------------------

//        [Fact]
//        public async Task ModificarEscenario_Should_Call_UpdateOneAsync()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var updateResultMock = new Mock<UpdateResult>();
//            updateResultMock.SetupGet(r => r.ModifiedCount).Returns(1);

//            _mockCollection
//                .Setup(c => c.UpdateOneAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<UpdateDefinition<Escenario>>(),
//                    It.IsAny<UpdateOptions>(),
//                    ct))
//                .ReturnsAsync(updateResultMock.Object);

//            var cambios = new Escenario
//            {
//                Id = _escenario1.Id,
//                Nombre = "Teatro Renovado",
//                Descripcion = "Actualizado",
//                Ubicacion = "Centro"
//            };

//            // Act
//            await _sut.ModificarEscenario(_escenario1.Id.ToString(), cambios, ct);

//            // Assert
//            _mockCollection.Verify(c => c.UpdateOneAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<UpdateDefinition<Escenario>>(),
//                    It.IsAny<UpdateOptions>(),
//                    ct),
//                Times.Once);
//        }

//        // -----------------------
//        // EliminarEscenario
//        // -----------------------

//        [Fact]
//        public async Task EliminarEscenario_Should_Call_DeleteOneAsync()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var deleteResultMock = new Mock<DeleteResult>();
//            deleteResultMock.SetupGet(r => r.DeletedCount).Returns(1);
//            deleteResultMock.SetupGet(r => r.IsAcknowledged).Returns(true);

//            _mockCollection
//                .Setup(c => c.DeleteOneAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    ct))
//                .ReturnsAsync(deleteResultMock.Object);

//            // Act
//            await _sut.EliminarEscenario(_escenario1.Id.ToString(), ct);

//            // Assert
//            _mockCollection.Verify(c => c.DeleteOneAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    ct),
//                Times.Once);
//        }

//        // -----------------------
//        // Helper para IAsyncCursor
//        // -----------------------

//        private static IAsyncCursor<Escenario> BuildCursor(List<Escenario> docs)
//        {
//            var cursor = new Mock<IAsyncCursor<Escenario>>();

//            bool first = true;

//            cursor
//                .Setup(c => c.MoveNext(It.IsAny<CancellationToken>()))
//                .Returns(() =>
//                {
//                    if (first)
//                    {
//                        first = false;
//                        return docs.Count > 0;
//                    }
//                    return false;
//                });

//            cursor
//                .Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(() =>
//                {
//                    if (first)
//                    {
//                        first = false;
//                        return docs.Count > 0;
//                    }
//                    return false;
//                });

//            cursor
//                .SetupGet(c => c.Current)
//                .Returns(docs);

//            return cursor.Object;
//        }

//        // ---------------------------------------------
//        // 1) Filtro con search + ciudad + activo
//        // ---------------------------------------------
//        [Fact]
//        public async Task SearchAsync_Filtra_Por_Search_Ciudad_Y_Activo()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var listaFiltrada = new List<Escenario> { _e1 }; // lo que "Mongo" devolvería

//            var cursor = BuildCursor(listaFiltrada);

//            _mockCollection
//                .Setup(c => c.CountDocumentsAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<CountOptions>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(1L);

//            _mockCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<FindOptions<Escenario, Escenario>>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(cursor);

//            // Act
//            var (items, total) = await _sut.SearchAsync(
//                search: "Teatro",
//                ciudad: "Caracas",
//                activo: true,
//                page: 1,
//                pageSize: 10,
//                ct: ct
//            );

//            // Assert
//            Assert.Equal(1, total);
//            Assert.Single(items);
//            Assert.Equal("Teatro Municipal", items[0].Nombre);
//            Assert.Equal("Caracas", items[0].Ciudad);
//            Assert.True(items[0].Activo);
//        }

//        // ---------------------------------------------
//        // 2) Sin resultados: debe devolver lista vacía y total 0
//        // ---------------------------------------------
//        [Fact]
//        public async Task SearchAsync_Sin_Resultados_Debe_Retornar_Vacio()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var listaVacia = new List<Escenario>();
//            var cursor = BuildCursor(listaVacia);

//            _mockCollection
//                .Setup(c => c.CountDocumentsAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<CountOptions>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(0L);

//            _mockCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<FindOptions<Escenario, Escenario>>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(cursor);

//            // Act
//            var (items, total) = await _sut.SearchAsync(
//                search: "AlgoQueNoExiste",
//                ciudad: "Narnia",
//                activo: true,
//                page: 1,
//                pageSize: 10,
//                ct: ct
//            );

//            // Assert
//            Assert.Equal(0, total);
//            Assert.Empty(items);
//        }

//        // ---------------------------------------------
//        // 3) Verifica que aplique paginación (Skip/Limit) en opciones
//        //    (probamos que calcula bien, no que Mongo recorte la lista)
//        // ---------------------------------------------
//        [Fact]
//        public async Task SearchAsync_Aplica_Skip_And_Limit_En_Options()
//        {
//            // Arrange
//            var ct = CancellationToken.None;

//            var lista = new List<Escenario> { _e1, _e2 };
//            var cursor = BuildCursor(lista);

//            FindOptions<Escenario, Escenario>? capturedOptions = null;

//            _mockCollection
//                .Setup(c => c.CountDocumentsAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<CountOptions>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(2L);

//            _mockCollection
//                .Setup(c => c.FindAsync(
//                    It.IsAny<FilterDefinition<Escenario>>(),
//                    It.IsAny<FindOptions<Escenario, Escenario>>(),
//                    It.IsAny<CancellationToken>()))
//                .Callback<FilterDefinition<Escenario>, FindOptions<Escenario, Escenario>, CancellationToken>((f, o, token) =>
//                {
//                    capturedOptions = o;
//                })
//                .ReturnsAsync(cursor);

//            var page = 2;
//            var pageSize = 10;

//            // Act
//            var (_, total) = await _sut.SearchAsync(
//                search: null,
//                ciudad: null,
//                activo: null,
//                page: page,
//                pageSize: pageSize,
//                ct: ct
//            );

//            // Assert
//            Assert.Equal(2, total);
//            Assert.NotNull(capturedOptions);
//            Assert.Equal((page - 1) * pageSize, capturedOptions!.Skip);
//            Assert.Equal(pageSize, capturedOptions.Limit);
//        }
//    }
//}
