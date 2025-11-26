using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.Zonas.EliminarZonaEvento;
using EventsService.Dominio.Excepciones;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.Zonas
{
    public class ZonasEventoController_EliminarZona_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILog> _mockLogger;
        private readonly ZonasEventoController _controller;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();

        public ZonasEventoController_EliminarZona_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILog>();

            _controller = new ZonasEventoController(_mockMediator.Object, _mockLogger.Object);
        }

        #region EliminarZona_Exito_Retorna204NoContent
        [Fact]
        public async Task EliminarZona_Exito_Retorna204NoContent()
        {
            // ARRANGE
            _mockMediator
                .Setup(m => m.Send(It.IsAny<EliminarZonaEventoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await _controller.EliminarZona(_eventId, _zonaId, CancellationToken.None);

            // ASSERT
            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);

            _mockMediator.Verify(m => m.Send(
                    It.Is<EliminarZonaEventoCommand>(c =>
                        c.EventId == _eventId &&
                        c.ZonaId == _zonaId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region EliminarZona_NoExiste_LanzaNotFoundException
        [Fact]
        public async Task EliminarZona_NoExiste_LanzaNotFoundException()
        {
            // ARRANGE
            _mockMediator
                .Setup(m => m.Send(It.IsAny<EliminarZonaEventoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT + ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _controller.EliminarZona(_eventId, _zonaId, CancellationToken.None));
        }
        #endregion
    }
}
