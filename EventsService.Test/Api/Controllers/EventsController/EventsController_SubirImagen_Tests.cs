

using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.Evento;
using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers.EventsController
{
    public class EventsController_SubirImagen_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IHttpClientFactory> MockHttpFactory;
        private readonly Mock<ILog> MockLogger;
        private readonly EventsService.Api.Controllers.EventsController Controller;

        // --- DATOS ---
        private readonly Guid EventoId = Guid.NewGuid();
        private readonly string ImagenUrlResult = "https://cdn.test.com/eventos/img123.jpg";

        public EventsController_SubirImagen_Tests()
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
        #region SubirImagen_ArchivoNull_Retorna400BadRequest
        [Fact]
        public async Task SubirImagen_ArchivoNull_Retorna400BadRequest()
        {
            // ARRANGE
            IFormFile? file = null;

            // ACT
            var result = await Controller.SubirImagen(EventoId, file!, CancellationToken.None);

            // ASSERT
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
            Assert.Equal("El archivo es inválido o está vacío.", badRequest.Value);

            MockLogger.Verify(l => l.Warn(It.IsAny<object>()), Times.Once);
            MockMediator.Verify(m => m.Send(It.IsAny<SubirImagenEventoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        // ---------------------------------------------------------------------
        #region SubirImagen_ArchivoVacio_Retorna400BadRequest
        [Fact]
        public async Task SubirImagen_ArchivoVacio_Retorna400BadRequest()
        {
            // ARRANGE
            var emptyStream = new MemoryStream();
            IFormFile file = new FormFile(emptyStream, 0, 0, "file", "vacio.jpg");

            // ACT
            var result = await Controller.SubirImagen(EventoId, file, CancellationToken.None);

            // ASSERT
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
            Assert.Equal("El archivo es inválido o está vacío.", badRequest.Value);

            MockLogger.Verify(l => l.Warn(It.IsAny<object>()), Times.Once);
            MockMediator.Verify(m => m.Send(It.IsAny<SubirImagenEventoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        // ---------------------------------------------------------------------
        #region SubirImagen_Valido_Retorna200OkYUrl
        [Fact]
        public async Task SubirImagen_Valido_Retorna200OkYUrl()
        {
            // ARRANGE
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            var stream = new MemoryStream(bytes);
            IFormFile file = new FormFile(stream, 0, bytes.Length, "file", "imagen.jpg");

            MockMediator
                .Setup(m => m.Send(It.IsAny<SubirImagenEventoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImagenUrlResult);

            // ACT
            var result = await Controller.SubirImagen(EventoId, file, CancellationToken.None);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            // Leemos las props anónimas EventoId e ImagenUrl
            var value = okResult.Value!;
            var type = value.GetType();

            var eventoIdProp = type.GetProperty("EventoId");
            var imagenUrlProp = type.GetProperty("ImagenUrl");

            Assert.NotNull(eventoIdProp);
            Assert.NotNull(imagenUrlProp);

            var eventoIdValue = (Guid)eventoIdProp!.GetValue(value)!;
            var imagenUrlValue = (string)imagenUrlProp!.GetValue(value)!;

            Assert.Equal(EventoId, eventoIdValue);
            Assert.Equal(ImagenUrlResult, imagenUrlValue);

            // Verificamos que se envió el comando correcto al Mediator
            MockMediator.Verify(m => m.Send(
                    It.Is<SubirImagenEventoCommand>(c =>
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
