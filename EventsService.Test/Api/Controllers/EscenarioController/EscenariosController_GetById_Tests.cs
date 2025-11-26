using Api.Controllers;
using EventsService.Api.Contracs.Escenario;
using EventsService.Aplicacion.DTOs.Escenario;
using EventsService.Aplicacion.Queries.ObtenerEscenario;
using EventsService.Dominio.Excepciones;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.EscenarioController
{
    public class EscenariosController_GetById_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<ILog> MockLogger;
        private readonly EscenariosController Controller;

        // --- DATOS ---
        private readonly string EscenarioId = "escenario_123"; // el controller espera string

        public EscenariosController_GetById_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockLogger = new Mock<ILog>();

            Controller = new EscenariosController(MockMediator.Object, MockLogger.Object);
        }

        #region GetById_Existe_Retorna200Ok
        [Fact]
        public async Task GetById_Existe_Retorna200Ok()
        {
            // ARRANGE: usamos el record EscenarioDto REAL (constructor posicional)
            var dto = new EscenarioDto(
                Id: Guid.NewGuid(),
                Nombre: "Teatro UCAB",
                Descripcion: "Principal",
                Ubicacion: "Av. Principal",
                Ciudad: "Caracas",
                Estado: "DC",
                Pais: "Venezuela",
                CapacidadTotal: 500,
                Activo: true
            );

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<ObtenerEscenarioQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // ACT
            var result = await Controller.GetById(EscenarioId, CancellationToken.None);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var resp = Assert.IsType<EscenarioResponse>(okResult.Value);
            Assert.Equal(dto.Id, resp.Id);
            Assert.Equal(dto.Nombre, resp.Nombre);
            Assert.Equal(dto.Ciudad, resp.Ciudad);
            Assert.Equal(dto.Pais, resp.Pais);

            MockMediator.Verify(m => m.Send(
                    It.Is<ObtenerEscenarioQuery>(q => q.Id == EscenarioId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region GetById_NoExiste_LanzaNotFoundException
        [Fact]
        public async Task GetById_NoExiste_LanzaNotFoundException()
        {
            // ARRANGE: devolvemos null del tipo correcto
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<ObtenerEscenarioQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((EscenarioDto?)null);

            // ACT + ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() =>
                Controller.GetById(EscenarioId, CancellationToken.None));

            MockMediator.Verify(m => m.Send(
                    It.Is<ObtenerEscenarioQuery>(q => q.Id == EscenarioId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
