using Api.Controllers;
using EventsService.Api.Contracs.Escenario;
using EventsService.Aplicacion.DTOs.Escenario;
using EventsService.Aplicacion.NewFolder;
using EventsService.Aplicacion.Queries.ObtenerEscenarios;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.EscenarioController
{
    public class EscenariosController_Search_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<ILog> MockLogger;
        private readonly EscenariosController Controller;

        public EscenariosController_Search_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockLogger = new Mock<ILog>();

            Controller = new EscenariosController(MockMediator.Object, MockLogger.Object);
        }

        // ---------------------------------------------------------------------
        #region Search_ConResultados_Retorna200OkYListaPaginada
        [Fact]
        public async Task Search_ConResultados_Retorna200OkYListaPaginada()
        {
            // ARRANGE: lista de EscenarioDto (TIPO REAL)
            var itemsDto = new List<EscenarioDto>
            {
                new EscenarioDto(
                    Id: Guid.NewGuid(),
                    Nombre: "Teatro 1",
                    Descripcion: "Desc 1",
                    Ubicacion: "Ubicacion 1",
                    Ciudad: "Caracas",
                    Estado: "DC",
                    Pais: "Venezuela",
                    CapacidadTotal: 300,
                    Activo: true
                ),
                new EscenarioDto(
                    Id: Guid.NewGuid(),
                    Nombre: "Teatro 2",
                    Descripcion: "Desc 2",
                    Ubicacion: "Ubicacion 2",
                    Ciudad: "Caracas",
                    Estado: "DC",
                    Pais: "Venezuela",
                    CapacidadTotal: 400,
                    Activo: false
                )
            };

            var pagedDto = new PagedResult<EscenarioDto>(
                Items: itemsDto,
                Total: 2,
                Page: 1,
                PageSize: 20
            );

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<ObtenerEscenariosQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedDto);

            // ACT
            var result = await Controller.Search("Teatro", "Caracas", null, 1, 20, CancellationToken.None);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var respPaged = Assert.IsType<PagedResult<EscenarioResponse>>(okResult.Value);
            Assert.Equal(2, respPaged.Total);
            Assert.Equal(1, respPaged.Page);
            Assert.Equal(20, respPaged.PageSize);
            Assert.Equal(2, respPaged.Items.Count);

            // Validamos que el mapeo base se vea coherente
            var firstResp = respPaged.Items[0];
            Assert.Equal(itemsDto[0].Id, firstResp.Id);
            Assert.Equal(itemsDto[0].Nombre, firstResp.Nombre);
            Assert.Equal(itemsDto[0].Ciudad, firstResp.Ciudad);
            Assert.Equal(itemsDto[0].Pais, firstResp.Pais);

        }
        #endregion

        // ---------------------------------------------------------------------
        #region Search_SinResultados_Retorna200OkListaVacia
        [Fact]
        public async Task Search_SinResultados_Retorna200OkListaVacia()
        {
            // ARRANGE: PagedResult<EscenarioDto> vacío
            var emptyDto = new PagedResult<EscenarioDto>(
                Items: new List<EscenarioDto>(),
                Total: 0,
                Page: 1,
                PageSize: 20
            );

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<ObtenerEscenariosQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyDto);

            // ACT
            var result = await Controller.Search(null, null, null, 1, 20, CancellationToken.None);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var respPaged = Assert.IsType<PagedResult<EscenarioResponse>>(okResult.Value);
            Assert.Equal(0, respPaged.Total);
            Assert.Empty(respPaged.Items);

        }
        #endregion
    }
}
