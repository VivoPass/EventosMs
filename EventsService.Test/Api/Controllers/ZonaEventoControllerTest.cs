//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using EventsService.Api.Controllers;
//using EventsService.Aplicacion.Commands.Zonas.CrearZonaEvento;
//using EventsService.Aplicacion.Commands.Zonas.EliminarZonaEvento;
//using EventsService.Aplicacion.Commands.Zonas.ModificarZonaEvento;
//using EventsService.Aplicacion.DTOs.Asiento;
//using EventsService.Aplicacion.DTOs.Zonas;
//using EventsService.Aplicacion.Queries.Zona.ListarZonasEvento;
//using EventsService.Aplicacion.Queries.Zona.ObtenerZonaEvento;
//using EventsService.Dominio.Entidades;
//using EventsService.Dominio.Excepciones;
//using EventsService.Dominio.ValueObjects;
//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using Moq;

//namespace EventsService.Test.Api.Controllers
//{
//    public class ZonaEventoControllerTest
//    {

//        private readonly Mock<IMediator> _mediator;
//        private readonly ZonasEventoController _controller;
//        private readonly Guid _ZonaId;
//        private readonly Guid _EventoId;
//        private readonly Guid _ZonaId2;
//        private readonly Guid _EventoId2;
//        private readonly ZonaEventoDto _ZonaFake;
//        private readonly ZonaEventoDto _ZonaFake2;
//        private readonly CreateZonaEventoCommand _createZona;
//        private readonly ModificarZonaEventoCommnand _fakeModificarZona;

//        public ZonaEventoControllerTest()
//        {
//            _mediator = new Mock<IMediator>();
//            _controller = new ZonasEventoController(_mediator.Object);
//            _ZonaId = Guid.NewGuid();
//            _EventoId = Guid.NewGuid();
//            _ZonaFake  = new ZonaEventoDto
//            {
//                Id = _ZonaId,
//                EventId = _EventoId,
//                EscenarioId = Guid.NewGuid(),
//                Nombre = "Zona VIP",
//                Tipo = "Sentado",
//                Capacidad = 150,
//                Precio = 120.50m,
//                Estado = "Activo",
//                CreatedAt = DateTime.UtcNow,
//                UpdatedAt = DateTime.UtcNow
//            };

//            _ZonaFake2 = new ZonaEventoDto
//            {
//                Id = _ZonaId2,
//                EventId = _EventoId,
//                EscenarioId = Guid.NewGuid(),
//                Nombre = "Zona VIP",
//                Tipo = "Sentado",
//                Capacidad = 150,
//                Precio = 120.50m,
//                Estado = "Activo",
//                CreatedAt = DateTime.UtcNow,
//                UpdatedAt = DateTime.UtcNow
//            };

//            _createZona = new CreateZonaEventoCommand
//            {
//                EventId = Guid.NewGuid(),
//                EscenarioId = Guid.NewGuid(),
//                Nombre = "Zona VIP",
//                Tipo = "sentado",
//                Capacidad = 120,
//                Precio = 150.00m,
//                Estado = "activa",
//                AutogenerarAsientos = true,
//            };

//            _fakeModificarZona = new ModificarZonaEventoCommnand
//            {
//                EventId = _EventoId,
//                ZonaId = _ZonaId,
//                Nombre = "Zona Actualizada",
//                Precio = 200.00m,
//                Estado = "Prueba xd",
//            };

//        }


//        [Fact]
//        public async Task ObtenerZona_RetornaOk()
//        {
//            _mediator.Setup(m =>
//                m.Send(It.Is<ObtenerZonaEventoQuery>(q => q.EventId == _EventoId && q.ZonaId == _ZonaId),
//                    It.IsAny<CancellationToken>())).ReturnsAsync(_ZonaFake);

//            var result = await _controller.ObtenerZona(_EventoId, _ZonaId, false, CancellationToken.None);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var response = Assert.IsType<ZonaEventoDto>(okResult.Value);

//            Assert.Equal(_ZonaFake.Id, response.Id);
//            Assert.Equal(_ZonaFake.Nombre, response.Nombre);


//        }

//        [Fact]
//        public async Task ObtenerZona_RetornaNotFound()
//        {
//            _mediator.Setup(m => m.Send(It.IsAny<ObtenerZonaEventoQuery>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync((ZonaEventoDto?)null);

//            Assert.ThrowsAsync<NotFoundException>(() =>
//                _controller.ObtenerZona(_EventoId, _ZonaId, false, CancellationToken.None));
//        }


//        [Fact]
//        public async Task CrearZona_returnOk()
//        {
//            _mediator.Setup(m => m.Send(It.IsAny<CreateZonaEventoCommand>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_ZonaId);

//            var result = await _controller.CrearZona(_EventoId, _createZona, CancellationToken.None);

//            _mediator.Verify(m => m.Send(It.Is<CreateZonaEventoCommand>(c =>
//                    c.EventId == _EventoId &&
//                    c.Nombre == _createZona.Nombre // verifica campos que te interesen
//            ), It.IsAny<CancellationToken>()), Times.Once);

//        }

//        [Fact]
//        public async Task ModificarZona_ReturnOk()
//        {
//            _mediator.Setup(m => m.Send(It.IsAny<ModificarZonaEventoCommnand>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            var result =
//                await _controller.ModificarZona(_EventoId, _ZonaId, _fakeModificarZona, CancellationToken.None);

//            _mediator.Verify(m => m.Send(It.Is<ModificarZonaEventoCommnand>(c => c.Nombre == _fakeModificarZona.Nombre), It.IsAny<CancellationToken>()), Times.Once);

//        }

//        [Fact]
//        public async Task ModificarZona_ReturnNotFoundException()
//        {
//            _mediator.Setup(m => m.Send(It.IsAny<ModificarZonaEventoCommnand>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(false);

//            Assert.ThrowsAsync<NotFoundException>(() =>
//                _controller.ModificarZona(_EventoId, _ZonaId, _fakeModificarZona, CancellationToken.None));
//        }

//        [Fact]
//        public async Task EliminarZona_ReturnsOk()
//        {
//            _mediator.Setup(m => m.Send(It.IsAny<EliminarZonaEventoCommand>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            var result =
//                await _controller.EliminarZona(_EventoId, _ZonaId, CancellationToken.None);

//            _mediator.Verify(m => m.Send(It.Is<EliminarZonaEventoCommand>(c => c.EventId == _EventoId), It.IsAny<CancellationToken>()), Times.Once);
//        }

//        [Fact]
//        public async Task EliminarZona_ReturnNotFoundException()
//        {
//            _mediator.Setup(m => m.Send(It.IsAny<EliminarZonaEventoCommand>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(false);

//            Assert.ThrowsAsync<NotFoundException>(() =>
//                _controller.EliminarZona(_EventoId, _ZonaId, CancellationToken.None));
//        }

//        [Fact]
//        public async Task ObtenerZonasEventos_ReturnList()
//        {
//            List<ZonaEventoDto> zonas = new List<ZonaEventoDto>{_ZonaFake, _ZonaFake2};

//            _mediator.Setup(m => m.Send(It.Is<ListarZonasEventoQuery>(q => q.EventId == _EventoId), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(zonas);


//            var result = await _controller.ListarZonas(_EventoId, "", "", "", false);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var ls = Assert.IsType<List<ZonaEventoDto>>(okResult.Value);


//            Assert.Equal(2 , ls.Count);
//            Assert.Equal(zonas, ls);
//        }
//    }
//}
