using EventsService.Aplicacion.Commands.ModificarEvento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using log4net;
using Moq;

namespace EventsService.Test.Aplicacion.CommandHandlers.Eventos
{
    public class CommandHandler_UpdateEvent_Tests
    {
        private readonly Mock<IEventRepository> MockEventRepo;
        private readonly Mock<ICategoryRepository> MockCategoryRepo;
        private readonly Mock<IScenarioRepository> MockScenarioRepo;
        private readonly Mock<ILog> MockLog;
        private readonly UpdateEventHandler Handler;

        // --- DATOS ---
        private readonly Guid eventId;
        private readonly Guid categoriaOriginalId;
        private readonly Guid escenarioOriginalId;
        private readonly Guid categoriaNuevaId;
        private readonly Guid escenarioNuevoId;

        private readonly UpdateEventCommand commandFullUpdate;

        public CommandHandler_UpdateEvent_Tests()
        {
            MockEventRepo = new Mock<IEventRepository>();
            MockCategoryRepo = new Mock<ICategoryRepository>();
            MockScenarioRepo = new Mock<IScenarioRepository>();
            MockLog = new Mock<ILog>();

            Handler = new UpdateEventHandler(
                MockEventRepo.Object,
                MockCategoryRepo.Object,
                MockScenarioRepo.Object,
                MockLog.Object
            );

            eventId = Guid.NewGuid();
            categoriaOriginalId = Guid.NewGuid();
            escenarioOriginalId = Guid.NewGuid();
            categoriaNuevaId = Guid.NewGuid();
            escenarioNuevoId = Guid.NewGuid();

            // Comando con todos los campos llenos para cubrir el máximo de ramas
            commandFullUpdate = new UpdateEventCommand(
                Id: eventId,
                Nombre: "Nombre actualizado",
                CategoriaId: categoriaNuevaId,
                EscenarioId: escenarioNuevoId,
                Inicio: DateTimeOffset.UtcNow.AddDays(2),
                Fin: DateTimeOffset.UtcNow.AddDays(2).AddHours(3),
                AforoMaximo: 900,
                Tipo: "Conferencia",
                Lugar: "Online",
                Descripcion: "Evento actualizado"
            );
        }

        private Evento CreateExistingEvent()
        {
            return new Evento
            {
                Id = eventId,
                Nombre = "Nombre original",
                CategoriaId = categoriaOriginalId,
                EscenarioId = escenarioOriginalId,
                Inicio = DateTimeOffset.UtcNow.AddDays(1),
                Fin = DateTimeOffset.UtcNow.AddDays(1).AddHours(2),
                AforoMaximo = 500,
                Tipo = "Concierto",
                Lugar = "Caracas",
                Descripcion = "Descripcion original",
                OrganizadorId = Guid.NewGuid()
            };
        }

        #region Handle_EventNotFound_ShouldReturnFalse()
        [Fact]
        public async Task Handle_EventNotFound_ShouldReturnFalse()
        {
            // ARRANGE
            MockEventRepo
                .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Evento?)null);

            // ACT
            var result = await Handler.Handle(commandFullUpdate, CancellationToken.None);

            // ASSERT
            Assert.False(result);

            MockCategoryRepo.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            MockScenarioRepo.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            MockEventRepo.Verify(r => r.UpdateAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        #region Handle_ValidUpdate_ShouldUpdateAndReturnTrue()
        [Fact]
        public async Task Handle_ValidUpdate_ShouldUpdateAndReturnTrue()
        {
            // ARRANGE
            var existing = CreateExistingEvent();

            MockEventRepo
                .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            MockCategoryRepo
                .Setup(r => r.ExistsAsync(categoriaNuevaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockScenarioRepo
                .Setup(r => r.ExistsAsync(escenarioNuevoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockEventRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await Handler.Handle(commandFullUpdate, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            // Verificamos que el objeto en memoria fue actualizado con los datos del comando
            Assert.Equal(commandFullUpdate.Nombre!.Trim(), existing.Nombre);
            Assert.Equal(commandFullUpdate.CategoriaId!.Value, existing.CategoriaId);
            Assert.Equal(commandFullUpdate.EscenarioId!.Value, existing.EscenarioId);
            Assert.Equal(commandFullUpdate.Inicio!.Value, existing.Inicio);
            Assert.Equal(commandFullUpdate.Fin!.Value, existing.Fin);
            Assert.Equal(commandFullUpdate.AforoMaximo!.Value, existing.AforoMaximo);
            Assert.Equal(commandFullUpdate.Tipo, existing.Tipo);
            Assert.Equal(commandFullUpdate.Lugar, existing.Lugar);
            Assert.Equal(commandFullUpdate.Descripcion, existing.Descripcion);

            MockEventRepo.Verify(r => r.UpdateAsync(
                    It.Is<Evento>(e => e.Id == eventId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Handle_CategoryDoesNotExist_ShouldThrowEventoException()
        [Fact]
        public async Task Handle_CategoryDoesNotExist_ShouldThrowEventoException()
        {
            // ARRANGE
            var existing = CreateExistingEvent();

            var commandWithCategoryChange = commandFullUpdate with { CategoriaId = categoriaNuevaId };

            MockEventRepo
                .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            MockCategoryRepo
                .Setup(r => r.ExistsAsync(categoriaNuevaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventoException>(() =>
                Handler.Handle(commandWithCategoryChange, CancellationToken.None));

            MockScenarioRepo.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            MockEventRepo.Verify(r => r.UpdateAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        #region Handle_ScenarioDoesNotExist_ShouldThrowEventoException()
        [Fact]
        public async Task Handle_ScenarioDoesNotExist_ShouldThrowEventoException()
        {
            // ARRANGE
            var existing = CreateExistingEvent();

            var commandWithScenarioChange = commandFullUpdate with
            {
                CategoriaId = null,            // para que no pase por validación de categoría
                EscenarioId = escenarioNuevoId
            };

            MockEventRepo
                .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            MockScenarioRepo
                .Setup(r => r.ExistsAsync(escenarioNuevoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventoException>(() =>
                Handler.Handle(commandWithScenarioChange, CancellationToken.None));

            MockEventRepo.Verify(r => r.UpdateAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        #region Handle_UpdateAsyncThrowsUnexpectedException_ShouldThrowUpdateEventHandlerException()
        [Fact]
        public async Task Handle_UpdateAsyncThrowsUnexpectedException_ShouldThrowUpdateEventHandlerException()
        {
            // ARRANGE
            var existing = CreateExistingEvent();

            MockEventRepo
                .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            MockCategoryRepo
                .Setup(r => r.ExistsAsync(categoriaNuevaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockScenarioRepo
                .Setup(r => r.ExistsAsync(escenarioNuevoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var dbException = new InvalidOperationException("Simulated DB failure.");
            MockEventRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbException);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<UpdateEventHandlerException>(() =>
                Handler.Handle(commandFullUpdate, CancellationToken.None));

            Assert.Equal(dbException, ex.InnerException);
        }
        #endregion
    }
}
