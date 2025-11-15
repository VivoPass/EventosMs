using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.CrearEscenario;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.Entidades;
using Moq;

namespace EventsService.Test.Aplication.Commands.Escenario
{
    public  class CreateEscenarioHandlerTest
    {
        private readonly Mock<IScenarioRepository> _repo;
        private readonly CreateEscenarioCommand _command;
        private readonly CreateEscenarioHandler _handler;
        private readonly Guid _id;


        public CreateEscenarioHandlerTest()
        {
            _repo = new Mock<IScenarioRepository>();
            _command = new CreateEscenarioCommand("Escenario prueba", "prueba", "prueba", "prueba", "prueba", "prueba");
            _handler = new CreateEscenarioHandler(_repo.Object);
            _id = Guid.NewGuid();
        }

        [Fact]
        public async Task RegistrarEscenario_Exitoso()
        {
            _repo.Setup(r => r.CrearAsync(It.IsAny<Dominio.Entidades.Escenario>(), It.IsAny<CancellationToken>())).ReturnsAsync(_id.ToString);


            _handler.Handle(_command, CancellationToken.None);

            _repo.Verify(r => r.CrearAsync(It.IsAny<Dominio.Entidades.Escenario>(), It.IsAny<CancellationToken>()),Times.Once);
        }

    }
}
