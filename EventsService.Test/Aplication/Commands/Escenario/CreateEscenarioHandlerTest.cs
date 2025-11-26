using EventsService.Aplicacion.Commands.CrearEscenario;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using Moq;

namespace EventsService.Test.Aplicacion.CommandHandlers.Escenarios
{
    public class CommandHandler_CreateEscenario_Tests
    {
        private readonly Mock<IScenarioRepository> MockScenarioRepo;
        private readonly Mock<ILog> MockLog;
        private readonly CreateEscenarioHandler Handler;

        // --- DATOS ---
        private readonly string expectedId;
        private readonly CreateEscenarioCommand commandBase;

        public CommandHandler_CreateEscenario_Tests()
        {
            MockScenarioRepo = new Mock<IScenarioRepository>();
            MockLog = new Mock<ILog>();

            Handler = new CreateEscenarioHandler(MockScenarioRepo.Object, MockLog.Object);

            expectedId = Guid.NewGuid().ToString();

            commandBase = new CreateEscenarioCommand(
                Nombre: "   Escenario Principal   ",  // con espacios para validar Trim()
                Descripcion: "Escenario para conciertos y eventos masivos",
                Ubicacion: "Av. Principal con Calle 1",
                Ciudad: "Caracas",
                Estado: "Distrito Capital",
                Pais: "Venezuela"
            );
        }

        #region Handle_ValidRequest_ShouldCreateEscenarioAndReturnId()
        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateEscenarioAndReturnId()
        {
            // ARRANGE
            Escenario? capturado = null;

            MockScenarioRepo
                .Setup(r => r.CrearAsync(It.IsAny<Escenario>(), It.IsAny<CancellationToken>()))
                .Callback<Escenario, CancellationToken>((e, _) =>
                {
                    capturado = e;
                })
                .ReturnsAsync(expectedId);

            // ACT
            var resultId = await Handler.Handle(commandBase, CancellationToken.None);

            // ASSERT
            Assert.Equal(expectedId, resultId);

            Assert.NotNull(capturado);
            Assert.NotEqual(Guid.Empty, capturado!.Id);
            Assert.Equal(commandBase.Nombre.Trim(), capturado.Nombre);
            Assert.Equal(commandBase.Descripcion, capturado.Descripcion);
            Assert.Equal(commandBase.Ubicacion, capturado.Ubicacion);
            Assert.Equal(commandBase.Ciudad, capturado.Ciudad);
            Assert.Equal(commandBase.Estado, capturado.Estado);
            Assert.Equal(commandBase.Pais, capturado.Pais);

            MockScenarioRepo.Verify(r => r.CrearAsync(
                    It.Is<Escenario>(e =>
                        e.Nombre == commandBase.Nombre.Trim() &&
                        e.Ciudad == commandBase.Ciudad &&
                        e.Pais == commandBase.Pais),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_RepositoryThrows_ShouldThrowCreateEscenarioHandlerException()
        [Fact]
        public async Task Handle_RepositoryThrows_ShouldThrowCreateEscenarioHandlerException()
        {
            // ARRANGE
            var dbException = new InvalidOperationException("Simulated DB failure.");

            MockScenarioRepo
                .Setup(r => r.CrearAsync(It.IsAny<Escenario>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbException);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<CreateEscenarioHandlerException>(() =>
                Handler.Handle(commandBase, CancellationToken.None));

            Assert.Equal(dbException, ex.InnerException);
        }
        #endregion
    }
}
