using EventsService.Api.Controllers;
using EventsService.Api.DTOs;
using EventsService.Aplicacion.Commands.Evento;
using EventsService.Aplicacion.Commands.ModificarEvento;
using EventsService.Aplicacion.Queries.ObtenerEvento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace EventsService.Test.Api.Controllers.EventsController
{
    public class EventsController_Update_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IHttpClientFactory> MockHttpFactory;
        private readonly Mock<ILog> MockLogger;

        private readonly EventsService.Api.Controllers.EventsController Controller;

        // --- DATOS ---
        private readonly Guid OrganizadorId = Guid.NewGuid();
        private readonly Guid EventoId = Guid.NewGuid();

        private readonly UpdateEventRequest ValidUpdateRequest;

        public EventsController_Update_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockHttpFactory = new Mock<IHttpClientFactory>();
            MockLogger = new Mock<ILog>();

            // HttpClient exitoso
            var httpClientOk = CreateHttpClient(HttpStatusCode.OK);
            MockHttpFactory.Setup(c => c.CreateClient("UsuariosClient")).Returns(httpClientOk);

            Controller = new EventsService.Api.Controllers.EventsController(
                MockMediator.Object,
                MockHttpFactory.Object,
                MockLogger.Object
            );

            ValidUpdateRequest = new UpdateEventRequest(
                "Evento actualizado",
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddDays(1),
                DateTimeOffset.UtcNow.AddDays(1).AddHours(2),
                200,
                "Concierto",
                "Teatro",
                "Descripcion prueba",
                "https://www.youtube.com/?gl=ES&hl=es"
            );
        }

        // -----------------------------------------------------------------------------
        // Helper para HttpClient
        // -----------------------------------------------------------------------------
        private HttpClient CreateHttpClient(HttpStatusCode code)
        {
            var handler = new FakeHandler(new HttpResponseMessage(code)
            {
                Content = new StringContent("")
            });

            return new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };
        }

        private class FakeHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            public FakeHandler(HttpResponseMessage response) => _response = response;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
                => Task.FromResult(_response);
        }

        // -----------------------------------------------------------------------------
        #region Update_Exitoso_Retorna204NoContent
        [Fact]
        public async Task Update_Exitoso_Retorna204NoContent()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Evento
                {
                    Id = EventoId,
                    OrganizadorId = OrganizadorId,
                    Nombre = "Evento actualizado"
                });

            // ACT
            var result = await Controller.Update(EventoId, ValidUpdateRequest, CancellationToken.None);

            // ASSERT
            Assert.IsType<NoContentResult>(result);
        }
        #endregion

        // -----------------------------------------------------------------------------
        #region Update_Exitoso_Pero_NoExisteDespues_Retorna204YWarn
        [Fact]
        public async Task Update_Exitoso_Pero_NoExisteDespues_Retorna204YWarn()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Evento?)null);

            // ACT
            var result = await Controller.Update(EventoId, ValidUpdateRequest, CancellationToken.None);

            // ASSERT
            Assert.IsType<NoContentResult>(result);

            MockLogger.Verify(l => l.Warn(It.IsAny<object>()), Times.Once);
        }
        #endregion

        // -----------------------------------------------------------------------------
        #region Update_Exitoso_PeroPublicacionFalla_RegistraWarn
        [Fact]
        public async Task Update_Exitoso_PeroPublicacionFalla_RegistraWarn()
        {
            // ARRANGE
            var httpClientFail = CreateHttpClient(HttpStatusCode.BadRequest);
            MockHttpFactory.Setup(c => c.CreateClient("UsuariosClient")).Returns(httpClientFail);

            MockMediator
                .Setup(m => m.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Evento
                {
                    Id = EventoId,
                    OrganizadorId = OrganizadorId
                });

            // ACT
            var result = await Controller.Update(EventoId, ValidUpdateRequest, CancellationToken.None);

            // ASSERT
            Assert.IsType<NoContentResult>(result);

            MockLogger.Verify(l => l.Warn(It.IsAny<object>()), Times.Once);
        }
        #endregion

        // -----------------------------------------------------------------------------
        #region Update_NoExisteEvento_LanzaNotFoundException
        [Fact]
        public async Task Update_NoExisteEvento_LanzaNotFoundException()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT + ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() =>
                Controller.Update(EventoId, ValidUpdateRequest, CancellationToken.None));

            MockMediator.Verify(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()), Times.Never);
            MockHttpFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        }
        #endregion

        // -----------------------------------------------------------------------------
        #region Update_MediatorLanzaExcepcion_Retorna500
        [Fact]
        public async Task Update_MediatorLanzaExcepcion_Retorna500()
        {
            // ARRANGE
            var exception = new Exception("Error inesperado");

            MockMediator
                .Setup(m => m.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // ACT
            var result = await Assert.ThrowsAsync<Exception>(() =>
                Controller.Update(EventoId, ValidUpdateRequest, CancellationToken.None));
        }
        #endregion

        // -----------------------------------------------------------------------------
        #region Update_Exitoso_VerificaLlamadoHttpClient
        [Fact]
        public async Task Update_Exitoso_VerificaLlamadoHttpClient()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Evento
                {
                    Id = EventoId,
                    OrganizadorId = OrganizadorId
                });

            // ACT
            await Controller.Update(EventoId, ValidUpdateRequest, CancellationToken.None);

            // ASSERT
            MockHttpFactory.Verify(h => h.CreateClient("UsuariosClient"), Times.Once);
        }
        #endregion
    }
}
