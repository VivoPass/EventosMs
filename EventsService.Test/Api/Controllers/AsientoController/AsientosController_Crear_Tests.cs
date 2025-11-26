using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.Asiento.CrearAsiento;
using EventsService.Aplicacion.DTOs.Asiento;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.Asientos
{
    public class AsientosController_Crear_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILog> _mockLogger;
        private readonly AsientosController _controller;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();
        private readonly Guid _asientoId = Guid.NewGuid();

        public AsientosController_Crear_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILog>();

            _controller = new AsientosController(_mockMediator.Object, _mockLogger.Object);
        }

        #region Crear_AsientoValido_Retorna201Created
        [Fact]
        public async Task Crear_AsientoValido_Retorna201Created()
        {
            // ARRANGE
            var dto = new CrearAsientoDto
            {
                FilaIndex = 1,
                ColIndex = 5,
                Label = "A5",
                Estado = "Libre",
                Meta = new Dictionary<string, string> { { "key", "value" } }
            };

            var resultCommand = new CrearAsientoResult(_asientoId);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CrearAsientoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultCommand);

            // ACT
            var result = await _controller.Crear(_eventId, _zonaId, dto, CancellationToken.None);

            // ASSERT
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
            Assert.Equal(nameof(AsientosController.ObtenerPorId), created.ActionName);

            Assert.Equal(_eventId, created.RouteValues["eventId"]);
            Assert.Equal(_zonaId, created.RouteValues["zonaId"]);
            Assert.Equal(_asientoId, created.RouteValues["asientoId"]);

            var value = created.Value!;
            var idProp = value.GetType().GetProperty("id");
            Assert.NotNull(idProp);
            var idValue = (Guid)idProp!.GetValue(value)!;
            Assert.Equal(_asientoId, idValue);

            _mockMediator.Verify(m => m.Send(
                    It.Is<CrearAsientoCommand>(c =>
                        c.EventId == _eventId &&
                        c.ZonaEventoId == _zonaId &&      // 👈 nombre real
                        c.FilaIndex == dto.FilaIndex &&
                        c.ColIndex == dto.ColIndex &&
                        c.Label == dto.Label &&
                        c.Estado == dto.Estado &&
                        c.Meta == dto.Meta),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion
    }
}
