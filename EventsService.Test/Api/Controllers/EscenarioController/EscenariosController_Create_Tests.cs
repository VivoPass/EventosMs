
using Api.Controllers;
using EventsService.Api.Contracs.Escenario;
using EventsService.Aplicacion.Commands.CrearEscenario;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.EscenarioController
{
    public class EscenariosController_Create_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<ILog> MockLogger;
        private readonly EscenariosController Controller;

        // --- DATOS ---
        private readonly EscenarioCreateRequest ValidRequest;
        private readonly string EscenarioId = "escenario_123";

        public EscenariosController_Create_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockLogger = new Mock<ILog>();

            Controller = new EscenariosController(MockMediator.Object, MockLogger.Object);

            ValidRequest = new EscenarioCreateRequest(
                Nombre: "Teatro UCAB",
                Descripcion: "Escenario principal",
                Ubicacion: "Av. Principal",
                Ciudad: "Caracas",
                Estado: "DC",
                Pais: "Venezuela"
            );
        }

        #region Create_CreacionExitosa_Retorna201Created
        [Fact]
        public async Task Create_CreacionExitosa_Retorna201Created()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<CreateEscenarioCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(EscenarioId);

            // ACT
            var result = await Controller.Create(ValidRequest, CancellationToken.None);

            // ASSERT
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
            Assert.Equal("GetById", created.ActionName);
            Assert.Equal(EscenarioId, created.Value);
            Assert.Equal(EscenarioId, created.RouteValues["id"]);
        }
        #endregion
    }
}
