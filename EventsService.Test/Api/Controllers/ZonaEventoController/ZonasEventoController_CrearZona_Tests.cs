using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.Zonas.CrearZonaEvento;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.Zonas
{
    public class ZonasEventoController_CrearZona_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILog> _mockLogger;
        private readonly ZonasEventoController _controller;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();

        public ZonasEventoController_CrearZona_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILog>();

            _controller = new ZonasEventoController(_mockMediator.Object, _mockLogger.Object);
        }

        #region CrearZona_Exito_Retorna201Created
        [Fact]
        public async Task CrearZona_Exito_Retorna201Created()
        {
            // ARRANGE
            var body = new CreateZonaEventoCommand
            {
                EventId = Guid.NewGuid(), // debería ser sobreescrito por el route param
                Nombre = "Zona VIP",
                Tipo = "Numerada"
            };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateZonaEventoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_zonaId);

            // ACT
            var result = await _controller.CrearZona(_eventId, body, CancellationToken.None);

            // ASSERT
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
            Assert.Equal(nameof(ZonasEventoController.ObtenerZona), created.ActionName);
            Assert.Equal(_eventId, created.RouteValues["eventId"]);
            Assert.Equal(_zonaId, created.RouteValues["zonaId"]);

            // El body que llega al Mediator debe tener el EventId del route
            _mockMediator.Verify(m => m.Send(
                    It.Is<CreateZonaEventoCommand>(c => c.EventId == _eventId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
