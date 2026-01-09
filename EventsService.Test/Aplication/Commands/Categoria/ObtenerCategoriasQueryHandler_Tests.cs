using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Queries.Categoria;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using Moq;
using Xunit;

namespace EventsService.Test.Application.QueryHandlers
{
    public class ObtenerCategoriasQueryHandler_Tests
    {
        private readonly Mock<ICategoryRepository> _mockRepo;
        private readonly ObtenerCategoriasQueryHandler _handler;

        private readonly List<Categoria> _listaCategorias;

        public ObtenerCategoriasQueryHandler_Tests()
        {
            _mockRepo = new Mock<ICategoryRepository>();
            _handler = new ObtenerCategoriasQueryHandler(_mockRepo.Object);

            _listaCategorias = new List<Categoria>
            {
                new Categoria { Id = Guid.NewGuid(), Nombre = "Deportes", Descripcion = "Eventos deportivos" },
                new Categoria { Id = Guid.NewGuid(), Nombre = "Música", Descripcion = "Conciertos" },
            };
        }

        // ------------------------------------------------------------
        // Caso 1: Debe retornar una lista de categorías si existen
        // ------------------------------------------------------------
        [Fact]
        public async Task Handle_CategoriasExisten_DebeRetornarLista()
        {
            // ARRANGE
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_listaCategorias);

            var query = new ObtenerCategoriasQuery();

            // ACT
            var resultado = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, c => c.Nombre == "Deportes");
            Assert.Contains(resultado, c => c.Nombre == "Música");
        }

        // ------------------------------------------------------------
        // Caso 2: Debe retornar lista vacía si no hay categorías
        // ------------------------------------------------------------
        [Fact]
        public async Task Handle_NoHayCategorias_DebeRetornarListaVacia()
        {
            // ARRANGE
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Categoria>());

            var query = new ObtenerCategoriasQuery();

            // ACT
            var resultado = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        // ------------------------------------------------------------
        // Caso 3: Error del repositorio → se debe propagar
        // ------------------------------------------------------------
        [Fact]
        public async Task Handle_FalloEnRepositorio_DebePropagarExcepcion()
        {
            // ARRANGE
            var exSimulada = new InvalidOperationException("Error DB");
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(exSimulada);

            var query = new ObtenerCategoriasQuery();

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Equal(exSimulada, ex);
        }
    }
}
