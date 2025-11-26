using EventsService.Api.Controllers;
using EventsService.Aplicacion.DTOs.Zonas;
using EventsService.Aplicacion.Queries.Zona.ObtenerZonaEvento;
using EventsService.Dominio.Excepciones;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.Zonas
{
    public class ZonasEventoController_ObtenerZona_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILog> _mockLogger;
        private readonly ZonasEventoController _controller;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();

        public ZonasEventoController_ObtenerZona_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILog>();

            _controller = new ZonasEventoController(_mockMediator.Object, _mockLogger.Object);
        }

        #region ObtenerZona_Existe_Retorna200Ok
        [Fact]
        public async Task ObtenerZona_Existe_Retorna200Ok()
        {
            // ARRANGE
            var dto = new ZonaEventoDto
            {
                Id = _zonaId,
                EventId = _eventId,
                EscenarioId = Guid.NewGuid(),
                Nombre = "Zona VIP",
                Tipo = "Numerada",
                Capacidad = 100,
                Precio = 50m,
                Estado = "Activa",
                Grid = new GridDto { StartRow = 0, StartCol = 0, RowSpan = 5, ColSpan = 10 },
                Asientos = null, // simula includeSeats=false si quieres
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ObtenerZonaEventoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // ACT
            var result = await _controller.ObtenerZona(_eventId, _zonaId, includeSeats: true, CancellationToken.None);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
            Assert.Same(dto, ok.Value);

            _mockMediator.Verify(m => m.Send(
                    It.Is<ObtenerZonaEventoQuery>(q =>
                        q.EventId == _eventId &&
                        q.ZonaId == _zonaId &&
                        q.IncludeSeats == true),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region ObtenerZona_NoExiste_LanzaNotFoundException
        [Fact]
        public async Task ObtenerZona_NoExiste_LanzaNotFoundException()
        {
            // ARRANGE
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ObtenerZonaEventoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ZonaEventoDto?)null);

            // ACT + ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _controller.ObtenerZona(_eventId, _zonaId, false, CancellationToken.None));

            _mockMediator.Verify(m => m.Send(
                    It.Is<ObtenerZonaEventoQuery>(q =>
                        q.EventId == _eventId &&
                        q.ZonaId == _zonaId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
