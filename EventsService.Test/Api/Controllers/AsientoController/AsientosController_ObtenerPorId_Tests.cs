using EventsService.Api.Controllers;
using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Aplicacion.Queries.Asiento.ObtenerAsiento;
using EventsService.Dominio.Excepciones;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.Asientos
{
    public class AsientosController_ObtenerPorId_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILog> _mockLogger;
        private readonly AsientosController _controller;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();
        private readonly Guid _asientoId = Guid.NewGuid();

        public AsientosController_ObtenerPorId_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILog>();

            _controller = new AsientosController(_mockMediator.Object, _mockLogger.Object);
        }

        #region ObtenerPorId_Existe_Retorna200Ok
        [Fact]
        public async Task ObtenerPorId_Existe_Retorna200Ok()
        {
            // ARRANGE
            var dto = new AsientoDto
            {
                Id = _asientoId,
                FilaIndex = 1,
                ColIndex = 5,
                Label = "A5",
                Estado = "Libre"
            };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ObtenerAsientoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // ACT
            var result = await _controller.ObtenerPorId(_eventId, _zonaId, _asientoId, CancellationToken.None);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var value = Assert.IsType<AsientoDto>(ok.Value);
            Assert.Equal(_asientoId, value.Id);

            _mockMediator.Verify(m => m.Send(
                    It.Is<ObtenerAsientoQuery>(q =>
                        q.EventId == _eventId &&
                        q.ZonaId == _zonaId &&
                        q.AsientoId == _asientoId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region ObtenerPorId_NoExiste_LanzaNotFoundException
        [Fact]
        public async Task ObtenerPorId_NoExiste_LanzaNotFoundException()
        {
            // ARRANGE
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ObtenerAsientoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AsientoDto?)null);

            // ACT + ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _controller.ObtenerPorId(_eventId, _zonaId, _asientoId, CancellationToken.None));

            _mockMediator.Verify(m => m.Send(
                    It.Is<ObtenerAsientoQuery>(q =>
                        q.EventId == _eventId &&
                        q.ZonaId == _zonaId &&
                        q.AsientoId == _asientoId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
