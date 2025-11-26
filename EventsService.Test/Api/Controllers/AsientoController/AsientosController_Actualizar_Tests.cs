using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.Asiento.ActualizarAsiento;
using EventsService.Aplicacion.DTOs.Asiento;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.Asientos
{
    public class AsientosController_Actualizar_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILog> _mockLogger;
        private readonly AsientosController _controller;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();
        private readonly Guid _asientoId = Guid.NewGuid();

        public AsientosController_Actualizar_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILog>();

            _controller = new AsientosController(_mockMediator.Object, _mockLogger.Object);
        }

        #region Actualizar_AsientoValido_Retorna204NoContent
        [Fact]
        public async Task Actualizar_AsientoValido_Retorna204NoContent()
        {
            // ARRANGE
            var dto = new ActualizarAsientoDto
            {
                Label = "A10",
                Estado = "Reservado"
            };

            // ACT
            var result = await _controller.Actualizar(_eventId, _zonaId, _asientoId, dto, CancellationToken.None);

            // ASSERT
            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);

            _mockMediator.Verify(m => m.Send(
                    It.Is<ActualizarAsientoCommand>(c =>
                        c.EventId == _eventId &&
                        c.ZonaId == _zonaId &&
                        c.AsientoId == _asientoId &&
                        c.Label == dto.Label &&
                        c.Estado == dto.Estado),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
