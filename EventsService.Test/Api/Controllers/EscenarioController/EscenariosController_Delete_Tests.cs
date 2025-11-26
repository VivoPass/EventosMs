using Api.Controllers;
using EventsService.Aplicacion.Commands.EliminarEscenario;
using EventsService.Dominio.Excepciones;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.EscenarioController
{
    public class EscenariosController_Delete_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<ILog> MockLogger;
        private readonly EscenariosController Controller;

        // --- DATOS ---
        private readonly string EscenarioId = "escenario_123";

        public EscenariosController_Delete_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockLogger = new Mock<ILog>();

            Controller = new EscenariosController(MockMediator.Object, MockLogger.Object);
        }

        #region Delete_Exitoso_Retorna204NoContent
        [Fact]
        public async Task Delete_Exitoso_Retorna204NoContent()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<EliminarEscenarioCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // ACT
            var result = await Controller.Delete(EscenarioId, CancellationToken.None);

            // ASSERT
            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);

            MockMediator.Verify(m => m.Send(
                    It.Is<EliminarEscenarioCommand>(c => c.Id == EscenarioId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region Delete_NoExisteEscenario_LanzaNotFoundException
        [Fact]
        public async Task Delete_NoExisteEscenario_LanzaNotFoundException()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<EliminarEscenarioCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Escenario", EscenarioId));

            // ACT + ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() =>
                Controller.Delete(EscenarioId, CancellationToken.None));
        }
        #endregion
    }
}

