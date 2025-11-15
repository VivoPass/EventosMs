using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Asiento;
using EventsService.Aplicacion.Queries.Asiento.ListarAsientos;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using Moq;
using Xunit;

namespace EventsService.Test.Aplication.Queries.Asiento
{
    public class ListarAsientosHandlerTests
    {
        private readonly Mock<IAsientoRepository> _asientosMock;
        private readonly Mock<IZonaEventoRepository> _zonasMock;
        private readonly ListarAsientosHandler _handler;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly Guid _zonaId = Guid.NewGuid();

        private readonly ZonaEvento _zonaExistente;
        private readonly List<Dominio.Entidades.Asiento> _asientos;

        public ListarAsientosHandlerTests()
        {
            _asientosMock = new Mock<IAsientoRepository>();
            _zonasMock = new Mock<IZonaEventoRepository>();

            _handler = new ListarAsientosHandler(_asientosMock.Object, _zonasMock.Object);

            _zonaExistente = new ZonaEvento
            {
                Id = _zonaId,
                EventId = _eventId,
                Nombre = "Zona Test",
                Tipo = "sentado"
            };

            _asientos = new List<Dominio.Entidades.Asiento>
            {
                new Dominio.Entidades.Asiento { Id = Guid.NewGuid(), EventId = _eventId, ZonaEventoId = _zonaId, Label = "A1", Estado = "available", FilaIndex = 1, ColIndex = 1 },
                new Dominio.Entidades.Asiento { Id = Guid.NewGuid(), EventId = _eventId, ZonaEventoId = _zonaId, Label = "A2", Estado = "reserved", FilaIndex = 1, ColIndex = 2 }
            };

            _zonasMock
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_zonaExistente);

            _asientosMock
                .Setup(r => r.ListByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_asientos.AsReadOnly());
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenZonaInvalid()
        {
            _zonasMock
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ZonaEvento?)null);

            var query = new ListarAsientosQuery(_eventId, _zonaId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
            _asientosMock.Verify(r => r.ListByZonaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsMappedAsientos_WhenZonaValid()
        {
            var query = new ListarAsientosQuery(_eventId, _zonaId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var dtoA1 = result.SingleOrDefault(a => a.Label == "A1");
            var dtoA2 = result.SingleOrDefault(a => a.Label == "A2");

            Assert.NotNull(dtoA1);
            Assert.Equal("available", dtoA1!.Estado);
            Assert.Equal(1, dtoA1.FilaIndex);
            Assert.Equal(1, dtoA1.ColIndex);

            Assert.NotNull(dtoA2);
            Assert.Equal("reserved", dtoA2!.Estado);
            Assert.Equal(1, dtoA2.FilaIndex);
            Assert.Equal(2, dtoA2.ColIndex);

            _asientosMock.Verify(r => r.ListByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoAsientos()
        {
            _asientosMock
                .Setup(r => r.ListByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Dominio.Entidades.Asiento>().AsReadOnly());

            var query = new ListarAsientosQuery(_eventId, _zonaId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
            _asientosMock.Verify(r => r.ListByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
