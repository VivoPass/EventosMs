using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.Asiento.ActualizarAsiento;
using EventsService.Aplicacion.Commands.Asiento.CrearAsiento;
using EventsService.Aplicacion.Commands.Asiento.EliminarAsiento;
using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Aplicacion.Queries.Asiento.ListarAsientos;
using EventsService.Aplicacion.Queries.Asiento.ObtenerAsiento;
using EventsService.Dominio.Entidades;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Api.Controllers
{
    public class AsientoControllerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly AsientosController _controller;
        private readonly Guid _AsientoId;
        private readonly Guid _AsientoId1;
        private readonly Guid _ZonaId;
        private readonly Guid _eventoId;
        private readonly CrearAsientoDto _AsientoFake;
        private readonly AsientoDto _asientoFake;
        private readonly AsientoDto _asientoFake1;
        private readonly ActualizarAsientoDto _actualizarAsiento;

        public AsientoControllerTest()
        {
            _mediator = new Mock<IMediator>();
            _controller = new AsientosController(_mediator.Object);

            _eventoId = Guid.NewGuid();
            _ZonaId = Guid.NewGuid();
            _AsientoId = Guid.NewGuid();
            _AsientoId1 = Guid.NewGuid();
            _AsientoFake = new CrearAsientoDto
            {
                FilaIndex = 3,
                ColIndex = 7,
                Label = "A7",
                Estado = "disponible",
                Meta = new Dictionary<string, string>
                {
                    { "Tipo", "Preferencial" },
                    { "Vista", "Frontal" }
                }
            };

            _asientoFake = new AsientoDto()
            {
                Id = _AsientoId1,
                FilaIndex = 3,
                ColIndex = 7,
                Label = "A7",
                Estado = "disponible",
            };

            _asientoFake1 = new AsientoDto()
            {
                Id = _AsientoId,
                FilaIndex = 3,
                ColIndex = 7,
                Label = "A7",
                Estado = "disponible",
            };

            _actualizarAsiento = new ActualizarAsientoDto
            {
                Label = "A1-10",
                Estado = "reservado",
                Meta = new Dictionary<string, string>
                {
                    { "color", "azul" },
                    { "prioridad", "alta" }
                }
            };
        }


        [Fact]
        public async Task CrearAsiento_returnOk()
        {
            _mediator.Setup(m => m.Send(It.IsAny<CrearAsientoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CrearAsientoResult(_AsientoId));

            var result = await _controller.Crear(_eventoId, _ZonaId, _AsientoFake, CancellationToken.None);

            _mediator.Verify(m => m.Send(It.IsAny<CrearAsientoCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task ObtenerAsiento_ReturnOk()
        {
            _mediator.Setup(m => m.Send(It.Is<ObtenerAsientoQuery>(q => q.AsientoId == _AsientoId),
                It.IsAny<CancellationToken>())).ReturnsAsync(_asientoFake);


            var result = await _controller.ObtenerPorId(_eventoId, _ZonaId, _AsientoId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<AsientoDto>(okResult.Value);

            Assert.Equal(_asientoFake.Id, dto.Id);
        }

        [Fact]
        public async Task ModificarAsiento_ReturnOk()
        {
            _mediator.Setup(m => m.Send(It.IsAny<ActualizarAsientoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.Actualizar(_eventoId, _ZonaId, _AsientoId, _actualizarAsiento,
                CancellationToken.None);

            _mediator.Verify(m => m.Send(It.IsAny<ActualizarAsientoCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task EliminarAsiento_returnOK()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EliminarAsientoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);


            var result = await _controller.Eliminar(_eventoId, _ZonaId, _AsientoId, CancellationToken.None);

            _mediator.Verify(m => m.Send(It.IsAny<EliminarAsientoCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task ListarAsientos_ReturnOk()
        {
            List<AsientoDto> asientos = new List<AsientoDto> {_asientoFake, _asientoFake1};

            _mediator.Setup(m => m.Send(It.IsAny<ListarAsientosQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(asientos);

            var result = await _controller.Listar(_eventoId, _ZonaId, CancellationToken.None);
            var OkResult = Assert.IsType<OkObjectResult>(result);
            var content = Assert.IsType<List<AsientoDto>>(OkResult.Value);

            Assert.Equal(asientos, content);

            _mediator.Verify(m=>m.Send(It.IsAny<ListarAsientosQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        
    }
}

