using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.Zonas.ModificarZonaEvento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using Moq;
using Xunit;

namespace EventsService.Test.Aplicacion.CommandHandlers.Zonas
{
    public class ModificarZonaEventoHandler_Tests
    {
        private readonly Mock<IZonaEventoRepository> _mockZonaRepo;
        private readonly Mock<IEscenarioZonaRepository> _mockEscenarioZonaRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly ModificarZonaEventoHandler _handler;

        // --- DATOS ---
        private readonly Guid _eventId;
        private readonly Guid _zonaId;
        private readonly Guid _escenarioZonaId;

        public ModificarZonaEventoHandler_Tests()
        {
            _mockZonaRepo = new Mock<IZonaEventoRepository>();
            _mockEscenarioZonaRepo = new Mock<IEscenarioZonaRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new ModificarZonaEventoHandler(
                _mockZonaRepo.Object,
                _mockEscenarioZonaRepo.Object,
                _mockLog.Object);

            _eventId = Guid.NewGuid();
            _zonaId = Guid.NewGuid();
            _escenarioZonaId = Guid.NewGuid();
        }

        private ModificarZonaEventoCommnand BuildBaseCommand(bool conGrid = false)
        {
            var cmd = new ModificarZonaEventoCommnand
            {
                EventId = _eventId,
                ZonaId = _zonaId,
                Nombre = "Zona VIP Actualizada",
                Precio = 150m,
                Estado = "activa"
            };

            if (conGrid)
            {
                cmd.Grid = new EventsService.Dominio.ValueObjects.GridRef
                {
                    StartRow = 1,
                    StartCol = 1,
                    RowSpan = 2,
                    ColSpan = 3
                };
            }

            return cmd;
        }

        #region Handle_ZonaExiste_SinGrid_DeberiaActualizarYDevolverTrue()
        [Fact]
        public async Task Handle_ZonaExiste_SinGrid_DeberiaActualizarYDevolverTrue()
        {
            // ARRANGE
            var cmd = BuildBaseCommand(conGrid: false);

            var zonaExistente = new ZonaEvento
            {
                Id = _zonaId,
                EventId = _eventId,
                Nombre = "Zona Original",
                Precio = 100m,
                Estado = "borrador",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(zonaExistente);

            _mockZonaRepo
                .Setup(r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // ACT
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            _mockZonaRepo.Verify(
                r => r.UpdateAsync(It.Is<ZonaEvento>(z =>
                    z.Id == _zonaId &&
                    z.Nombre == cmd.Nombre.Trim() &&
                    z.Precio == cmd.Precio &&
                    z.Estado == cmd.Estado), It.IsAny<CancellationToken>()),
                Times.Once);

            // No debería tocar EscenarioZona si no hay grid
            _mockEscenarioZonaRepo.Verify(
                r => r.GetByZonaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_ZonaNoExiste_DeberiaLanzarNotFoundException()
        [Fact]
        public async Task Handle_ZonaNoExiste_DeberiaLanzarNotFoundException()
        {
            // ARRANGE
            var cmd = BuildBaseCommand(conGrid: false);

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ZonaEvento?)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(cmd, CancellationToken.None));

            _mockZonaRepo.Verify(
                r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_ZonaExiste_ConGrid_DeberiaActualizarZonaYGrid()
        [Fact]
        public async Task Handle_ZonaExiste_ConGrid_DeberiaActualizarZonaYGrid()
        {
            // ARRANGE
            var cmd = BuildBaseCommand(conGrid: true);

            var zonaExistente = new ZonaEvento
            {
                Id = _zonaId,
                EventId = _eventId,
                Nombre = "Zona Original",
                Precio = 100m,
                Estado = "borrador",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var escenarioZona = new EscenarioZona
            {
                Id = _escenarioZonaId,
                EventId = _eventId,
                ZonaEventoId = _zonaId
            };

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(zonaExistente);

            _mockZonaRepo
                .Setup(r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockEscenarioZonaRepo
                .Setup(r => r.GetByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(escenarioZona);

            _mockEscenarioZonaRepo
                .Setup(r => r.UpdateGridAsync(
                    _escenarioZonaId,
                    It.IsAny<EventsService.Dominio.ValueObjects.GridRef>(),
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // ACT
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            _mockZonaRepo.Verify(
                r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _mockEscenarioZonaRepo.Verify(
                r => r.GetByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockEscenarioZonaRepo.Verify(
                r => r.UpdateGridAsync(
                    _escenarioZonaId,
                    It.Is<EventsService.Dominio.ValueObjects.GridRef>(g =>
                        g.StartRow == cmd.Grid.StartRow &&
                        g.StartCol == cmd.Grid.StartCol &&
                        g.RowSpan == cmd.Grid.RowSpan &&
                        g.ColSpan == cmd.Grid.ColSpan),
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_ZonaExiste_ConGrid_PeroSinEscenarioZona_NoRevientaYDevuelveTrue()
        [Fact]
        public async Task Handle_ZonaExiste_ConGrid_PeroSinEscenarioZona_NoRevientaYDevuelveTrue()
        {
            // ARRANGE
            var cmd = BuildBaseCommand(conGrid: true);

            var zonaExistente = new ZonaEvento
            {
                Id = _zonaId,
                EventId = _eventId,
                Nombre = "Zona Original",
                Precio = 100m,
                Estado = "borrador"
            };

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(zonaExistente);

            _mockZonaRepo
                .Setup(r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockEscenarioZonaRepo
                .Setup(r => r.GetByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EscenarioZona?)null);

            // ACT
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            _mockEscenarioZonaRepo.Verify(
                r => r.UpdateGridAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<EventsService.Dominio.ValueObjects.GridRef>(),
                    It.IsAny<string?>(),
                    It.IsAny<int?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_RepositorioLanzaExcepcion_DeberiaLanzarModificarZonaEventoHandlerException()
        [Fact]
        public async Task Handle_RepositorioLanzaExcepcion_DeberiaLanzarModificarZonaEventoHandlerException()
        {
            // ARRANGE
            var cmd = BuildBaseCommand(conGrid: false);

            var zonaExistente = new ZonaEvento
            {
                Id = _zonaId,
                EventId = _eventId,
                Nombre = "Zona Original",
                Precio = 100m,
                Estado = "borrador"
            };

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(zonaExistente);

            var dbEx = new InvalidOperationException("DB error simulado");

            _mockZonaRepo
                .Setup(r => r.UpdateAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbEx);

            // ACT & ASSERT
            await Assert.ThrowsAsync<ModificarZonaEventoHandlerException>(
                () => _handler.Handle(cmd, CancellationToken.None));
        }
        #endregion
    }
}
