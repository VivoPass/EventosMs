using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Aplicacion.Queries.Asiento.ObtenerAsiento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Interfaces;
using Moq;
using Xunit;

namespace EventsService.Test.Aplication.Queries.Asiento
{
    public class ObtenerAsientoPorIdHandlerTests
    {
        private readonly Mock<IAsientoRepository> _asientosMock;
        private readonly Mock<IZonaEventoRepository> _zonasMock;
        private readonly ObtenerAsientoPorIdHandler _handler;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();
        private readonly Guid _asientoId = Guid.NewGuid();

        public ObtenerAsientoPorIdHandlerTests()
        {
            _asientosMock = new Mock<IAsientoRepository>();
            _zonasMock = new Mock<IZonaEventoRepository>();

            _handler = new ObtenerAsientoPorIdHandler(_asientosMock.Object, _zonasMock.Object);
        }

        [Fact]
        public async Task Handle_ThrowsNotFound_WhenZonaNotFound()
        {
            // Arrange
            _zonasMock.Setup(z => z.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((ZonaEvento?)null);

            var query = new ObtenerAsientoQuery(_eventId, _zonaId, _asientoId);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }


        [Fact]
        public async Task Handle_ThrowsNotFound_WhenAsientoNotFound()
        {
            _zonasMock.Setup(z => z.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ZonaEvento { Id = _zonaId, EventId = _eventId });

            _asientosMock.Setup(a => a.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Dominio.Entidades.Asiento?)null);

            var query = new ObtenerAsientoQuery(_eventId, _zonaId, _asientoId);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ReturnsMappedDto_WhenExistsAndBelongs()
        {
            _zonasMock.Setup(z => z.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ZonaEvento { Id = _zonaId, EventId = _eventId });

            _asientosMock.Setup(a => a.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Dominio.Entidades.Asiento
                         {
                             Id = _asientoId,
                             EventId = _eventId,
                             ZonaEventoId = _zonaId,
                             Label = "A1",
                             Estado = "disponible",
                             FilaIndex = 3,
                             ColIndex = 5
                         });

            var query = new ObtenerAsientoQuery(_eventId, _zonaId, _asientoId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(_asientoId, result!.Id);
            Assert.Equal("A1", result.Label);
            Assert.Equal("disponible", result.Estado);
            Assert.Equal(3, result.FilaIndex);
            Assert.Equal(5, result.ColIndex);
        }
    }
}
