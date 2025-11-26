using EventsService.Aplicacion.Commands.EliminarEvento;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using Moq;

namespace EventsService.Test.Aplicacion.CommandHandlers.Eventos
{
    public class CommandHandler_DeleteEvent_Tests
    {
        private readonly Mock<IEventRepository> MockEventRepo;
        private readonly Mock<ILog> MockLog;
        private readonly DeleteEventHandler Handler;

        // --- DATOS ---
        private readonly Guid eventId;
        private readonly DeleteEventCommand command;

        public CommandHandler_DeleteEvent_Tests()
        {
            MockEventRepo = new Mock<IEventRepository>();
            MockLog = new Mock<ILog>();

            Handler = new DeleteEventHandler(MockEventRepo.Object, MockLog.Object);

            eventId = Guid.NewGuid();
            command = new DeleteEventCommand(eventId);
        }

        #region Handle_ExistingEvent_ShouldReturnTrue()
        [Fact]
        public async Task Handle_ExistingEvent_ShouldReturnTrue()
        {
            // ARRANGE
            MockEventRepo
                .Setup(r => r.DeleteAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await Handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            MockEventRepo.Verify(r => r.DeleteAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion

        #region Handle_NotFound_ShouldReturnFalse()
        [Fact]
        public async Task Handle_NotFound_ShouldReturnFalse()
        {
            // ARRANGE
            MockEventRepo
                .Setup(r => r.DeleteAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT
            var result = await Handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            MockEventRepo.Verify(r => r.DeleteAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion

        #region Handle_RepositoryThrows_ShouldThrowDeleteEventHandlerException()
        [Fact]
        public async Task Handle_RepositoryThrows_ShouldThrowDeleteEventHandlerException()
        {
            // ARRANGE
            var dbException = new InvalidOperationException("Simulated DB failure.");

            MockEventRepo
                .Setup(r => r.DeleteAsync(eventId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbException);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<DeleteEventHandlerException>(() =>
                Handler.Handle(command, CancellationToken.None));

            Assert.Equal(dbException, ex.InnerException);
        }
        #endregion
    }
}
