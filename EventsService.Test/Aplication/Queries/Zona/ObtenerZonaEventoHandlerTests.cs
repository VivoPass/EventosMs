using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Aplicacion.DTOs.Zonas;
using EventsService.Aplicacion.Queries.Zona.ObtenerZonaEvento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Interfaces;
using Moq;
using Xunit;

namespace EventsService.Test.Aplication.Queries.Zona
{
    public class ObtenerZonaEventoHandlerTests
    {
        private readonly Mock<IZonaEventoRepository> _zonaRepoMock;
        private readonly Mock<IEscenarioZonaRepository> _ezRepoMock;
        private readonly Mock<IAsientoRepository> _asientoRepoMock;
        private readonly ObtenerZonaEventoHandler _handler;

        // datos fake
        private readonly Guid _eventId;
        private readonly Guid _zonaId;
        private ZonaEvento _zona;
        private EscenarioZona _ez;
        private List<Dominio.Entidades.Asiento> _asientos;

        public ObtenerZonaEventoHandlerTests()
        {
            _zonaRepoMock = new Mock<IZonaEventoRepository>();
            _ezRepoMock = new Mock<IEscenarioZonaRepository>();
            _asientoRepoMock = new Mock<IAsientoRepository>();

            _handler = new ObtenerZonaEventoHandler(
                _zonaRepoMock.Object,
                _ezRepoMock.Object,
                _asientoRepoMock.Object
            );

            _eventId = Guid.NewGuid();
            _zonaId = Guid.NewGuid();

            _zona = new ZonaEvento
            {
                Id = _zonaId,
                EventId = _eventId,
                EscenarioId = Guid.NewGuid(),
                Nombre = "Zona A",
                Tipo = "general",
                Capacidad = 120,
                Precio = 10.5m,
                Estado = "active",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _ez = new EscenarioZona
            {
                Id = Guid.NewGuid(),
                EventId = _eventId,
                EscenarioId = _zona.EscenarioId,
                ZonaEventoId = _zonaId,
                Grid = new EventsService.Dominio.ValueObjects.GridRef
                {
                    StartRow = 2,
                    StartCol = 3,
                    RowSpan = 4,
                    ColSpan = 5
                },
                Color = "#ABCDEF",
                Visible = true,
                ZIndex = 7,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _asientos = new List<Dominio.Entidades.Asiento>
            {
                new Dominio.Entidades.Asiento { Id = Guid.NewGuid(), EventId = _eventId, ZonaEventoId = _zonaId, Label = "A1", Estado = "available", FilaIndex = 1, ColIndex = 1 },
                new Dominio.Entidades.Asiento { Id = Guid.NewGuid(), EventId = _eventId, ZonaEventoId = _zonaId, Label = "A2", Estado = "reserved", FilaIndex = 1, ColIndex = 2 }
            };

            // por defecto: zona existe y ez existe
            _zonaRepoMock
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_zona);

            _ezRepoMock
                .Setup(r => r.GetByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_ez);

            // por defecto: asientos listos para cuando se soliciten
            _asientoRepoMock
                .Setup(r => r.ListByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_asientos.AsReadOnly());
        }

        [Fact]
        public async Task Handle_ThrowsNotFoundException_WhenZonaNotFound()
        {
            // Arrange
            _zonaRepoMock
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ZonaEvento?)null);

            var query = new ObtenerZonaEventoQuery
            {
                EventId = _eventId,
                ZonaId = _zonaId,
                IncludeSeats = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ReturnsDto_WhenZoneExists_AndIncludeSeatsFalse()
        {
            // Arrange
            var query = new ObtenerZonaEventoQuery
            {
                EventId = _eventId,
                ZonaId = _zonaId,
                IncludeSeats = false
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_zonaId, result!.Id);
            Assert.Equal(_eventId, result.EventId);
            Assert.Equal(_zona.EscenarioId, result.EscenarioId);
            Assert.Equal("Zona A", result.Nombre);
            Assert.Equal("general", result.Tipo);
            Assert.Equal(120, result.Capacidad);
            Assert.Equal(10.5m, result.Precio);
            Assert.Equal("active", result.Estado);

            // grid mapeado desde ez
            Assert.NotNull(result.Grid);
            Assert.Equal(_ez.Grid.StartRow, result.Grid.StartRow);
            Assert.Equal(_ez.Grid.StartCol, result.Grid.StartCol);
            Assert.Equal(_ez.Grid.RowSpan, result.Grid.RowSpan);
            Assert.Equal(_ez.Grid.ColSpan, result.Grid.ColSpan);
            Assert.Equal(_ez.Color, result.Grid.Color);
            Assert.Equal(_ez.ZIndex, result.Grid.ZIndex);
            Assert.Equal(_ez.Visible, result.Grid.Visible);

            // no pide asientos, por lo tanto no debe haber listado o llamado
            Assert.Null(result.Asientos);
            _asientoRepoMock.Verify(r => r.ListByZonaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsDto_WithSeats_WhenIncludeSeatsTrue_AndTipoSentado()
        {
            // Arrange: convertir zona a tipo "sentado"
            _zona.Tipo = "sentado";
            _zonaRepoMock
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_zona);

            var query = new ObtenerZonaEventoQuery
            {
                EventId = _eventId,
                ZonaId = _zonaId,
                IncludeSeats = true
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result!.Asientos);
            Assert.Equal(2, result.Asientos!.Count);

            var seatDto1 = result.Asientos!.SingleOrDefault(s => s.Label == "A1");
            var seatDto2 = result.Asientos!.SingleOrDefault(s => s.Label == "A2");

            Assert.NotNull(seatDto1);
            Assert.Equal("available", seatDto1!.Estado);
            Assert.Equal(1, seatDto1.FilaIndex);
            Assert.Equal(1, seatDto1.ColIndex);

            Assert.NotNull(seatDto2);
            Assert.Equal("reserved", seatDto2!.Estado);
            Assert.Equal(1, seatDto2.FilaIndex);
            Assert.Equal(2, seatDto2.ColIndex);

            _asientoRepoMock.Verify(r => r.ListByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UsesDefaultGrid_WhenEscenarioZonaNull()
        {
            // Arrange: ez no existe
            _ezRepoMock
                .Setup(r => r.GetByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EscenarioZona?)null);

            var query = new ObtenerZonaEventoQuery
            {
                EventId = _eventId,
                ZonaId = _zonaId,
                IncludeSeats = false
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert: grid por defecto
            Assert.NotNull(result);
            Assert.NotNull(result!.Grid);
            Assert.Equal(0, result.Grid.StartRow);
            Assert.Equal(0, result.Grid.StartCol);
            Assert.Equal(0, result.Grid.RowSpan);
            Assert.Equal(0, result.Grid.ColSpan);
            Assert.Null(result.Grid.Color);
            Assert.Null(result.Grid.ZIndex);
            Assert.True(result.Grid.Visible); // por defecto true cuando ez == null
        }
    }
}
