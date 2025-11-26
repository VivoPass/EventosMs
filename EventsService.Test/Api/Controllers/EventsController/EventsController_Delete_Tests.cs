using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.EliminarEvento;
using EventsService.Aplicacion.Commands.Evento;
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
    public class EventsController_Delete_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IHttpClientFactory> MockHttpFactory;
        private readonly Mock<ILog> MockLogger;
        private readonly EventsService.Api.Controllers.EventsController Controller;

        // --- DATOS ---
        private readonly Guid EventoId = Guid.NewGuid();
        private readonly Guid OrganizadorId = Guid.NewGuid();

        public EventsController_Delete_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockHttpFactory = new Mock<IHttpClientFactory>();
            MockLogger = new Mock<ILog>();

            // HttpClient por defecto exitoso
            var httpClientOk = CreateHttpClient(HttpStatusCode.OK);
            MockHttpFactory
                .Setup(f => f.CreateClient("UsuariosClient"))
                .Returns(httpClientOk);

            Controller = new EventsService.Api.Controllers.EventsController(
                MockMediator.Object,
                MockHttpFactory.Object,
                MockLogger.Object
            );
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------
        private HttpClient CreateHttpClient(HttpStatusCode statusCode)
        {
            var handler = new FakeHandler(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(string.Empty)
            });

            return new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };
        }

        private class FakeHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public FakeHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        // ---------------------------------------------------------------------
        #region Delete_Exitoso_Retorna204NoContent
        [Fact]
        public async Task Delete_Exitoso_Retorna204NoContent()
        {
            // ARRANGE
            var existing = new Evento
            {
                Id = EventoId,
                OrganizadorId = OrganizadorId,
                Nombre = "Evento a eliminar"
            };

            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            MockMediator
                .Setup(m => m.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await Controller.Delete(EventoId, CancellationToken.None);

            // ASSERT
            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);

            MockMediator.Verify(m => m.Send(
                    It.Is<GetEventByIdQuery>(q => q.Id == EventoId),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            MockMediator.Verify(m => m.Send(
                    It.Is<DeleteEventCommand>(c => c.Id == EventoId),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            MockHttpFactory.Verify(f => f.CreateClient("UsuariosClient"), Times.Once);
        }
        #endregion

        // ---------------------------------------------------------------------
        #region Delete_EventoNoExisteAntes_LanzaNotFoundException
        [Fact]
        public async Task Delete_EventoNoExisteAntes_LanzaNotFoundException()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Evento?)null);

            // ACT + ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() =>
                Controller.Delete(EventoId, CancellationToken.None));

            MockMediator.Verify(m => m.Send(
                    It.IsAny<DeleteEventCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            MockHttpFactory.Verify(f => f.CreateClient("UsuariosClient"), Times.Never);
        }
        #endregion

        // ---------------------------------------------------------------------
        #region Delete_DeleteCommandDevuelveFalse_LanzaNotFoundException
        [Fact]
        public async Task Delete_DeleteCommandDevuelveFalse_LanzaNotFoundException()
        {
            // ARRANGE
            var existing = new Evento
            {
                Id = EventoId,
                OrganizadorId = OrganizadorId,
                Nombre = "Evento a eliminar"
            };

            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            MockMediator
                .Setup(m => m.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT + ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() =>
                Controller.Delete(EventoId, CancellationToken.None));

            MockHttpFactory.Verify(f => f.CreateClient("UsuariosClient"), Times.Never);
        }
        #endregion

        // ---------------------------------------------------------------------
        #region Delete_PublicacionActividadFalla_RegistraWarnYRetorna204
        [Fact]
        public async Task Delete_PublicacionActividadFalla_RegistraWarnYRetorna204()
        {
            // ARRANGE
            var existing = new Evento
            {
                Id = EventoId,
                OrganizadorId = OrganizadorId,
                Nombre = "Evento a eliminar"
            };

            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            MockMediator
                .Setup(m => m.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // HttpClient que falla al publicar actividad
            var httpClientFail = CreateHttpClient(HttpStatusCode.InternalServerError);
            MockHttpFactory
                .Setup(f => f.CreateClient("UsuariosClient"))
                .Returns(httpClientFail);

            // ACT
            var result = await Controller.Delete(EventoId, CancellationToken.None);

            // ASSERT
            Assert.IsType<NoContentResult>(result);
            MockLogger.Verify(l => l.Warn(It.IsAny<object>()), Times.Once);
        }
        #endregion

        // ---------------------------------------------------------------------
        #region Delete_PublicacionActividadExitosa_RegistraDebug
        [Fact]
        public async Task Delete_PublicacionActividadExitosa_RegistraDebug()
        {
            // ARRANGE
            var existing = new Evento
            {
                Id = EventoId,
                OrganizadorId = OrganizadorId,
                Nombre = "Evento a eliminar"
            };

            MockMediator
                .Setup(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            MockMediator
                .Setup(m => m.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var httpClientOk = CreateHttpClient(HttpStatusCode.OK);
            MockHttpFactory
                .Setup(f => f.CreateClient("UsuariosClient"))
                .Returns(httpClientOk);

            // ACT
            var result = await Controller.Delete(EventoId, CancellationToken.None);

            // ASSERT
            Assert.IsType<NoContentResult>(result);
            MockLogger.Verify(l => l.Debug(It.IsAny<object>()), Times.Once);
        }
        #endregion
    }
}
