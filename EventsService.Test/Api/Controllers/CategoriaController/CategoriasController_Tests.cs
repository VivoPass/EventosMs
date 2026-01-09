using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.API.Controllers;
using EventsService.Aplicacion.Queries.Categoria;
using EventsService.Dominio.Entidades;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EventsService.Test.API.Controllers
{
    public class CategoriasController_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly CategoriasController _controller;

        private readonly List<Categoria> _categorias;
        private readonly Categoria _categoria;

        public CategoriasController_Tests()
        {
            _mockMediator = new Mock<IMediator>();

            _controller = new CategoriasController(_mockMediator.Object);

            _categorias = new List<Categoria>
            {
                new Categoria { Id = Guid.NewGuid(), Nombre = "Deportes", Descripcion = "Eventos deportivos" },
                new Categoria { Id = Guid.NewGuid(), Nombre = "Música", Descripcion = "Conciertos" }
            };

            _categoria = new Categoria
            {
                Id = Guid.NewGuid(),
                Nombre = "Tecnología",
                Descripcion = "Eventos tech"
            };
        }

        // ---------------------------------------------------------------
        // GET ALL - ÉXITO
        // ---------------------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturn200OKWithData()
        {
            // ARRANGE
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ObtenerCategoriasQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_categorias);

            // ACT
            var result = await _controller.GetAll(CancellationToken.None);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var data = Assert.IsType<List<Categoria>>(ok.Value);
            Assert.Equal(2, data.Count);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<ObtenerCategoriasQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ---------------------------------------------------------------
        // GET ALL - ERROR
        // ---------------------------------------------------------------
        [Fact]
        public async Task GetAll_WhenMediatorFails_ShouldReturn500()
        {
            // ARRANGE
            var ex = new Exception("Fallo en Mediator");
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ObtenerCategoriasQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // ACT
            var result = await Record.ExceptionAsync(() => _controller.GetAll(CancellationToken.None));

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("Fallo en Mediator", result.Message);
        }

        // ---------------------------------------------------------------
        // GET BY ID - ÉXITO
        // ---------------------------------------------------------------
        [Fact]
        public async Task GetById_ShouldReturn200OKWithCategoria()
        {
            // ARRANGE
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ObtenerCategoriaPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_categoria);

            // ACT
            var result = await _controller.GetById(_categoria.Id, CancellationToken.None);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var data = Assert.IsType<Categoria>(ok.Value);
            Assert.Equal(_categoria.Id, data.Id);

            _mockMediator.Verify(
                m => m.Send(It.Is<ObtenerCategoriaPorIdQuery>(q => q.Id == _categoria.Id),
                            It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ---------------------------------------------------------------
        // GET BY ID - ERROR
        // ---------------------------------------------------------------
        [Fact]
        public async Task GetById_WhenMediatorFails_ShouldReturnException()
        {
            // ARRANGE
            var ex = new Exception("No encontrado");
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ObtenerCategoriaPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // ACT
            var result = await Record.ExceptionAsync(() =>
                _controller.GetById(Guid.NewGuid(), CancellationToken.None));

            // ASSERT
            Assert.Equal("No encontrado", result.Message);
        }
    }
}
