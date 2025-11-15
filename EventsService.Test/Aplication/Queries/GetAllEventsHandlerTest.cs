using EventsService.Aplicacion.Queries.ObtenerTodosEventos;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Test.Aplication.Queries
{
    public class GetAllEventsHandlerTest
    {
        private readonly Mock<IEventRepository> _repoMock;
        private readonly GetAllEventsHandler _handler;
        private readonly Evento _event1;
        private readonly Evento _event2;
        private readonly List<Evento> _eventos;

        public GetAllEventsHandlerTest()
        {
            _repoMock = new Mock<IEventRepository>();
            _handler = new GetAllEventsHandler(_repoMock.Object);

            _event1 = new Evento { Id = Guid.NewGuid(), Nombre = "Evento A" };
            _event2 = new Evento { Id = Guid.NewGuid(), Nombre = "Evento B" };
            _eventos = new List<Evento> { _event1, _event2 };
        }


        [Fact]
        public async Task Handle_ReturnsAllEvents()
        {
            // Arrange
            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_eventos);

            // Act
            var result = await _handler.Handle(new GetAllEventsQuery(), CancellationToken.None);

            // Assert: resultado no nulo y misma cantidad
            Assert.NotNull(result);
            Assert.Equal(_eventos.Count, result.Count);

            // Assert: verificar que los elementos esperados estén presentes (comparamos por Id)
            Assert.Contains(result, e => e.Id == _event1.Id);
            Assert.Contains(result, e => e.Id == _event2.Id);
        }
    }
}
