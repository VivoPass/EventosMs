using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.Zonas.CrearZonaEvento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.ValueObjects;
using log4net;
using Moq;
using Xunit;

namespace EventsService.Test.Aplicacion.CommandHandlers.Zonas
{
    public class CreateZonaEventoHandler_Tests
    {
        private readonly Mock<IZonaEventoRepository> _mockZonaRepo;
        private readonly Mock<IEscenarioZonaRepository> _mockEscenarioZonaRepo;
        private readonly Mock<IAsientoRepository> _mockAsientoRepo;
        private readonly Mock<IScenarioRepository> _mockEscenarioRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly CreateZonaEventoHandler _handler;

        // --- DATOS BASE ---
        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _escenarioId = Guid.NewGuid();

        public CreateZonaEventoHandler_Tests()
        {
            _mockZonaRepo = new Mock<IZonaEventoRepository>();
            _mockEscenarioZonaRepo = new Mock<IEscenarioZonaRepository>();
            _mockAsientoRepo = new Mock<IAsientoRepository>();
            _mockEscenarioRepo = new Mock<IScenarioRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new CreateZonaEventoHandler(
                _mockZonaRepo.Object,
                _mockEscenarioZonaRepo.Object,
                _mockAsientoRepo.Object,
                _mockEscenarioRepo.Object,
                _mockLog.Object);
        }

        private CreateZonaEventoCommand BuildBaseCommand(
            string tipo = "general",
            bool autogenerar = false,
            int? filas = null,
            int? columnas = null,
            int capacidad = 9)
        {
            // Ajusta el tipo de Numeracion y Grid según tus clases reales
            object numeracion = null!;
            if (filas.HasValue && columnas.HasValue)
            {
                // Ejemplo si tu tipo es ZonaNumeracion:
                // numeracion = new ZonaNumeracion
                // {
                //     Filas = filas.Value,
                //     Columnas = columnas.Value,
                //     Modo = "grid",
                //     PrefijoFila = "F",
                //     PrefijoAsiento = "A"
                // };
            }

            object grid = null!;
            // Ejemplo si tienes un tipo GridDto / ZonaGrid:
            // grid = new GridDto { StartRow = 0, StartCol = 0, RowSpan = 5, ColSpan = 10 };

            var cmd = new CreateZonaEventoCommand
            {
                EventId = _eventId,
                EscenarioId = _escenarioId,
                Nombre = "Zona Test",
                Tipo = tipo,
                Capacidad = capacidad,
                // casteos para que el compilador esté feliz cuando pongas tu tipo real
                Numeracion = (dynamic)numeracion!,
                Precio = 50m,
                Estado = "Activo",
                Grid = (dynamic)grid!,
                AutogenerarAsientos = autogenerar
            };

            return cmd;
        }

        #region Handle_Valido_SinAutogenerar_DeberiaCrearZonaYEscenarioZonaYRetornarId()
        [Fact]
        public async Task Handle_Valido_SinAutogenerar_DeberiaCrearZonaYEscenarioZonaYRetornarId()
        {
            // ARRANGE
            var command = BuildBaseCommand(
                tipo: "general",
                autogenerar: false,
                filas: null,
                columnas: null,
                capacidad: 100);

            // Escenario existe
            _mockEscenarioRepo
                .Setup(r => r.ObtenerEscenario(command.EscenarioId.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Escenario { Id = command.EscenarioId });

            // No hay nombre duplicado
            _mockZonaRepo
                .Setup(r => r.ExistsByNombreAsync(command.EventId, command.Nombre, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Guid zonaIdCapturado = Guid.Empty;

            _mockZonaRepo
                .Setup(r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
                .Callback<ZonaEvento, CancellationToken>((z, _) => zonaIdCapturado = z.Id)
                .Returns(Task.CompletedTask);

            _mockEscenarioZonaRepo
                .Setup(r => r.AddAsync(It.IsAny<EscenarioZona>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // ACT
            var resultId = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.NotEqual(Guid.Empty, resultId);
            Assert.Equal(zonaIdCapturado, resultId);

            _mockEscenarioRepo.Verify(
                r => r.ObtenerEscenario(command.EscenarioId.ToString(), It.IsAny<CancellationToken>()),
                Times.Once);

            _mockZonaRepo.Verify(
                r => r.ExistsByNombreAsync(command.EventId, command.Nombre, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockZonaRepo.Verify(
                r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _mockEscenarioZonaRepo.Verify(
                r => r.AddAsync(It.IsAny<EscenarioZona>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _mockAsientoRepo.Verify(
                r => r.BulkInsertAsync(It.IsAny<IReadOnlyCollection<Dominio.Entidades.Asiento>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_SentadoConFilasOColumnasInvalidas_DeberiaLanzarEventoException()
        [Fact]
        public async Task Handle_SentadoConFilasOColumnasInvalidas_DeberiaLanzarEventoException()
        {
            // ARRANGE
            var command = BuildBaseCommand(
                tipo: "sentado",
                autogenerar: false,
                filas: 0,          // inválido
                columnas: 5,
                capacidad: 100);

            _mockEscenarioRepo
                .Setup(r => r.ObtenerEscenario(command.EscenarioId.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Escenario { Id = command.EscenarioId });

            _mockZonaRepo
                .Setup(r => r.ExistsByNombreAsync(command.EventId, command.Nombre, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(command, CancellationToken.None));

            _mockZonaRepo.Verify(
                r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_RepositorioLanzaException_DeberiaLanzarCreateZonaEventoHandlerException()
        [Fact]
        public async Task Handle_RepositorioLanzaException_DeberiaLanzarCreateZonaEventoHandlerException()
        {
            // ARRANGE
            var command = BuildBaseCommand();

            _mockEscenarioRepo
                .Setup(r => r.ObtenerEscenario(command.EscenarioId.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Escenario { Id = command.EscenarioId });

            _mockZonaRepo
                .Setup(r => r.ExistsByNombreAsync(command.EventId, command.Nombre, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var dbException = new InvalidOperationException("Simulated DB error");
            _mockZonaRepo
                .Setup(r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbException);

            // ACT & ASSERT
            await Assert.ThrowsAsync<CreateZonaEventoHandlerException>(
                () => _handler.Handle(command, CancellationToken.None));
        }
        #endregion


        [Fact]
        public async Task prueb()
        {
            var command = BuildBaseCommand();
            var ex = Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
