using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Controllers;
using EventsService.Api.Contracs.Escenario;
using EventsService.Api.Controllers;
using EventsService.Aplicacion.Commands.CrearEscenario;
using EventsService.Aplicacion.Commands.CrearEvento;
using EventsService.Aplicacion.Commands.EliminarEscenario;
using EventsService.Aplicacion.Commands.ModificarEscenario;
using EventsService.Aplicacion.DTOs.Escenario;
using EventsService.Aplicacion.Queries.ObtenerEscenario;
using EventsService.Dominio.Excepciones;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Moq;

namespace EventsService.Test.Api.Controllers
{
    public class EscenarioControllerTest
    {

        private readonly EscenariosController _escenarioMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Guid _escenarioGuid;
        private readonly string _escenarioId;
        private readonly EscenarioDto _escenarioFake;
        private readonly EscenarioCreateRequest _crearEscenarioFake;
        private readonly EscenarioUpdateRequest _ModificarEscenario;


        public EscenarioControllerTest()
        {
            _escenarioGuid = Guid.NewGuid();
            _escenarioId = _escenarioGuid.ToString();
            _escenarioFake = new EscenarioDto(
                _escenarioGuid,
                "Concha Acústica",
                "Escenario al aire libre",
                "Av. Principal",
                "Caracas",
                "Distrito Capital",
                "Venezuela",
                4500,
                true
            );

            _crearEscenarioFake = new EscenarioCreateRequest(
                 "Concha Acústica",
                "Escenario al aire libre",
                "Av. Principal",
                "Caracas",
                "Distrito Capital",
                "Venezuela"
            );

            _ModificarEscenario = new EscenarioUpdateRequest(
                "Concha Acústica",
                "Escenario al aire libre",
                "Av. Principal",
                "Caracas",
                "Distrito Capital",
                "Venezuela"
            );

            _mediatorMock = new Mock<IMediator>();
            _escenarioMock = new EscenariosController(_mediatorMock.Object);
        }


        [Fact]
        public async Task ObtenerEscenario_ReturnEscenarioResponse()
        {
            _mediatorMock
                .Setup(m => m.Send(It.Is<ObtenerEscenarioQuery>(x => x.Id == _escenarioGuid.ToString()), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_escenarioFake);


            var result = await _escenarioMock.GetById(_escenarioGuid.ToString(), CancellationToken.None);
            // Assert - primero es el OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<EscenarioResponse>(okResult.Value);

            // Comprobar valores esperados
            Assert.Equal(_escenarioFake.Id, response.Id);
            Assert.Equal(_escenarioFake.Nombre, response.Nombre);


        }

        [Fact]
        public async Task ObtenerEscenario_ReturnNotFound()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<ObtenerEscenarioQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EscenarioDto?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _escenarioMock.GetById(_escenarioGuid.ToString(), CancellationToken.None));
        }

        [Fact]

        public async Task CrearEscenario_ReturnOK()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateEscenarioCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_escenarioGuid.ToString());

            var result = await _escenarioMock.Create(_crearEscenarioFake, CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);

            Assert.Equal("GetById", created.ActionName);
            Assert.Equal(_escenarioGuid.ToString(), created.RouteValues!["id"]);


        }

        [Fact]

        public async Task ModificarEscenario_ReturnOk()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<ModificarEscenarioCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            var result = await _escenarioMock.Update(_escenarioId, _ModificarEscenario, CancellationToken.None);

            _mediatorMock.Verify(m => m.Send(It.Is<ModificarEscenarioCommand>(c => c.Id == _escenarioId), CancellationToken.None), Times.Once);
        }


        [Fact]
        public async Task EliminarEscenario_ReturnOk()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<EliminarEscenarioCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            var result = await _escenarioMock.Delete(_escenarioId, CancellationToken.None);

            _mediatorMock.Verify(
                m => m.Send(It.Is<EliminarEscenarioCommand>(c => c.Id == _escenarioId), It.IsAny<CancellationToken>()),
                Times.Once);



        }
        

    }
}
