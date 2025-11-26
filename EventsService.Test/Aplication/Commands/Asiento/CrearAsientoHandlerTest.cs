using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.Asiento.CrearAsiento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using Moq;
using Xunit;

namespace EventsService.Test.Aplicacion.CommandHandlers.Asiento
{
    public class CrearAsientoHandler_Tests
    {
        private readonly Mock<IAsientoRepository> _mockAsientoRepo;
        private readonly Mock<IZonaEventoRepository> _mockZonaRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly CrearAsientoHandler _handler;

        // --- DATOS ---
        private readonly Guid _eventId;
        private readonly Guid _zonaId;

        public CrearAsientoHandler_Tests()
        {
            _mockAsientoRepo = new Mock<IAsientoRepository>();
            _mockZonaRepo = new Mock<IZonaEventoRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new CrearAsientoHandler(
                _mockAsientoRepo.Object,
                _mockZonaRepo.Object,
                _mockLog.Object
            );

            _eventId = Guid.NewGuid();
            _zonaId = Guid.NewGuid();
        }

        private CrearAsientoCommand BuildBaseCommand(
            string? label = "A1",
            string? estado = "disponible")
        {
            return new CrearAsientoCommand(
                EventId: _eventId,
                ZonaEventoId: _zonaId,
                FilaIndex: 1,
                ColIndex: 1,
                Label: label!,
                Estado: estado,
                Meta: new Dictionary<string, string> { { "k", "v" } }
            );
        }

        private ZonaEvento BuildZonaValida()
        {
            return new ZonaEvento
            {
                Id = _zonaId,
                EventId = _eventId,
                Nombre = "Zona VIP",
                Tipo = "sentado",
                Capacidad = 10,
                Estado = "activa",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        #region Handle_Valido_DeberiaCrearAsientoYRetornarId()
        [Fact]
        public async Task Handle_Valido_DeberiaCrearAsientoYRetornarId()
        {
            // ARRANGE
            var command = BuildBaseCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZonaValida());

            _mockAsientoRepo
                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, command.Label, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Dominio.Entidades.Asiento)null);

            _mockAsientoRepo
                .Setup(r => r.InsertAsync(It.IsAny<Dominio.Entidades.Asiento>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.NotEqual(Guid.Empty, result.AsientoId);

            _mockAsientoRepo.Verify(
                r => r.InsertAsync(It.IsAny<Dominio.Entidades.Asiento>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_ZonaNoExiste_DeberiaLanzarEventoException()
        [Fact]
        public async Task Handle_ZonaNoExiste_DeberiaLanzarEventoException()
        {
            // ARRANGE
            var command = BuildBaseCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ZonaEvento)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventoException>(
                () => _handler.Handle(command, CancellationToken.None));

            _mockAsientoRepo.Verify(
                r => r.InsertAsync(It.IsAny<Dominio.Entidades.Asiento>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_ZonaNoPerteneceAlEvento_DeberiaLanzarEventoException()
        [Fact]
        public async Task Handle_ZonaNoPerteneceAlEvento_DeberiaLanzarEventoException()
        {
            // ARRANGE
            var command = BuildBaseCommand();

            var zonaDeOtroEvento = new ZonaEvento
            {
                Id = _zonaId,
                EventId = Guid.NewGuid(), // distinto
                Nombre = "Zona Ajena"
            };

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(zonaDeOtroEvento);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventoException>(
                () => _handler.Handle(command, CancellationToken.None));

            _mockAsientoRepo.Verify(
                r => r.InsertAsync(It.IsAny<Dominio.Entidades.Asiento>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_LabelVacio_DeberiaLanzarArgumentException()
        [Fact]
        public async Task Handle_LabelVacio_DeberiaLanzarArgumentException()
        {
            // ARRANGE
            var command = BuildBaseCommand(label: "  "); // label vacío/whitespace

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZonaValida());

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            _mockAsientoRepo.Verify(
                r => r.InsertAsync(It.IsAny<Dominio.Entidades.Asiento>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_AsientoDuplicado_DeberiaLanzarEventoException()
        [Fact]
        public async Task Handle_AsientoDuplicado_DeberiaLanzarEventoException()
        {
            // ARRANGE
            var command = BuildBaseCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZonaValida());

            // Ya existe un asiento con ese label
            _mockAsientoRepo
                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, command.Label, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dominio.Entidades.Asiento { Id = Guid.NewGuid() });

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventoException>(
                () => _handler.Handle(command, CancellationToken.None));

            _mockAsientoRepo.Verify(
                r => r.InsertAsync(It.IsAny<Dominio.Entidades.Asiento>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_InsertAsyncFalla_DeberiaLanzarCrearAsientoHandlerException()
        [Fact]
        public async Task Handle_InsertAsyncFalla_DeberiaLanzarCrearAsientoHandlerException()
        {
            // ARRANGE
            var command = BuildBaseCommand();

            _mockZonaRepo
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildZonaValida());

            _mockAsientoRepo
                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, command.Label, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Dominio.Entidades.Asiento)null);

            var exDb = new InvalidOperationException("Error de conexión al guardar asiento.");
            _mockAsientoRepo
                .Setup(r => r.InsertAsync(It.IsAny<Dominio.Entidades.Asiento>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exDb);

            // ACT & ASSERT
            await Assert.ThrowsAsync<CrearAsientoHandlerException>(
                () => _handler.Handle(command, CancellationToken.None));
        }
        #endregion
    }
}
