
using EventsService.Api.Controllers;
using EventsService.Aplicacion.Queries.ObtenerEvento;
using EventsService.Dominio.Entidades;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.EventsController
{
    public class EventsController_GetEventById_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IHttpClientFactory> MockHttpFactory;
        private readonly Mock<ILog> MockLogger;
        private readonly EventsService.Api.Controllers.EventsController Controller;

        // --- DATOS ---
        private readonly Guid EventoId = Guid.NewGuid();
        private readonly Guid OrganizadorId = Guid.NewGuid();

        public EventsController_GetEventById_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockHttpFactory = new Mock<IHttpClientFactory>();
            MockLogger = new Mock<ILog>();

            // El HttpClientFactory no se usa en este método,
            // pero lo inyectamos igual para respetar el ctor.
            MockHttpFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            Controller = new EventsService.Api.Controllers.EventsController(
                MockMediator.Object,
                MockHttpFactory.Object,
                MockLogger.Object
            );
        }

        // ---------------------------------------------------------------------
        #region GetEventById_Existe_Retorna200Ok
        [Fact]
        public async Task GetEventById_Existe_Retorna200Ok()
        {
            // ARRANGE
            var evento = new Evento
            {
                Id = EventoId,
                OrganizadorId = OrganizadorId,
                Nombre = "Evento prueba"
            };

            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(evento);

            // ACT
            var result = await Controller.GetEventById(EventoId, CancellationToken.None);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Same(evento, okResult.Value);

            MockMediator.Verify(
                m => m.Send(It.Is<GetEventByIdQuery>(q => q.Id == EventoId),
                            It.IsAny<CancellationToken>()),
                Times.Once);

            MockLogger.Verify(l => l.Debug(It.IsAny<object>()), Times.Once);
            MockLogger.Verify(l => l.Warn(It.IsAny<object>()), Times.Never);
        }
        #endregion

        // ---------------------------------------------------------------------
        #region GetEventById_NoExiste_Retorna404NotFound
        [Fact]
        public async Task GetEventById_NoExiste_Retorna404NotFound()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Evento?)null);

            // ACT
            var result = await Controller.GetEventById(EventoId, CancellationToken.None);

            // ASSERT
            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);

            MockMediator.Verify(
                m => m.Send(It.Is<GetEventByIdQuery>(q => q.Id == EventoId),
                            It.IsAny<CancellationToken>()),
                Times.Once);

            MockLogger.Verify(l => l.Warn(It.IsAny<object>()), Times.Once);
            MockLogger.Verify(l => l.Debug(It.IsAny<object>()), Times.Never);
        }
        #endregion
    }
}
