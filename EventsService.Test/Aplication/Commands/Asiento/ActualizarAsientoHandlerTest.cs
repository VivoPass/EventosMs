using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.Asiento.ActualizarAsiento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using Moq;
using Xunit;

namespace EventsService.Test.Aplicacion.CommandHandlers.Asiento
{
    public class ActualizarAsientoHandler_Tests
    {
        private readonly Mock<IAsientoRepository> _mockAsientoRepo;
        private readonly Mock<IZonaEventoRepository> _mockZonaRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly ActualizarAsientoHandler _handler;

        // --- DATOS ---
        private readonly Guid _eventId;
        private readonly Guid _zonaId;
        private readonly Guid _asientoId;

        public ActualizarAsientoHandler_Tests()
        {
            _mockAsientoRepo = new Mock<IAsientoRepository>();
            _mockZonaRepo = new Mock<IZonaEventoRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new ActualizarAsientoHandler(
                _mockAsientoRepo.Object,
                _mockZonaRepo.Object,
                _mockLog.Object
            );

            _eventId = Guid.NewGuid();
            _zonaId = Guid.NewGuid();
            _asientoId = Guid.NewGuid();
        }

        private ActualizarAsientoCommand BuildCommand(
            string? label = " A10 ",
            string? estado = "ocupado",
            Dictionary<string, string>? meta = null)
            => new ActualizarAsientoCommand(
                _eventId,
                _zonaId,
                _asientoId,
                label,
                estado,
                meta ?? new Dictionary<string, string> { { "key", "value" } });

        private ZonaEvento BuildZona()
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

        private Dominio.Entidades.Asiento BuildAsientoActual(string label = "A1")
            => new Dominio.Entidades.Asiento
            {
                Id = _asientoId,
                EventId = _eventId,
                ZonaEventoId = _zonaId,
                Label = label,
                Estado = "disponible",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        #region Handle_Valido_SinCambioDeLabel_DeberiaActualizarYRetornarTrue()
        [Fact]
        public async Task Handle_Valido_SinCambioDeLabel_DeberiaActualizarYRetornarTrue()
        {
            // ARRANGE
            var command = BuildCommand(label: "A1", estado: "ocupado");

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZona());

            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAsientoActual(label: "A1"));

            _mockAsientoRepo
                .Setup(r => r.UpdateParcialAsync(
                    _asientoId,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            _mockAsientoRepo.Verify(r => r.UpdateParcialAsync(
                _asientoId,
                command.Label,
                command.Estado,
                command.Meta,
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_ZonaNoExiste_DeberiaRetornarFalse()
        [Fact]
        public async Task Handle_ZonaNoExiste_DeberiaRetornarFalse()
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

            _mockAsientoRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockAsientoRepo.Verify(r => r.UpdateParcialAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_ZonaDeOtroEvento_DeberiaRetornarFalse()
        [Fact]
        public async Task Handle_ZonaDeOtroEvento_DeberiaRetornarFalse()
        {
            // ARRANGE
            var command = BuildCommand();

            var zonaDeOtroEvento = new ZonaEvento
            {
                Id = _zonaId,
                EventId = Guid.NewGuid(), // distinto
                Nombre = "Otra zona"
            };

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(zonaDeOtroEvento);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            _mockAsientoRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockAsientoRepo.Verify(r => r.UpdateParcialAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_AsientoNoExiste_DeberiaRetornarFalse()
        [Fact]
        public async Task Handle_AsientoNoExiste_DeberiaRetornarFalse()
        {
            // ARRANGE
            var command = BuildCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZona());

            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Dominio.Entidades.Asiento)null);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            _mockAsientoRepo.Verify(r => r.UpdateParcialAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_AsientoNoCoincideConZonaOEvento_DeberiaRetornarFalse()
        [Fact]
        public async Task Handle_AsientoNoCoincideConZonaOEvento_DeberiaRetornarFalse()
        {
            // ARRANGE
            var command = BuildCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZona());

            var asientoOtraZona = new Dominio.Entidades.Asiento
            {
                Id = _asientoId,
                EventId = _eventId,
                ZonaEventoId = Guid.NewGuid(), // otra zona
                Label = "Z9"
            };

            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(asientoOtraZona);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            _mockAsientoRepo.Verify(r => r.UpdateParcialAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_CambiaLabel_YaExisteDuplicado_DeberiaLanzarEventoException()
        [Fact]
        public async Task Handle_CambiaLabel_YaExisteDuplicado_DeberiaLanzarEventoException()
        {
            // ARRANGE
            var command = BuildCommand(label: "B15");

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZona());

            // Asiento actual con label "A1"
            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAsientoActual(label: "A1"));

            // Duplicado con nuevo label
            var asientoDuplicado = new Dominio.Entidades.Asiento
            {
                Id = Guid.NewGuid(),
                EventId = _eventId,
                ZonaEventoId = _zonaId,
                Label = "B15"
            };

            _mockAsientoRepo
                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, "B15", It.IsAny<CancellationToken>()))
                .ReturnsAsync(asientoDuplicado);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventoException>(
                () => _handler.Handle(command, CancellationToken.None));

            _mockAsientoRepo.Verify(r => r.UpdateParcialAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_UpdateParcialAsyncRetornaFalse_DeberiaRetornarFalse()
        [Fact]
        public async Task Handle_UpdateParcialAsyncRetornaFalse_DeberiaRetornarFalse()
        {
            // ARRANGE
            var command = BuildCommand(label: "A2");

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZona());

            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAsientoActual(label: "A1"));

            _mockAsientoRepo
                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, "A2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Dominio.Entidades.Asiento)null);

            _mockAsientoRepo
                .Setup(r => r.UpdateParcialAsync(
                    _asientoId,
                    "A2",
                    command.Estado,
                    command.Meta,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            _mockAsientoRepo.Verify(r => r.UpdateParcialAsync(
                _asientoId,
                "A2",
                command.Estado,
                command.Meta,
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_RepoLanzaExcepcion_DeberiaLanzarActualizarAsientoHandlerException()
        [Fact]
        public async Task Handle_RepoLanzaExcepcion_DeberiaLanzarActualizarAsientoHandlerException()
        {
            // ARRANGE
            var command = BuildCommand(label: "A3");

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZona());

            _mockAsientoRepo
                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAsientoActual(label: "A1"));

            _mockAsientoRepo
                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, "A3", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Dominio.Entidades.Asiento)null);

            var exDb = new InvalidOperationException("Fallo DB en UpdateParcialAsync");
            _mockAsientoRepo
                .Setup(r => r.UpdateParcialAsync(
                    _asientoId,
                    "A3",
                    command.Estado,
                    command.Meta,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exDb);

            // ACT & ASSERT
            await Assert.ThrowsAsync<ActualizarAsientoHandlerException>(
                () => _handler.Handle(command, CancellationToken.None));
        }
        #endregion
    }
}
