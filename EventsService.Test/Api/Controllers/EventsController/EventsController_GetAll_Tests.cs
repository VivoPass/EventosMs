
using EventsService.Api.Controllers;
using EventsService.Aplicacion.Queries.ObtenerTodosEventos;
using EventsService.Dominio.Entidades;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EventsService.Test.Api.Controllers.EventsController
{
    public class EventsController_GetAll_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IHttpClientFactory> MockHttpFactory;
        private readonly Mock<ILog> MockLogger;
        private readonly EventsService.Api.Controllers.EventsController Controller;

        // --- DATOS ---
        private readonly List<Evento> ListaEventos;

        public EventsController_GetAll_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockHttpFactory = new Mock<IHttpClientFactory>();
            MockLogger = new Mock<ILog>();

            // HttpClientFactory no se usa en este método, pero lo inyectamos igual
            MockHttpFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            Controller = new EventsService.Api.Controllers.EventsController(
                MockMediator.Object,
                MockHttpFactory.Object,
                MockLogger.Object
            );

            ListaEventos = new List<Evento>
            {
                new Evento
                {
                    Id = Guid.NewGuid(),
                    OrganizadorId = Guid.NewGuid(),
                    Nombre = "Evento 1"
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    OrganizadorId = Guid.NewGuid(),
                    Nombre = "Evento 2"
                }
            };
        }

        // ---------------------------------------------------------------------
        #region GetAll_ConEventos_Retorna200OkYLista
        [Fact]
        public async Task GetAll_ConEventos_Retorna200OkYLista()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<GetAllEventsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ListaEventos);

            // ACT
            var result = await Controller.GetAll(CancellationToken.None);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var value = Assert.IsAssignableFrom<IEnumerable<Evento>>(okResult.Value);
            Assert.Equal(ListaEventos.Count, value.Count());

            MockMediator.Verify(
                m => m.Send(It.IsAny<GetAllEventsQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);

            MockLogger.Verify(l => l.Info(It.IsAny<object>()), Times.Once);
            MockLogger.Verify(l => l.Debug(It.IsAny<object>()), Times.Once);
        }
        #endregion

        // ---------------------------------------------------------------------
        #region GetAll_SinEventos_Retorna200OkYListaVacia
        [Fact]
        public async Task GetAll_SinEventos_Retorna200OkYListaVacia()
        {
            // ARRANGE
            var listaVacia = new List<Evento>();

            MockMediator
                .Setup(m => m.Send(It.IsAny<GetAllEventsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(listaVacia);

            // ACT
            var result = await Controller.GetAll(CancellationToken.None);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var value = Assert.IsAssignableFrom<IEnumerable<Evento>>(okResult.Value);
            Assert.Empty(value);

            MockMediator.Verify(
                m => m.Send(It.IsAny<GetAllEventsQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);

            
            MockLogger.Verify(
                l => l.Debug(It.Is<object>(o => o!.ToString()!.Contains("0 eventos"))),
                Times.Once);
        }
        #endregion
    }
}
