using EventsService.Aplicacion.Commands.EliminarEscenario;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using MediatR;
using Moq;

namespace EventsService.Test.Aplicacion.CommandHandlers.Escenarios
{
    public class CommandHandler_EliminarEscenario_Tests
    {
        private readonly Mock<IScenarioRepository> MockScenarioRepo;
        private readonly Mock<ILog> MockLog;
        private readonly EliminarEscenarioHandler Handler;

        // --- DATOS ---
        private readonly string escenarioId;
        private readonly EliminarEscenarioCommand command;

        public CommandHandler_EliminarEscenario_Tests()
        {
            MockScenarioRepo = new Mock<IScenarioRepository>();
            MockLog = new Mock<ILog>();

            Handler = new EliminarEscenarioHandler(MockScenarioRepo.Object, MockLog.Object);

            escenarioId = Guid.NewGuid().ToString();
            command = new EliminarEscenarioCommand(escenarioId);
        }

        #region Handle_ValidRequest_ShouldCallRepositoryAndReturnUnit()
        [Fact]
        public async Task Handle_ValidRequest_ShouldCallRepositoryAndReturnUnit()
        {
            // ARRANGE
            MockScenarioRepo
                .Setup(r => r.EliminarEscenario(escenarioId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // ACT
            var result = await Handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.Equal(Unit.Value, result);

            MockScenarioRepo.Verify(r =>
                    r.EliminarEscenario(escenarioId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_RepositoryThrows_ShouldThrowEliminarEscenarioHandlerException()
        [Fact]
        public async Task Handle_RepositoryThrows_ShouldThrowEliminarEscenarioHandlerException()
        {
            // ARRANGE
            var dbException = new InvalidOperationException("Simulated DB failure.");

            MockScenarioRepo
                .Setup(r => r.EliminarEscenario(escenarioId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbException);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<EliminarEscenarioHandlerException>(() =>
                Handler.Handle(command, CancellationToken.None));

            Assert.Equal(dbException, ex.InnerException);
        }
        #endregion
    }
}
