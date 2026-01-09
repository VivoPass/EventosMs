using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Infraestructura.Repositories;
using EventsService.Infrastructura.mongo;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace EventsService.Test.Infraestructura.Repositories
{
    public class Repository_CategoryRepository_Tests
    {
        private readonly Mock<IMongoDatabase> _mockDb;
        private readonly Mock<IMongoCollection<Categoria>> _mockCategoriasCollection;

        private readonly EventCollections _collections;
        private readonly CategoryRepositoryMongo _repository;

        private readonly Categoria _cat1;
        private readonly Categoria _cat2;

        public Repository_CategoryRepository_Tests()
        {
            // 1. Mocks base
            _mockDb = new Mock<IMongoDatabase>();
            _mockCategoriasCollection = new Mock<IMongoCollection<Categoria>>();

            // 2. Configurar GetCollection<Categoria>()
            _mockDb
                .Setup(d => d.GetCollection<Categoria>(
                    "categorias",
                    It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockCategoriasCollection.Object);

            // 3. Crear EventCollections REAL con el IMongoDatabase mockeado
            _collections = new EventCollections(_mockDb.Object);

            // 4. Crear repositorio real con las colecciones mockeadas
            _repository = new CategoryRepositoryMongo(_collections);

            // 5. Datos para pruebas
            _cat1 = new Categoria
            {
                Id = Guid.NewGuid(),
                Nombre = "Deportes",
                Descripcion = "Eventos deportivos"
            };

            _cat2 = new Categoria
            {
                Id = Guid.NewGuid(),
                Nombre = "Música",
                Descripcion = "Conciertos y shows"
            };
        }

        private Mock<IAsyncCursor<Categoria>> CrearCursor(List<Categoria> categorias)
        {
            var cursor = new Mock<IAsyncCursor<Categoria>>();

            cursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categorias.Count > 0)
                .ReturnsAsync(false);

            cursor.SetupGet(c => c.Current).Returns(categorias);

            return cursor;
        }

      

        // --------------------------------------------------------------------
        // GetByIdAsync
        // --------------------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_Encontrado_RetornaCategoria()
        {
            var cursorMock = CrearCursor(new List<Categoria> { _cat1 });

            _mockCategoriasCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Categoria>>(),
                    It.IsAny<FindOptions<Categoria, Categoria>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByIdAsync(_cat1.Id, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(_cat1.Id, result!.Id);
        }

        // --------------------------------------------------------------------
        // GetAllAsync
        // --------------------------------------------------------------------
        [Fact]
        public async Task GetAllAsync_EncuentraCategorias_RetornaLista()
        {
            var cursorMock = CrearCursor(new List<Categoria> { _cat1, _cat2 });

            _mockCategoriasCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Categoria>>(),
                    It.IsAny<FindOptions<Categoria, Categoria>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetAllAsync(CancellationToken.None);

            Assert.Equal(2, result.Count);
        }
    }
}
