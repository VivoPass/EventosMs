using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.Zonas.ModificarZonaEvento;
using EventsService.Dominio.Excepciones;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.Zonas
{
    public class ZonasEventoController_ModificarZona_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILog> _mockLogger;
        private readonly ZonasEventoController _controller;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();

        public ZonasEventoController_ModificarZona_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILog>();

            _controller = new ZonasEventoController(_mockMediator.Object, _mockLogger.Object);
        }

        #region ModificarZona_Exito_Retorna204NoContent
        [Fact]
        public async Task ModificarZona_Exito_Retorna204NoContent()
        {
            // ARRANGE
            var body = new ModificarZonaEventoCommnand
            {
                EventId = Guid.NewGuid(),
                ZonaId = Guid.NewGuid(),
                Nombre = "Nueva VIP"
            };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ModificarZonaEventoCommnand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await _controller.ModificarZona(_eventId, _zonaId, body, CancellationToken.None);

            // ASSERT
            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);

            _mockMediator.Verify(m => m.Send(
                    It.Is<ModificarZonaEventoCommnand>(c =>
                        c.EventId == _eventId &&
                        c.ZonaId == _zonaId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region ModificarZona_NoExiste_LanzaNotFoundException
        [Fact]
        public async Task ModificarZona_NoExiste_LanzaNotFoundException()
        {
            // ARRANGE
            var body = new ModificarZonaEventoCommnand
            {
                EventId = Guid.NewGuid(),
                ZonaId = Guid.NewGuid(),
                Nombre = "Zona X"
            };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ModificarZonaEventoCommnand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT + ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _controller.ModificarZona(_eventId, _zonaId, body, CancellationToken.None));
        }
        #endregion
    }
}
