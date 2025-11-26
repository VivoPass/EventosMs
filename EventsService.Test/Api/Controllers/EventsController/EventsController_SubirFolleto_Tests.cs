
using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.Evento;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.EventsController
{
    public class EventsController_SubirFolleto_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IHttpClientFactory> MockHttpFactory;
        private readonly Mock<ILog> MockLogger;
        private readonly EventsService.Api.Controllers.EventsController Controller;

        // --- DATOS ---
        private readonly Guid EventoId = Guid.NewGuid();
        private readonly string FolletoUrlResult = "https://cdn.test.com/eventos/folleto123.pdf";

        public EventsController_SubirFolleto_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockHttpFactory = new Mock<IHttpClientFactory>();
            MockLogger = new Mock<ILog>();

            // HttpClientFactory no se usa en este endpoint, pero se inyecta para respetar el ctor
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
        #region SubirFolleto_ArchivoNull_Retorna400BadRequest
        [Fact]
        public async Task SubirFolleto_ArchivoNull_Retorna400BadRequest()
        {
            // ARRANGE
            IFormFile? file = null;

            // ACT
            var result = await Controller.SubirFolleto(EventoId, file!, CancellationToken.None);

            // ASSERT
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
            Assert.Equal("El archivo es inválido o está vacío.", badRequest.Value);

            MockLogger.Verify(l => l.Warn(It.IsAny<object>()), Times.Once);
            MockMediator.Verify(m => m.Send(It.IsAny<SubirFolletoEventoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        // ---------------------------------------------------------------------
        #region SubirFolleto_ArchivoVacio_Retorna400BadRequest
        [Fact]
        public async Task SubirFolleto_ArchivoVacio_Retorna400BadRequest()
        {
            // ARRANGE
            var emptyStream = new MemoryStream();
            IFormFile file = new FormFile(emptyStream, 0, 0, "file", "folleto_vacio.pdf");

            // ACT
            var result = await Controller.SubirFolleto(EventoId, file, CancellationToken.None);

            // ASSERT
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
            Assert.Equal("El archivo es inválido o está vacío.", badRequest.Value);

            MockLogger.Verify(l => l.Warn(It.IsAny<object>()), Times.Once);
            MockMediator.Verify(m => m.Send(It.IsAny<SubirFolletoEventoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        // ---------------------------------------------------------------------
        #region SubirFolleto_Valido_Retorna200OkYUrl
        [Fact]
        public async Task SubirFolleto_Valido_Retorna200OkYUrl()
        {
            // ARRANGE
            var bytes = new byte[] { 10, 20, 30, 40, 50 };
            var stream = new MemoryStream(bytes);
            IFormFile file = new FormFile(stream, 0, bytes.Length, "file", "programa_evento.pdf");

            MockMediator
                .Setup(m => m.Send(It.IsAny<SubirFolletoEventoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FolletoUrlResult);

            // ACT
            var result = await Controller.SubirFolleto(EventoId, file, CancellationToken.None);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var value = okResult.Value!;
            var type = value.GetType();

            var eventoIdProp = type.GetProperty("EventoId");
            var folletoUrlProp = type.GetProperty("FolletoUrl");

            Assert.NotNull(eventoIdProp);
            Assert.NotNull(folletoUrlProp);

            var eventoIdValue = (Guid)eventoIdProp!.GetValue(value)!;
            var folletoUrlValue = (string)folletoUrlProp!.GetValue(value)!;

            Assert.Equal(EventoId, eventoIdValue);
            Assert.Equal(FolletoUrlResult, folletoUrlValue);

            MockMediator.Verify(m => m.Send(
                    It.Is<SubirFolletoEventoCommand>(c =>
                        c.EventoId == EventoId &&
                        c.FileName == file.FileName &&
                        c.FileStream != null),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            MockLogger.Verify(l => l.Info(It.IsAny<object>()), Times.AtLeastOnce);
        }
        #endregion
    }
}
