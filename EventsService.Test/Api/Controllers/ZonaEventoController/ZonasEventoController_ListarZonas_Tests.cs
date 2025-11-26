using EventsService.Api.Controllers;
using EventsService.Aplicacion.DTOs.Zonas;
using EventsService.Aplicacion.Queries.Zona.ListarZonasEvento;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.Zonas
{
    public class ZonasEventoController_ListarZonas_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILog> _mockLogger;
        private readonly ZonasEventoController _controller;

        private readonly Guid _eventId = Guid.NewGuid();

        public ZonasEventoController_ListarZonas_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILog>();

            _controller = new ZonasEventoController(_mockMediator.Object, _mockLogger.Object);
        }

        #region ListarZonas_ConResultados_Retorna200Ok
        [Fact]
        public async Task ListarZonas_ConResultados_Retorna200Ok()
        {
            // ARRANGE
            var zonas = new List<ZonaEventoDto>
            {
                new ZonaEventoDto
                {
                    Id = Guid.NewGuid(),
                    EventId = _eventId,
                    EscenarioId = Guid.NewGuid(),
                    Nombre = "VIP",
                    Tipo = "Numerada",
                    Capacidad = 100,
                    Precio = 60m,
                    Estado = "Activa",
                    Grid = new GridDto { StartRow = 0, StartCol = 0, RowSpan = 4, ColSpan = 5 },
                    Asientos = null,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new ZonaEventoDto
                {
                    Id = Guid.NewGuid(),
                    EventId = _eventId,
                    EscenarioId = Guid.NewGuid(),
                    Nombre = "General",
                    Tipo = "Libre",
                    Capacidad = 300,
                    Precio = 20m,
                    Estado = "Activa",
                    Grid = new GridDto { StartRow = 5, StartCol = 0, RowSpan = 10, ColSpan = 20 },
                    Asientos = null,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ListarZonasEventoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(zonas);

            // ACT
            var result = await _controller.ListarZonas(
                _eventId,
                tipo: "Numerada",
                estado: "Activa",
                search: "VIP",
                includeSeats: true,
                ct: CancellationToken.None);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var value = Assert.IsAssignableFrom<IEnumerable<ZonaEventoDto>>(ok.Value);
            Assert.Equal(2, value.Count());

            _mockMediator.Verify(m => m.Send(
                    It.Is<ListarZonasEventoQuery>(q =>
                        q.EventId == _eventId &&
                        q.Tipo == "Numerada" &&
                        q.Estado == "Activa" &&
                        q.Search == "VIP" &&
                        q.IncludeSeats == true),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region ListarZonas_SinResultados_RetornaListaVacia
        [Fact]
        public async Task ListarZonas_SinResultados_RetornaListaVacia()
        {
            // ARRANGE
            var zonas = new List<ZonaEventoDto>();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ListarZonasEventoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(zonas);

            // ACT
            var result = await _controller.ListarZonas(
                _eventId,
                tipo: null,
                estado: null,
                search: null,
                includeSeats: false,
                ct: CancellationToken.None);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var value = Assert.IsAssignableFrom<IEnumerable<ZonaEventoDto>>(ok.Value);
            Assert.Empty(value);

            _mockMediator.Verify(m => m.Send(
                    It.Is<ListarZonasEventoQuery>(q =>
                        q.EventId == _eventId &&
                        q.Tipo == null &&
                        q.Estado == null &&
                        q.Search == null &&
                        q.IncludeSeats == false),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
