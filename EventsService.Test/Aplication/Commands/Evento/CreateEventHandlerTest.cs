using EventsService.Aplicacion.Commands.CrearEvento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using Moq;

namespace EventsService.Test.Aplicacion.CommandHandlers.Eventos
{
    public class CommandHandler_CreateEvent_Tests
    {
        private readonly Mock<IEventRepository> MockEventRepo;
        private readonly Mock<ICategoryRepository> MockCategoryRepo;
        private readonly Mock<IScenarioRepository> MockScenarioRepo;
        private readonly Mock<ILog> MockLog;
        private readonly CreateEventHandler Handler;

        // --- DATOS ---
        private readonly Guid categoriaId;
        private readonly Guid escenarioId;
        private readonly Guid organizadorId;
        private readonly CreateEventCommand command;
        private readonly CreateEventCommand commandPrueba;

        public CommandHandler_CreateEvent_Tests()
        {
            MockEventRepo = new Mock<IEventRepository>();
            MockCategoryRepo = new Mock<ICategoryRepository>();
            MockScenarioRepo = new Mock<IScenarioRepository>();
            MockLog = new Mock<ILog>();

            Handler = new CreateEventHandler(
                MockEventRepo.Object,
                MockCategoryRepo.Object,
                MockScenarioRepo.Object,
                MockLog.Object
            );

            categoriaId = Guid.NewGuid();
            escenarioId = Guid.NewGuid();
            organizadorId = Guid.NewGuid();

            command = new CreateEventCommand(
                Nombre: "Concierto de prueba",
                CategoriaId: categoriaId,
                EscenarioId: escenarioId,
                Inicio: DateTimeOffset.UtcNow.AddDays(1),
                Fin: DateTimeOffset.UtcNow.AddDays(1).AddHours(2),
                AforoMaximo: 500,
                Tipo: "Concierto",
                Lugar: "Caracas",
                Descripcion: "Evento de integración",
                OrganizadorId: organizadorId,
                OnlineMeetingUrl: "https://www.youtube.com/?gl=ES&hl=es"
            );

            commandPrueba = new CreateEventCommand(
                Nombre: "Concierto de prueba",
                CategoriaId: categoriaId,
                EscenarioId: escenarioId,
                Inicio: DateTimeOffset.UtcNow.AddDays(1),
                Fin: DateTimeOffset.UtcNow.AddDays(1).AddHours(2),
                AforoMaximo: 8,
                Tipo: "Concierto",
                Lugar: "Caracas",
                Descripcion: "Evento de integración",
                OrganizadorId: organizadorId,
                OnlineMeetingUrl: "https://www.youtube.com/?gl=ES&hl=es"
            );
        }

        #region Handle_ValidRequest_ShouldCreateEventAndReturnId()
        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateEventAndReturnId()
        {
            // ARRANGE
            MockCategoryRepo
                .Setup(r => r.ExistsAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockScenarioRepo
                .Setup(r => r.ExistsAsync(escenarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Guid capturedId = Guid.Empty;

            MockEventRepo
                .Setup(r => r.InsertAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
                .Callback<Evento, CancellationToken>((e, _) =>
                {
                    capturedId = e.Id;
                })
                .Returns(Task.CompletedTask);

            // ACT
            var resultId = await Handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.NotEqual(Guid.Empty, resultId);
            Assert.Equal(capturedId, resultId);

            MockEventRepo.Verify(r => r.InsertAsync(
                    It.Is<Evento>(e =>
                        e.Nombre == command.Nombre.Trim() &&
                        e.CategoriaId == command.CategoriaId &&
                        e.EscenarioId == command.EscenarioId &&
                        e.AforoMaximo == command.AforoMaximo &&
                        e.OrganizadorId == command.OrganizadorId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_CategoryDoesNotExist_ShouldThrowEventoException()
        [Fact]
        public async Task Handle_CategoryDoesNotExist_ShouldThrowEventoException()
        {
            // ARRANGE
            MockCategoryRepo
                .Setup(r => r.ExistsAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventoException>(() =>
                Handler.Handle(command, CancellationToken.None));

            MockScenarioRepo.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            MockEventRepo.Verify(r => r.InsertAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        #region Handle_ScenarioDoesNotExist_ShouldThrowEventoException()
        [Fact]
        public async Task Handle_ScenarioDoesNotExist_ShouldThrowEventoException()
        {
            // ARRANGE
            MockCategoryRepo
                .Setup(r => r.ExistsAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockScenarioRepo
                .Setup(r => r.ExistsAsync(escenarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventoException>(() =>
                Handler.Handle(command, CancellationToken.None));

            MockEventRepo.Verify(r => r.InsertAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        #region Handle_PersistenceThrowsUnexpectedException_ShouldThrowCreateEventHandlerException()
        [Fact]
        public async Task Handle_PersistenceThrowsUnexpectedException_ShouldThrowCreateEventHandlerException()
        {
            // ARRANGE
            MockCategoryRepo
                .Setup(r => r.ExistsAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockScenarioRepo
                .Setup(r => r.ExistsAsync(escenarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var dbException = new InvalidOperationException("Falla simulada en BD.");
            MockEventRepo
                .Setup(r => r.InsertAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbException);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<CreateEventHandlerException>(() =>
                Handler.Handle(command, CancellationToken.None));

            Assert.Equal(dbException, ex.InnerException);
        }
        #endregion


        [Fact]
        public async Task CrearEvento_MenosDe10Aforo_RetornaExcepcion()
        {
            MockCategoryRepo
                .Setup(r => r.ExistsAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockScenarioRepo
                .Setup(r => r.ExistsAsync(escenarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);


            var ex = Assert.ThrowsAsync<EventoException>(() => Handler.Handle(commandPrueba, CancellationToken.None));

            Assert.Equal("El aforo tiene que ser mayor que 10", ex.Result.Message);
        }
    }
}
