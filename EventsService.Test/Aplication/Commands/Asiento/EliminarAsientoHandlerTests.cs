using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.Asiento.EliminarAsiento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using Moq;
using Xunit;

namespace EventsService.Test.Aplicacion.CommandHandlers.Asiento
{
    public class EliminarAsientoHandler_Tests
    {
        private readonly Mock<IAsientoRepository> _mockAsientoRepo;
        private readonly Mock<IZonaEventoRepository> _mockZonaRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly EliminarAsientoHandler _handler;

        // --- DATOS ---
        private readonly Guid _eventId;
        private readonly Guid _zonaId;
        private readonly Guid _asientoId;

        public EliminarAsientoHandler_Tests()
        {
            _mockAsientoRepo = new Mock<IAsientoRepository>();
            _mockZonaRepo = new Mock<IZonaEventoRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new EliminarAsientoHandler(
                _mockAsientoRepo.Object,
                _mockZonaRepo.Object,
                _mockLog.Object
            );

            _eventId = Guid.NewGuid();
            _zonaId = Guid.NewGuid();
            _asientoId = Guid.NewGuid();
        }

        private EliminarAsientoCommand BuildCommand()
            => new EliminarAsientoCommand(_eventId, _zonaId, _asientoId);

        private ZonaEvento BuildZonaValida()
            => new ZonaEvento
            {
                Id = _zonaId,
                EventId = _eventId,
                Nombre = "Zona Principal",
                Tipo = "sentado",
                Capacidad = 100,
                Estado = "activa",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        private Dominio.Entidades.Asiento BuildAsientoValido()
            => new Dominio.Entidades.Asiento
            {
                Id = _asientoId,
                EventId = _eventId,
                ZonaEventoId = _zonaId,
                Label = "A1",
                Estado = "disponible",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        #region Handle_Valido_DeberiaEliminarYRetornarTrue()
        [Fact]
        public async Task Handle_Valido_DeberiaEliminarYRetornarTrue()
        {
            // ARRANGE
            var command = BuildCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZonaValida());

            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAsientoValido());

            _mockAsientoRepo
                .Setup(r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            _mockAsientoRepo.Verify(
                r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_ZonaNoExiste_DeberiaRetornarFalseYNoEliminar()
        [Fact]
        public async Task Handle_ZonaNoExiste_DeberiaRetornarFalseYNoEliminar()
        {
            // ARRANGE
            var command = BuildCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ZonaEvento)null);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            _mockAsientoRepo.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _mockAsientoRepo.Verify(
                r => r.DeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_ZonaDeOtroEvento_DeberiaRetornarFalseYNoEliminar()
        [Fact]
        public async Task Handle_ZonaDeOtroEvento_DeberiaRetornarFalseYNoEliminar()
        {
            // ARRANGE
            var command = BuildCommand();

            var zonaDeOtroEvento = new ZonaEvento
            {
                Id = _zonaId,
                EventId = Guid.NewGuid(), // distinto al del command
                Nombre = "Zona Ajena"
            };

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(zonaDeOtroEvento);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            _mockAsientoRepo.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _mockAsientoRepo.Verify(
                r => r.DeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_AsientoNoExiste_DeberiaRetornarFalseYNoEliminar()
        [Fact]
        public async Task Handle_AsientoNoExiste_DeberiaRetornarFalseYNoEliminar()
        {
            // ARRANGE
            var command = BuildCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZonaValida());

            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Dominio.Entidades.Asiento)null);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            _mockAsientoRepo.Verify(
                r => r.DeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_AsientoDeOtraZonaOEvento_DeberiaRetornarFalseYNoEliminar()
        [Fact]
        public async Task Handle_AsientoDeOtraZonaOEvento_DeberiaRetornarFalseYNoEliminar()
        {
            // ARRANGE
            var command = BuildCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZonaValida());

            var asientoDeOtraZona = new Dominio.Entidades.Asiento
            {
                Id = _asientoId,
                EventId = _eventId,
                ZonaEventoId = Guid.NewGuid(), // otra zona
                Label = "X9"
            };

            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(asientoDeOtraZona);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            _mockAsientoRepo.Verify(
                r => r.DeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_DeleteByIdAsyncRetornaFalse_DeberiaRetornarFalse()
        [Fact]
        public async Task Handle_DeleteByIdAsyncRetornaFalse_DeberiaRetornarFalse()
        {
            // ARRANGE
            var command = BuildCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZonaValida());

            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAsientoValido());

            _mockAsientoRepo
                .Setup(r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            _mockAsientoRepo.Verify(
                r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_RepoLanzaExcepcion_DeberiaLanzarEliminarAsientoHandlerException()
        [Fact]
        public async Task Handle_RepoLanzaExcepcion_DeberiaLanzarEliminarAsientoHandlerException()
        {
            // ARRANGE
            var command = BuildCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZonaValida());

            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAsientoValido());

            var exDb = new InvalidOperationException("Fallo en la BD al eliminar asiento.");
            _mockAsientoRepo
                .Setup(r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exDb);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EliminarAsientoHandlerException>(
                () => _handler.Handle(command, CancellationToken.None));
        }
        #endregion
    }
}
