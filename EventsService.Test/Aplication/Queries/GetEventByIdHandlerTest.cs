using EventsService.Aplicacion.Queries.ObtenerEvento;
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
    public class GetEventByIdHandlerTest
    {
        private readonly Mock<IEventRepository> _repoMock;
        private readonly GetEventByIdHandler _handler;
        private readonly Guid _eventId;
        private readonly Evento _eventoFake;

        public GetEventByIdHandlerTest()
        {
            _repoMock = new Mock<IEventRepository>();
            _handler = new GetEventByIdHandler(_repoMock.Object);

            _eventId = Guid.NewGuid();
            _eventoFake = new Evento
            {
                Id = _eventId,
                Nombre = "Evento de prueba",
                Descripcion = "Descripción de prueba"
                
            };
        }


        [Fact]
        public async Task Handle_ReturnsEvent_WhenFound()
        {
            // Arrange
            _repoMock
                .Setup(r => r.GetByIdAsync(_eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_eventoFake);

            var query = new GetEventByIdQuery(_eventId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_eventId, result!.Id);
            Assert.Equal(_eventoFake.Nombre, result.Nombre);

            
        }

        [Fact]
        public async Task Handle_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _repoMock
                .Setup(r => r.GetByIdAsync(_eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Evento?)null);

            var query = new GetEventByIdQuery(_eventId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}

