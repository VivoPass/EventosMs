using EventsService.Aplicacion.Commands.ModificarEscenario;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using MediatR;
using Moq;

namespace EventsService.Test.Aplicacion.CommandHandlers.Escenarios
{
    public class CommandHandler_ModificarEscenario_Tests
    {
        private readonly Mock<IScenarioRepository> MockScenarioRepo;
        private readonly Mock<ILog> MockLog;
        private readonly ModificarEscenarioHandler Handler;

        // --- DATOS ---
        private readonly string escenarioId;
        private readonly ModificarEscenarioCommand commandFullUpdate;

        public CommandHandler_ModificarEscenario_Tests()
        {
            MockScenarioRepo = new Mock<IScenarioRepository>();
            MockLog = new Mock<ILog>();

            Handler = new ModificarEscenarioHandler(MockScenarioRepo.Object, MockLog.Object);

            escenarioId = Guid.NewGuid().ToString();

            commandFullUpdate = new ModificarEscenarioCommand(
                Id: escenarioId,
                Nombre: "  Escenario Actualizado  ", // probamos Trim()
                Descripcion: "Nueva descripción",
                Ubicacion: "Av. Nueva, Calle 2",
                Ciudad: "Valencia",
                Estado: "Carabobo",
                Pais: "Venezuela"
            );
        }

        private Escenario CreateExistingEscenario()
        {
            return new Escenario
            {
                Id = Guid.Parse(escenarioId),
                Nombre = "Escenario Original",
                Descripcion = "Descripcion original",
                Ubicacion = "Av. Original",
                Ciudad = "Caracas",
                Estado = "Distrito Capital",
                Pais = "Venezuela"
            };
        }

        #region Handle_ValidRequest_ShouldModifyEscenarioAndReturnUnit()
        [Fact]
        public async Task Handle_ValidRequest_ShouldModifyEscenarioAndReturnUnit()
        {
            // ARRANGE
            var current = CreateExistingEscenario();
            Escenario? capturado = null;

            MockScenarioRepo
                .Setup(r => r.ObtenerEscenario(escenarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(current);

            MockScenarioRepo
                .Setup(r => r.ModificarEscenario(escenarioId, It.IsAny<Escenario>(), It.IsAny<CancellationToken>()))
                .Callback<string, Escenario, CancellationToken>((id, e, _) =>
                {
                    capturado = e;
                })
                .Returns(Task.CompletedTask);

            // ACT
            var result = await Handler.Handle(commandFullUpdate, CancellationToken.None);

            // ASSERT
            Assert.Equal(Unit.Value, result);
            Assert.NotNull(capturado);

            // Validamos merge / mapeo
            Assert.Equal(current.Id, capturado!.Id); // mantiene el mismo Id
            Assert.Equal(commandFullUpdate.Nombre!.Trim(), capturado.Nombre);
            Assert.Equal(commandFullUpdate.Descripcion, capturado.Descripcion);
            Assert.Equal(commandFullUpdate.Ubicacion, capturado.Ubicacion);
            Assert.Equal(commandFullUpdate.Ciudad, capturado.Ciudad);
            Assert.Equal(commandFullUpdate.Estado, capturado.Estado);
            Assert.Equal(commandFullUpdate.Pais, capturado.Pais);

            MockScenarioRepo.Verify(r =>
                    r.ModificarEscenario(
                        escenarioId,
                        It.Is<Escenario>(e =>
                            e.Nombre == commandFullUpdate.Nombre.Trim() &&
                            e.Ciudad == commandFullUpdate.Ciudad &&
                            e.Pais == commandFullUpdate.Pais),
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_ScenarioNotFound_ShouldThrowEventoException()
        [Fact]
        public async Task Handle_ScenarioNotFound_ShouldThrowEventoException()
        {
            // ARRANGE
            MockScenarioRepo
                .Setup(r => r.ObtenerEscenario(escenarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Escenario?)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventoException>(() =>
                Handler.Handle(commandFullUpdate, CancellationToken.None));

            MockScenarioRepo.Verify(r =>
                    r.ModificarEscenario(It.IsAny<string>(), It.IsAny<Escenario>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_ModificarEscenarioThrows_ShouldThrowModificarEscenarioHandlerException()
        [Fact]
        public async Task Handle_ModificarEscenarioThrows_ShouldThrowModificarEscenarioHandlerException()
        {
            // ARRANGE
            var current = CreateExistingEscenario();
            var dbException = new InvalidOperationException("Simulated DB failure.");

            MockScenarioRepo
                .Setup(r => r.ObtenerEscenario(escenarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(current);

            MockScenarioRepo
                .Setup(r => r.ModificarEscenario(escenarioId, It.IsAny<Escenario>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbException);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<ModificarEscenarioHandlerException>(() =>
                Handler.Handle(commandFullUpdate, CancellationToken.None));

            Assert.Equal(dbException, ex.InnerException);
        }
        #endregion
    }
}
