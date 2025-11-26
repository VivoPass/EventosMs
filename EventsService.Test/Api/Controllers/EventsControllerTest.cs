//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;
//using EventsService.Api.Controllers;
//using EventsService.Api.DTOs;
//using EventsService.Aplicacion.Commands.CrearEvento;
//using EventsService.Aplicacion.Queries.ObtenerEvento;
//using MediatR;
//using EventsService.Dominio.Entidades;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Http;
//using EventsService.Dominio.Excepciones;
//using EventsService.Aplicacion.Commands.ModificarEvento;
//using EventsService.Aplicacion.Commands.EliminarEvento;
//using EventsService.Aplicacion.Commands.Evento;
//using EventsService.Aplicacion.Queries.ObtenerTodosEventos;

//namespace EventsService.Test.Api.Controllers
//{
//    public class EventsControllerTest
//    {
//        [Fact]
//        public async Task ObtenerEvento_Retorna_ok()
//        {
//            // Arrange
//            var mockMediator = new Mock<IMediator>();
//            var eventId = Guid.NewGuid();
//            var fakeEvent = new Evento
//            {
//                Id = eventId,
//                Nombre = "Concierto"
               
//            };

//            mockMediator.Setup(m => m.Send(It.Is<GetEventByIdQuery>(e => e.Id == eventId), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(fakeEvent);

//            var controller = new EventsController(mockMediator.Object);

//            // Act 
//            var result = await controller.GetEventById(eventId, CancellationToken.None);



//            // Assert

//            var ok = Assert.IsType<OkObjectResult>(result);
//            Assert.Equal(200, ok.StatusCode);
//            Assert.Equal(fakeEvent, ok.Value);

//            mockMediator.Verify(m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
//        }




//        [Fact]
//        public async Task ObtenerEvento_Lanza_NotFoundException_SiNoExiste()
//        {
//            // Arrange
//            var mockMediator = new Mock<IMediator>();
//            var eventId = Guid.NewGuid();

//            // Configuramos el mock para devolver null (evento no encontrado)
//            mockMediator
//                .Setup(m => m.Send(It.Is<GetEventByIdQuery>(e => e.Id == eventId), It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Evento?)null);

//            var controller = new EventsController(mockMediator.Object);
//            var actionResult = await controller.GetEventById(eventId, CancellationToken.None);

//            // Act & Assert
//            var notFound = Assert.IsType<NotFoundResult>(actionResult);

//            mockMediator.Verify(
//                m => m.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()),
//                Times.Once
//            );
//        }


//        [Fact]

//        public async Task CreateEvent_devuelve_201_con_location()
//        {
//            var mock = new Mock<IMediator>();
//            var controller = new EventsController(mock.Object);
//            var nuevoId = Guid.NewGuid();

//            mock.Setup(m => m.Send(It.IsAny<CreateEventCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(nuevoId);


//            var req = new EventsService.Api.DTOs.CreateEventRequest(
//                Nombre: "Tech Expo",
//                CategoriaId: Guid.NewGuid(),
//                EscenarioId: Guid.NewGuid(),
//                Inicio: DateTimeOffset.UtcNow,
//                Fin: DateTimeOffset.UtcNow.AddHours(2),
//                AforoMaximo: 200,
//                Tipo: "Feria",
//                Lugar: "Miami",
//                Descripcion: "Desc"

//            );

//            var result = await controller.CreateEvent(req, CancellationToken.None);

//            var created = Assert.IsType<CreatedAtActionResult>(result);
//            Assert.Equal(nameof(EventsController.GetEventById), created.ActionName);
//            Assert.Equal(nuevoId, created.RouteValues!["id"]);

//            mock.Verify(m => m.Send(It.Is<CreateEventCommand>(c => c.Nombre == req.Nombre), It.IsAny<CancellationToken>()), Times.Once);


//        }

//        [Fact]
//        public async Task Update_devuelve_204_NoContent_si_existe()
//        {
//            var mock = new Mock<IMediator>();
//            var controller = new EventsController(mock.Object);
//            var id = Guid.NewGuid();

//            mock.Setup(m => m.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            var req = new UpdateEventRequest(
//                Nombre: "Tech Expo (v2)",
//                CategoriaId: Guid.NewGuid(),
//                EscenarioId: Guid.NewGuid(),
//                Inicio: DateTimeOffset.UtcNow,
//                Fin: DateTimeOffset.UtcNow.AddHours(4),
//                AforoMaximo: 300,
//                Tipo: "Feria",
//                Lugar: "Miami",
//                Descripcion: "Edición actualizada"
//            );

//            var result = await controller.Update(id, req, CancellationToken.None);

//            var noContent = Assert.IsType<NoContentResult>(result);
//            Assert.Equal(204, noContent.StatusCode);

//            mock.Verify(m => m.Send(
//                    It.Is<UpdateEventCommand>(c => c.Id == id && c.Nombre == req.Nombre),
//                    It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task Update_devuelve_NoExiste()
//        {
//            var mock = new Mock<IMediator>();
//            var controller = new EventsController(mock.Object);
//            var id = Guid.NewGuid();

//            mock.Setup(m => m.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(false);

//            var req = new UpdateEventRequest(
//                Nombre: "Tech Expo (v2)",
//                CategoriaId: Guid.NewGuid(),
//                EscenarioId: Guid.NewGuid(),
//                Inicio: DateTimeOffset.UtcNow,
//                Fin: DateTimeOffset.UtcNow.AddHours(4),
//                AforoMaximo: 300,
//                Tipo: "Feria",
//                Lugar: "Miami",
//                Descripcion: "Edición actualizada"
//            );

//            await Assert.ThrowsAsync<NotFoundException>(async () =>
//                await controller.Update(id, req, CancellationToken.None));

//            mock.Verify(
//                m => m.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task Delete_devuelve_204_NoContent_si_existe()
//        {
//            // Arrange
//            var mockMediator = new Mock<IMediator>();
//            var controller = new EventsController(mockMediator.Object);
//            var id = Guid.NewGuid();

//            mockMediator
//                .Setup(m => m.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            // Act
//            var result = await controller.Delete(id, CancellationToken.None);

//            // Assert
//            var noContent = Assert.IsType<NoContentResult>(result);
//            Assert.Equal(204, noContent.StatusCode);

//            mockMediator.Verify(
//                m => m.Send(It.Is<DeleteEventCommand>(c => c.Id == id), It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task Delete_lanza_NotFoundException_SiNoExiste()
//        {
//            // Arrange
//            var mockMediator = new Mock<IMediator>();
//            var controller = new EventsController(mockMediator.Object);
//            var id = Guid.NewGuid();

//            mockMediator
//                .Setup(m => m.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(false);

//            // Act & Assert
//            await Assert.ThrowsAsync<NotFoundException>(async () =>
//                await controller.Delete(id, CancellationToken.None));

//            mockMediator.Verify(
//                m => m.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task GetAll_devuelve_200_Ok_con_lista()
//        {
//            // Arrange
//            var mockMediator = new Mock<IMediator>();
//            var controller = new EventsController(mockMediator.Object);

//            var ev1 = new Evento { Id = Guid.NewGuid(), Nombre = "Concierto" };
//            var ev2 = new Evento { Id = Guid.NewGuid(), Nombre = "Feria" };
//            var fakeList = new List<Evento> { ev1, ev2 };   // <-- LIST<Evento>, no object

//            mockMediator
//                .Setup(m => m.Send(
//                    It.IsAny<GetAllEventsQuery>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(fakeList);

//            // Act
//            var result = await controller.GetAll(CancellationToken.None); // <-- pasa ct si el método lo pide

//            // Assert
//            var ok = Assert.IsType<OkObjectResult>(result);
//            Assert.Equal(200, ok.StatusCode);

//            // El valor devuelto debe ser IEnumerable<Evento> (o List<Evento>)
//            var value = Assert.IsAssignableFrom<IEnumerable<Evento>>(ok.Value);
//            var arr = value.ToList();

//            Assert.Equal(2, arr.Count);
//            Assert.Contains(arr, e => e.Id == ev1.Id && e.Nombre == ev1.Nombre);
//            Assert.Contains(arr, e => e.Id == ev2.Id && e.Nombre == ev2.Nombre);

//            mockMediator.Verify(
//                m => m.Send(It.IsAny<GetAllEventsQuery>(), It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//          // ---------- Helpers ----------
//        private IFormFile CreateFakeFile(string fileName = "test.jpg", string content = "fake file")
//        {
//            var bytes = Encoding.UTF8.GetBytes(content);
//            var stream = new MemoryStream(bytes);

//            return new FormFile(stream, 0, bytes.Length, "file", fileName)
//            {
//                Headers = new HeaderDictionary(),
//                ContentType = "image/jpeg"
//            };
//        }

//        // ---------- SubirImagen ----------

//        [Fact]
//        public async Task SubirImagen_SinArchivo_RetornaBadRequest()
//        {
//            Mock<IMediator> _mediatorMock = new Mock<IMediator>();
//            EventsController _controller = new EventsController(_mediatorMock.Object);
//            // Arrange
//            var eventoId = Guid.NewGuid();

//            // Act
//            var result = await _controller.SubirImagen(eventoId, file: null!, CancellationToken.None);

//            // Assert
//            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
//            Assert.Equal("El archivo es inválido o está vacío.", badRequest.Value);
//            _mediatorMock.Verify(
//                m => m.Send(It.IsAny<SubirImagenEventoCommand>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }

//        [Fact]
//        public async Task SubirImagen_ArchivoValido_RetornaOkYLlamaMediator()
//        {
//            Mock<IMediator> _mediatorMock = new Mock<IMediator>();
//            EventsController _controller = new EventsController(_mediatorMock.Object);
//            // Arrange
//            var eventoId = Guid.NewGuid();
//            var file = CreateFakeFile();
//            var expectedUrl = "https://res.cloudinary.com/demo/image/upload/test.jpg";

//            _mediatorMock
//                .Setup(m => m.Send(
//                    It.IsAny<SubirImagenEventoCommand>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(expectedUrl);

//            // Act
//            var result = await _controller.SubirImagen(eventoId, file, CancellationToken.None);

//            // Assert
//            var ok = Assert.IsType<OkObjectResult>(result);

//            // Leemos las propiedades del objeto anónimo devuelto
//            var value = ok.Value!;
//            var tipo = value.GetType();

//            var eventoIdProp = (Guid)tipo.GetProperty("EventoId")!.GetValue(value)!;
//            var imagenUrlProp = (string)tipo.GetProperty("ImagenUrl")!.GetValue(value)!;

//            Assert.Equal(eventoId, eventoIdProp);
//            Assert.Equal(expectedUrl, imagenUrlProp);

//            _mediatorMock.Verify(
//                m => m.Send(
//                    It.Is<SubirImagenEventoCommand>(c =>
//                        c.EventoId == eventoId &&
//                        c.FileName == file.FileName),
//                    It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task SubirFolleto_SinArchivo_RetornaBadRequest()
//        {
//            Mock<IMediator> _mediatorMock = new Mock<IMediator>();
//            EventsController _controller = new EventsController(_mediatorMock.Object);
//            // Arrange
//            var eventoId = Guid.NewGuid();

//            // Act
//            var result = await _controller.SubirFolleto(eventoId, file: null!, CancellationToken.None);

//            // Assert
//            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
//            Assert.Equal("El archivo es inválido o está vacío.", badRequest.Value);
//            _mediatorMock.Verify(
//                m => m.Send(It.IsAny<SubirFolletoEventoCommand>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }

//        [Fact]
//        public async Task SubirFolleto_ArchivoValido_RetornaOkYLlamaMediator()
//        {
//            Mock<IMediator> _mediatorMock = new Mock<IMediator>();
//            EventsController _controller = new EventsController(_mediatorMock.Object);
//            // Arrange
//            var eventoId = Guid.NewGuid();
//            var file = CreateFakeFile("folleto.pdf");
//            var expectedUrl = "https://res.cloudinary.com/demo/raw/upload/folleto.pdf";

//            _mediatorMock
//                .Setup(m => m.Send(
//                    It.IsAny<SubirFolletoEventoCommand>(),
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(expectedUrl);

//            // Act
//            var result = await _controller.SubirFolleto(eventoId, file, CancellationToken.None);

//            // Assert
//            var ok = Assert.IsType<OkObjectResult>(result);

//            var value = ok.Value!;
//            var tipo = value.GetType();

//            var eventoIdProp = (Guid)tipo.GetProperty("EventoId")!.GetValue(value)!;
//            var folletoUrlProp = (string)tipo.GetProperty("FolletoUrl")!.GetValue(value)!;

//            Assert.Equal(eventoId, eventoIdProp);
//            Assert.Equal(expectedUrl, folletoUrlProp);

//            _mediatorMock.Verify(
//                m => m.Send(
//                    It.Is<SubirFolletoEventoCommand>(c =>
//                        c.EventoId == eventoId &&
//                        c.FileName == file.FileName),
//                    It.IsAny<CancellationToken>()),
//                Times.Once);
//        }
//    }


//}
