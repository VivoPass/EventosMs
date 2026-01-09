using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Queries.Categoria;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.Entidades;
using Moq;
using Xunit;

namespace EventsService.Test.Application.QueryHandlers
{
    public class ObtenerCategoriaPorIdQueryHandler_Tests
    {
        private readonly Mock<ICategoryRepository> _mockRepo;
        private readonly ObtenerCategoriaPorIdQueryHandler _handler;

        private readonly Guid _idCategoria;
        private readonly Categoria _categoria;

        public ObtenerCategoriaPorIdQueryHandler_Tests()
        {
            _mockRepo = new Mock<ICategoryRepository>();
            _handler = new ObtenerCategoriaPorIdQueryHandler(_mockRepo.Object);

            _idCategoria = Guid.NewGuid();

            _categoria = new Categoria
            {
                Id = _idCategoria,
                Nombre = "Deportes",
                Descripcion = "Eventos deportivos"
            };
        }

        // ------------------------------------------------------------
        // Caso 1: Cuando la categoría existe → debe retornarla
        // ------------------------------------------------------------
        [Fact]
        public async Task Handle_CategoriaExiste_DebeRetornarCategoria()
        {
            // ARRANGE
            _mockRepo
                .Setup(r => r.GetByIdAsync(_idCategoria, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_categoria);

            var query = new ObtenerCategoriaPorIdQuery(_idCategoria);

            // ACT
            var resultado = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(_idCategoria, resultado.Id);
            Assert.Equal("Deportes", resultado.Nombre);
        }

        // ------------------------------------------------------------
        // Caso 2: Cuando la categoría NO existe → debe lanzar excepción
        // ------------------------------------------------------------
        [Fact]
        public async Task Handle_CategoriaNoExiste_DebeLanzarException()
        {
            // ARRANGE
            _mockRepo
                .Setup(r => r.GetByIdAsync(_idCategoria, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Categoria?)null);

            var query = new ObtenerCategoriaPorIdQuery(_idCategoria);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Contains(_idCategoria.ToString(), ex.Message);
        }

        // ------------------------------------------------------------
        // Caso 3: La DB falla (Mongo, red, etc.) → la excepción original debe subir
        // ------------------------------------------------------------
        [Fact]
        public async Task Handle_FalloEnRepositorio_DebePropagarExcepcion()
        {
            // ARRANGE
            var exSimulada = new InvalidOperationException("Error en DB");

            _mockRepo
                .Setup(r => r.GetByIdAsync(_idCategoria, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exSimulada);

            var query = new ObtenerCategoriaPorIdQuery(_idCategoria);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Equal(exSimulada, ex);
        }
    }
}
