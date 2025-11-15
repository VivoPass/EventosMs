using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.EliminarEvento;
using EventsService.Dominio.Interfaces;
using Moq;

namespace EventsService.Test.Aplication.Commands.Evento
{
    public  class DeleteEventHandlerTest
    {

        private readonly Mock<IEventRepository> _repo;
        private readonly DeleteEventCommand _commnand;
        private readonly DeleteEventHandler _handler;
        private readonly Guid _eventoId;

        public DeleteEventHandlerTest()
        {
            _repo = new Mock<IEventRepository>();
            _eventoId = Guid.NewGuid();
            _commnand = new DeleteEventCommand(_eventoId);
            _handler = new DeleteEventHandler(_repo.Object);

        }

        [Fact]
        public async Task EliminarEvento_valid_returnTrue()
        {
            _repo.Setup(r => r.DeleteAsync(_commnand.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _handler.Handle(_commnand, CancellationToken.None);

            Assert.True(result);
        }

    }
}
