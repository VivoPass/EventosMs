using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.Zonas.EliminarZonaEvento;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using Moq;
using Xunit;

namespace EventsService.Test.Aplicacion.CommandHandlers.Zonas
{
    public class EliminarZonaEventoHandler_Tests
    {
        private readonly Mock<IZonaEventoRepository> _mockZonaRepo;
        private readonly Mock<IAsientoRepository> _mockAsientoRepo;
        private readonly Mock<IEscenarioZonaRepository> _mockEzRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly EliminarZonaEventoHandler _handler;

        // --- DATOS ---
        private readonly Guid _eventId;
        private readonly Guid _zonaId;

        public EliminarZonaEventoHandler_Tests()
        {
            _mockZonaRepo = new Mock<IZonaEventoRepository>();
            _mockAsientoRepo = new Mock<IAsientoRepository>();
            _mockEzRepo = new Mock<IEscenarioZonaRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new EliminarZonaEventoHandler(
                _mockZonaRepo.Object,
                _mockAsientoRepo.Object,
                _mockEzRepo.Object,
                _mockLog.Object
            );

            _eventId = Guid.NewGuid();
            _zonaId = Guid.NewGuid();
        }

        private EliminarZonaEventoCommand BuildBaseCommand()
        {
            return new EliminarZonaEventoCommand
            {
                EventId = _eventId,
                ZonaId = _zonaId
            };
        }

        #region Handle_ZonaExiste_EliminaTodoYRetornaTrue()
        [Fact]
        public async Task Handle_ZonaExiste_EliminaTodoYRetornaTrue()
        {
            // ARRANGE
            var cmd = BuildBaseCommand();

            _mockAsientoRepo
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(10); // 10 asientos eliminados

            _mockEzRepo
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true); // bloque visual eliminado

            _mockZonaRepo
                .Setup(r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true); // zona eliminada

            // ACT
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            _mockAsientoRepo.Verify(
                r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockEzRepo.Verify(
                r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockZonaRepo.Verify(
                r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_ZonaNoExiste_EliminaDependenciasPeroRetornaFalse()
        [Fact]
        public async Task Handle_ZonaNoExiste_EliminaDependenciasPeroRetornaFalse()
        {
            // ARRANGE
            var cmd = BuildBaseCommand();

            _mockAsientoRepo
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            _mockEzRepo
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockZonaRepo
                .Setup(r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // no encontró la zona

            // ACT
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            _mockAsientoRepo.Verify(
                r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockEzRepo.Verify(
                r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockZonaRepo.Verify(
                r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_RepositorioAsientosLanzaExcepcion_DeberiaLanzarEliminarZonaEventoHandlerException()
        [Fact]
        public async Task Handle_RepositorioAsientosLanzaExcepcion_DeberiaLanzarEliminarZonaEventoHandlerException()
        {
            // ARRANGE
            var cmd = BuildBaseCommand();

            var exDb = new InvalidOperationException("Error de conexión a la BD de asientos.");

            _mockAsientoRepo
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exDb);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EliminarZonaEventoHandlerException>(
                () => _handler.Handle(cmd, CancellationToken.None));

            // Al lanzar en asientos, no debería llegar a eliminar escenarioZona ni zona
            _mockEzRepo.Verify(
                r => r.DeleteByZonaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _mockZonaRepo.Verify(
                r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_RepositorioZonaLanzaExcepcion_DeberiaLanzarEliminarZonaEventoHandlerException()
        [Fact]
        public async Task Handle_RepositorioZonaLanzaExcepcion_DeberiaLanzarEliminarZonaEventoHandlerException()
        {
            // ARRANGE
            var cmd = BuildBaseCommand();

            _mockAsientoRepo
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);

            _mockEzRepo
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var exDb = new InvalidOperationException("Error de conexión en DeleteAsync de ZonaEvento.");

            _mockZonaRepo
                .Setup(r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exDb);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EliminarZonaEventoHandlerException>(
                () => _handler.Handle(cmd, CancellationToken.None));

            _mockAsientoRepo.Verify(
                r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockEzRepo.Verify(
                r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
