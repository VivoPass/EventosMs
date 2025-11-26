using EventsService.Api.Controllers;
using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Aplicacion.Queries.Asiento.ListarAsientos;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.Asientos
{
    public class AsientosController_Listar_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILog> _mockLogger;
        private readonly AsientosController _controller;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();

        public AsientosController_Listar_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILog>();

            _controller = new AsientosController(_mockMediator.Object, _mockLogger.Object);
        }

        #region Listar_ConResultados_Retorna200Ok
        [Fact]
        public async Task Listar_ConResultados_Retorna200Ok()
        {
            // ARRANGE
            var asientos = new List<AsientoDto>
            {
                new AsientoDto { Id = Guid.NewGuid(), FilaIndex = 1, ColIndex = 1, Label = "A1", Estado = "Libre" },
                new AsientoDto { Id = Guid.NewGuid(), FilaIndex = 1, ColIndex = 2, Label = "A2", Estado = "Ocupado" }
            };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ListarAsientosQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(asientos);

            // ACT
            var result = await _controller.Listar(_eventId, _zonaId, CancellationToken.None);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var value = Assert.IsAssignableFrom<IReadOnlyList<AsientoDto>>(ok.Value);
            Assert.Equal(2, value.Count);

            _mockMediator.Verify(m => m.Send(
                    It.Is<ListarAsientosQuery>(q =>
                        q.EventId == _eventId &&
                        q.ZonaId == _zonaId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Listar_SinResultados_RetornaListaVacia
        [Fact]
        public async Task Listar_SinResultados_RetornaListaVacia()
        {
            // ARRANGE
            var asientos = new List<AsientoDto>();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ListarAsientosQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(asientos);

            // ACT
            var result = await _controller.Listar(_eventId, _zonaId, CancellationToken.None);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var value = Assert.IsAssignableFrom<IReadOnlyList<AsientoDto>>(ok.Value);
            Assert.Empty(value);
        }
        #endregion
    }
}
