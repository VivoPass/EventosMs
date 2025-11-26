using Api.Controllers;
using EventsService.Api.Contracs.Escenario;
using EventsService.Aplicacion.Commands.ModificarEscenario;
using EventsService.Dominio.Excepciones;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.EscenarioController
{
    public class EscenariosController_Update_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<ILog> MockLogger;
        private readonly EscenariosController Controller;

        // --- DATOS ---
        private readonly string EscenarioId = "escenario_123";
        private readonly EscenarioUpdateRequest ValidRequest;

        public EscenariosController_Update_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockLogger = new Mock<ILog>();

            Controller = new EscenariosController(MockMediator.Object, MockLogger.Object);

            ValidRequest = new EscenarioUpdateRequest(
                Nombre: "Teatro actualizado",
                Descripcion: "Escenario renovado",
                Ubicacion: "Nueva ubicación",
                Ciudad: "Caracas",
                Estado: "DC",
                Pais: "Venezuela"
            );
        }

        #region Update_Exitoso_Retorna204NoContent
        [Fact]
        public async Task Update_Exitoso_Retorna204NoContent()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<ModificarEscenarioCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // ACT
            var result = await Controller.Update(EscenarioId, ValidRequest, CancellationToken.None);

            // ASSERT
            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);

            MockMediator.Verify(m => m.Send(
                    It.Is<ModificarEscenarioCommand>(c =>
                        c.Id == EscenarioId &&
                        c.Nombre == ValidRequest.Nombre &&
                        c.Ciudad == ValidRequest.Ciudad),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Update_NoExisteEscenario_LanzaNotFoundException
        [Fact]
        public async Task Update_NoExisteEscenario_LanzaNotFoundException()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<ModificarEscenarioCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Escenario", EscenarioId));

            // ACT + ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() =>
                Controller.Update(EscenarioId, ValidRequest, CancellationToken.None));
        }
        #endregion
    }
}
