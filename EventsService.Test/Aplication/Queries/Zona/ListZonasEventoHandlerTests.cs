using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Aplicacion.DTOs.Zonas;
using EventsService.Aplicacion.Queries.Zona.ListarZonasEvento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using Moq;
using Xunit;

namespace EventsService.Test.Aplication.Queries.Zona
{
    public class ListZonasEventoHandlerTests
    {
        private readonly Mock<IZonaEventoRepository> _zonaRepoMock;
        private readonly Mock<IEscenarioZonaRepository> _ezRepoMock;
        private readonly Mock<IAsientoRepository> _asientoRepoMock;
        private readonly ListZonasEventoHandler _handler;

        private readonly Guid _eventId;
        private readonly Guid _zonaId1;
        private readonly Guid _zonaId2;

        private ZonaEvento _zona1;
        private ZonaEvento _zona2;
        private EscenarioZona _ezZona1;
        private List<Dominio.Entidades.Asiento> _asientosZona1;

        public ListZonasEventoHandlerTests()
        {
            _zonaRepoMock = new Mock<IZonaEventoRepository>();
            _ezRepoMock = new Mock<IEscenarioZonaRepository>();
            _asientoRepoMock = new Mock<IAsientoRepository>();

            _handler = new ListZonasEventoHandler(
                _zonaRepoMock.Object,
                _ezRepoMock.Object,
                _asientoRepoMock.Object
            );

            _eventId = Guid.NewGuid();
            _zonaId1 = Guid.NewGuid();
            _zonaId2 = Guid.NewGuid();

            _zona1 = new ZonaEvento
            {
                Id = _zonaId1,
                EventId = _eventId,
                EscenarioId = Guid.NewGuid(),
                Nombre = "Zona Sentada Principal",
                Tipo = "sentado",
                Capacidad = 200,
                Precio = 15.0m,
                Estado = "active",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _zona2 = new ZonaEvento
            {
                Id = _zonaId2,
                EventId = _eventId,
                EscenarioId = Guid.NewGuid(),
                Nombre = "Zona General Exterior",
                Tipo = "general",
                Capacidad = 500,
                Precio = 5.0m,
                Estado = "inactive",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            };

            _ezZona1 = new EscenarioZona
            {
                Id = Guid.NewGuid(),
                EventId = _eventId,
                EscenarioId = _zona1.EscenarioId,
                ZonaEventoId = _zonaId1,
                Grid = new EventsService.Dominio.ValueObjects.GridRef { StartRow = 1, StartCol = 1, RowSpan = 3, ColSpan = 4 },
                Color = "#FF0000",
                Visible = true,
                ZIndex = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _asientosZona1 = new List<Dominio.Entidades.Asiento>
            {
                new Dominio.Entidades.Asiento { Id = Guid.NewGuid(), EventId = _eventId, ZonaEventoId = _zonaId1, Label = "A1", Estado = "available", FilaIndex = 1, ColIndex = 1 },
                new Dominio.Entidades.Asiento { Id = Guid.NewGuid(), EventId = _eventId, ZonaEventoId = _zonaId1, Label = "A2", Estado = "reserved", FilaIndex = 1, ColIndex = 2 }
            };

            // Por defecto: repo devuelve ambas zonas
            _zonaRepoMock
                .Setup(r => r.ListByEventAsync(_eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ZonaEvento> { _zona1, _zona2 });

            // por defecto: ez para zona1 existe, para zona2 puede no existir
            _ezRepoMock
                .Setup(r => r.GetByZonaAsync(_eventId, _zonaId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_ezZona1);

            _ezRepoMock
                .Setup(r => r.GetByZonaAsync(_eventId, _zonaId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EscenarioZona?)null);

            // por defecto: asientos para zona1
            _asientoRepoMock
                .Setup(r => r.ListByZonaAsync(_eventId, _zonaId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_asientosZona1.AsReadOnly());
        }

        [Fact]
        public async Task Handle_ReturnsMappedList_WhenNoFilters()
        {
            // Arrange
            var query = new ListarZonasEventoQuery { EventId = _eventId, Tipo = null, Estado = null, Search = null, IncludeSeats = false };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var z1 = result.SingleOrDefault(z => z.Id == _zonaId1);
            var z2 = result.SingleOrDefault(z => z.Id == _zonaId2);

            Assert.NotNull(z1);
            Assert.Equal("Zona Sentada Principal", z1!.Nombre);
            Assert.Equal("sentado", z1.Tipo);
            Assert.Equal(200, z1.Capacidad);
            Assert.Equal("#FF0000", z1.Grid.Color);
            Assert.Equal(1, z1.Grid.StartRow);
            Assert.True(z1.Asientos == null || z1.Asientos.Count == 0); // IncludeSeats=false

            Assert.NotNull(z2);
            Assert.Equal("Zona General Exterior", z2!.Nombre);
            Assert.Equal("general", z2.Tipo);
            Assert.Equal(500, z2.Capacidad);
            // ez null => default grid
            Assert.Equal(0, z2.Grid.StartRow);
            Assert.True(z2.Grid.Visible);
        }

        [Fact]
        public async Task Handle_AppliesFilters_TipoEstadoAndSearch()
        {
            // Arrange: query que filtra por tipo "general" y estado "inactive" y busca "Exterior"
            var query = new ListarZonasEventoQuery
            {
                EventId = _eventId,
                Tipo = "general",
                Estado = "inactive",
                Search = "Exterior",
                IncludeSeats = false
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert: sólo zona2 debe permanecer
            Assert.NotNull(result);
            Assert.Single(result);
            var only = result[0];
            Assert.Equal(_zonaId2, only.Id);
            Assert.Equal("Zona General Exterior", only.Nombre);
        }

        [Fact]
        public async Task Handle_IncludesSeats_WhenIncludeSeatsTrue_AndTipoSentado()
        {
            // Arrange: IncludeSeats true => only affects zonas with tipo "sentado"
            var query = new ListarZonasEventoQuery { EventId = _eventId, IncludeSeats = true };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert: zona1 debe traer Asientos, zona2 no (porque es "general")
            var z1 = result.Single(z => z.Id == _zonaId1);
            Assert.NotNull(z1.Asientos);
            Assert.Equal(2, z1.Asientos!.Count);
            Assert.Contains(z1.Asientos!, a => a.Label == "A1");
            Assert.Contains(z1.Asientos!, a => a.Label == "A2");

            var z2 = result.Single(z => z.Id == _zonaId2);
            Assert.True(z2.Asientos == null || z2.Asientos.Count == 0);

            _asientoRepoMock.Verify(r => r.ListByZonaAsync(_eventId, _zonaId1, It.IsAny<CancellationToken>()), Times.Once);
            _asientoRepoMock.Verify(r => r.ListByZonaAsync(_eventId, _zonaId2, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoZonas()
        {
            // Arrange: repo devuelve lista vacía
            _zonaRepoMock
                .Setup(r => r.ListByEventAsync(_eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ZonaEvento>());

            var query = new ListarZonasEventoQuery { EventId = _eventId, IncludeSeats = true };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
