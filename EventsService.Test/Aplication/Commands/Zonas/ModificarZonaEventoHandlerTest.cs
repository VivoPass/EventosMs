using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.Zonas.ModificarZonaEvento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.ValueObjects;
using Moq;
using Xunit;

namespace EventsService.Test.Aplication.Commands.Zonas
{
    public class ModificarZonaEventoHandlerTests
    {
        private readonly Mock<IZonaEventoRepository> _zonaRepoMock;
        private readonly Mock<IEscenarioZonaRepository> _escenarioZonaRepoMock;
        private readonly ModificarZonaEventoHandler _handler;

        // Datos fake
        private readonly Guid _eventId;
        private readonly Guid _zonaId;
        private ZonaEvento? _existingZona;
        private EscenarioZona? _existingEscenarioZona;

        // captura para inspección
        private ZonaEvento? _capturedUpdatedZona;

        public ModificarZonaEventoHandlerTests()
        {
            _zonaRepoMock = new Mock<IZonaEventoRepository>();
            _escenarioZonaRepoMock = new Mock<IEscenarioZonaRepository>();

            _handler = new ModificarZonaEventoHandler(_zonaRepoMock.Object, _escenarioZonaRepoMock.Object);

            _eventId = Guid.NewGuid();
            _zonaId = Guid.NewGuid();

            _existingZona = new ZonaEvento
            {
                Id = _zonaId,
                EventId = _eventId,
                Nombre = "Original Name",
                Precio = 20m,
                Estado = "active",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Capacidad = 100
            };

            _existingEscenarioZona = new EscenarioZona
            {
                Id = Guid.NewGuid(),
                EventId = _eventId,
                EscenarioId = Guid.NewGuid(),
                ZonaEventoId = _zonaId,
                Grid = null, // ahora usaremos GridRef en UpdateGridAsync
                Color = "#000000",
                Visible = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            // Por defecto retornamos la zona existente
            _zonaRepoMock
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _existingZona);
        }

        [Fact]
        public async Task Handle_ReturnsTrue_AndUpdatesFields_WhenZoneExists()
        {
            // Arrange
            _zonaRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
                .Callback<ZonaEvento, CancellationToken>((z, ct) => _capturedUpdatedZona = z)
                .Returns(Task.CompletedTask);

            var cmd = new ModificarZonaEventoCommnand
            {
                EventId = _eventId,
                ZonaId = _zonaId,
                Nombre = "New Name",
                Precio = 35.5m,
                Estado = "inactive",
                Grid = null // no actualizamos grid en este test
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.True(result);
            _zonaRepoMock.Verify(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
            _zonaRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.NotNull(_capturedUpdatedZona);
            Assert.Equal("New Name", _capturedUpdatedZona!.Nombre);
            Assert.Equal(35.5m, _capturedUpdatedZona.Precio);
            Assert.Equal("inactive", _capturedUpdatedZona.Estado);
            Assert.True(_capturedUpdatedZona.UpdatedAt > _capturedUpdatedZona.CreatedAt);
        }

        [Fact]
        public async Task Handle_CallsUpdateGrid_WhenGridProvided_AndEscenarioZonaExists()
        {
            // Arrange
            // repo.GetAsync ya devuelve _existingZona por defecto desde constructor

            _escenarioZonaRepoMock
                .Setup(r => r.GetByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_existingEscenarioZona);

            // Preparamos un GridRef esperado
            var newGrid = new GridRef { StartRow = 1, StartCol = 1, RowSpan = 2, ColSpan = 3 };

            _escenarioZonaRepoMock
                .Setup(r => r.UpdateGridAsync(
                    _existingEscenarioZona!.Id,
                    It.Is<GridRef>(g => g.StartRow == newGrid.StartRow && g.StartCol == newGrid.StartCol && g.RowSpan == newGrid.RowSpan && g.ColSpan == newGrid.ColSpan),
                    It.IsAny<string?>(),
                    It.IsAny<int?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _zonaRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var cmd = new ModificarZonaEventoCommnand
            {
                EventId = _eventId,
                ZonaId = _zonaId,
                Grid = newGrid
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.True(result);
            _escenarioZonaRepoMock.Verify(r => r.GetByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
            _escenarioZonaRepoMock.Verify(r => r.UpdateGridAsync(
                _existingEscenarioZona!.Id,
                It.Is<GridRef>(g => g.StartRow == newGrid.StartRow && g.StartCol == newGrid.StartCol && g.RowSpan == newGrid.RowSpan && g.ColSpan == newGrid.ColSpan),
                null,
                null,
                true,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DoesNotCallUpdateGrid_WhenGridIsNull()
        {
            // Arrange
            _zonaRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var cmd = new ModificarZonaEventoCommnand
            {
                EventId = _eventId,
                ZonaId = _zonaId,
                Grid = null // grid null
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.True(result);
            _escenarioZonaRepoMock.Verify(r => r.GetByZonaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _escenarioZonaRepoMock.Verify(r => r.UpdateGridAsync(It.IsAny<Guid>(), It.IsAny<GridRef>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DoesNotCallUpdateGrid_WhenEscenarioZonaNotFound()
        {
            // Arrange
            _escenarioZonaRepoMock
                .Setup(r => r.GetByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EscenarioZona?)null);

            _zonaRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var cmd = new ModificarZonaEventoCommnand
            {
                EventId = _eventId,
                ZonaId = _zonaId,
                Grid = new GridRef { StartRow = 2, StartCol = 2, RowSpan = 1, ColSpan = 1 }
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.True(result);
            _escenarioZonaRepoMock.Verify(r => r.GetByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
            _escenarioZonaRepoMock.Verify(r => r.UpdateGridAsync(It.IsAny<Guid>(), It.IsAny<GridRef>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsNotFoundException_WhenZonaNotFound()
        {
            // Arrange: GetAsync devuelve null
            _zonaRepoMock
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ZonaEvento?)null);

            var cmd = new ModificarZonaEventoCommnand
            {
                EventId = _eventId,
                ZonaId = _zonaId,
                Nombre = "Whatever"
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(cmd, CancellationToken.None));

            // Aseguramos que no intentó actualizar nada
            _zonaRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()), Times.Never);
            _escenarioZonaRepoMock.Verify(r => r.GetByZonaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
