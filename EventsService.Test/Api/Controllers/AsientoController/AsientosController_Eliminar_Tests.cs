using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.Asiento.EliminarAsiento;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.Asientos
{
    public class AsientosController_Eliminar_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILog> _mockLogger;
        private readonly AsientosController _controller;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();
        private readonly Guid _asientoId = Guid.NewGuid();

        public AsientosController_Eliminar_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILog>();

            _controller = new AsientosController(_mockMediator.Object, _mockLogger.Object);
        }

        #region Eliminar_AsientoValido_Retorna204NoContent
        [Fact]
        public async Task Eliminar_AsientoValido_Retorna204NoContent()
        {
            // ARRANGE
            // No hace falta Setup, no usamos el resultado de Send

            // ACT
            var result = await _controller.Eliminar(_eventId, _zonaId, _asientoId, CancellationToken.None);

            // ASSERT
            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);

            _mockMediator.Verify(m => m.Send(
                    It.Is<EliminarAsientoCommand>(c =>
                        c.EventId == _eventId &&
                        c.ZonaId == _zonaId &&
                        c.AsientoId == _asientoId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}