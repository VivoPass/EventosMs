using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Api.Controllers;
using EventsService.Api.DTOs;
using EventsService.Aplicacion.Commands.CrearEvento;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EventsService.Test.Api.Controllers.EventsController
{
    public class EventsController_CreateEvent_Tests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILog> _mockLogger;
        private readonly EventsService.Api.Controllers.EventsController _controller;

        private readonly Guid _organizadorId = Guid.NewGuid();
        private readonly Guid _newEventId = Guid.NewGuid();

        public EventsController_CreateEvent_Tests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILog>();

            // HttpClient simulado con BaseAddress
            var httpClientOk = CreateHttpClient(HttpStatusCode.OK);
            _mockHttpClientFactory
                .Setup(c => c.CreateClient("UsuariosClient"))
                .Returns(httpClientOk);

            _controller = new EventsService.Api.Controllers.EventsController(
                _mockMediator.Object,
                _mockHttpClientFactory.Object,
                _mockLogger.Object
            );
        }

        // ---------------------------------------------------------
        // 🔥 ESTE ES EL MÉTODO QUE TENÍAS QUE MODIFICAR
        // ---------------------------------------------------------
        private HttpClient CreateHttpClient(HttpStatusCode statusCode)
        {
            var handler = new FakeHttpMessageHandler(
                new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(string.Empty)
                });

            return new HttpClient(handler, disposeHandler: true)
            {
                BaseAddress = new Uri("http://localhost")   // <<--- FIX IMPORTANTE
            };
        }
        // ---------------------------------------------------------

        // Handler falso para simular respuestas HTTP
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public FakeHttpMessageHandler(HttpResponseMessage response)
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

        // ---------------------------------------------------------
        // TEST 1
        // ---------------------------------------------------------
        [Fact]
        public async Task CreateEvent_ActivityPublishSuccess_ShouldReturn201Created()
        {
            var req = new CreateEventRequest(
                "Concierto prueba",
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(2),
                500,
                "Concierto",
                "Teatro Principal",
                "Un concierto de prueba",
                _organizadorId, 
                "https://www.youtube.com/?gl=ES&hl=es"
            );

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_newEventId);

            var result = await _controller.CreateEvent(req, CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);
            Assert.Equal("GetEventById", created.ActionName);
            Assert.Equal(_newEventId, created.RouteValues["id"]);
        }

        // ---------------------------------------------------------
        // TEST 2
        // ---------------------------------------------------------
        [Fact]
        public async Task CreateEvent_ActivityPublishFails_ShouldStillReturn201Created()
        {
            // HttpClient con error simulado
            var httpClientFails = CreateHttpClient(HttpStatusCode.InternalServerError);

            _mockHttpClientFactory
                .Setup(c => c.CreateClient("UsuariosClient"))
                .Returns(httpClientFails);

            var req = new CreateEventRequest(
                "Evento con falla en publish",
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(1),
                100,
                "Webinar",
                "Online",
                "Test de falla en actividad",
                _organizadorId,
                "https://www.youtube.com/?gl=ES&hl=es"
            );

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_newEventId);

            var result = await _controller.CreateEvent(req, CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);

            _mockLogger.Verify(
                l => l.Warn(It.IsAny<object>()),
                Times.Once
            );
        }
    }
}
