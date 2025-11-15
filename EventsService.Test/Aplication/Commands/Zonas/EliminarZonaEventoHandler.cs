using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.Zonas.EliminarZonaEvento;
using EventsService.Dominio.Interfaces;
using Moq;
using Xunit;

namespace EventsService.Test.Aplication.Commands.Zonas
{
    public class EliminarZonaEventoHandlerTests
    {
        private readonly Mock<IZonaEventoRepository> _zonaRepoMock;
        private readonly Mock<IAsientoRepository> _asientoRepoMock;
        private readonly Mock<IEscenarioZonaRepository> _ezRepoMock;
        private readonly EliminarZonaEventoHandler _handler;

        private readonly Guid _eventId;
        private readonly Guid _zonaId;

        public EliminarZonaEventoHandlerTests()
        {
            _zonaRepoMock = new Mock<IZonaEventoRepository>();
            _asientoRepoMock = new Mock<IAsientoRepository>();
            _ezRepoMock = new Mock<IEscenarioZonaRepository>();

            _handler = new EliminarZonaEventoHandler(
                _zonaRepoMock.Object,
                _asientoRepoMock.Object,
                _ezRepoMock.Object
            );

            _eventId = Guid.NewGuid();
            _zonaId = Guid.NewGuid();
        }

        [Fact]
        public async Task Handle_ReturnsTrue_WhenDeleteSucceeds()
        {
            // Arrange
            // Asientos: devuelve long (filas afectadas)
            _asientoRepoMock
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1L)
                .Verifiable();

            // EscenarioZona: devuelve bool indicando éxito
            _ezRepoMock
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // ZonaEvento: devuelve bool indicando éxito
            _zonaRepoMock
                .Setup(r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            var cmd = new EliminarZonaEventoCommand { EventId = _eventId, ZonaId = _zonaId };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.True(result);
            _asientoRepoMock.Verify(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
            _ezRepoMock.Verify(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
            _zonaRepoMock.Verify(r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFalse_WhenZonaDeleteFails()
        {
            // Arrange
            // Asientos: devolvemos 1L (ok)
            _asientoRepoMock
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1L);

            // EscenarioZona: devolvemos true (ok)
            _ezRepoMock
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ZonaEvento: devuelve false -> eliminación final falla
            _zonaRepoMock
                .Setup(r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var cmd = new EliminarZonaEventoCommand { EventId = _eventId, ZonaId = _zonaId };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.False(result);
            _asientoRepoMock.Verify(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
            _ezRepoMock.Verify(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
            _zonaRepoMock.Verify(r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PropagatesException_WhenAsientoRepoThrows()
        {
            // Arrange
            _asientoRepoMock
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB failure in asiento repo"));

            var cmd = new EliminarZonaEventoCommand { EventId = _eventId, ZonaId = _zonaId };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(cmd, CancellationToken.None));
            Assert.Equal("DB failure in asiento repo", ex.Message);

            // Si falla en asientos no se debe intentar borrar ez ni zona
            _ezRepoMock.Verify(r => r.DeleteByZonaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _zonaRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CallsRepos_InCorrectOrder()
        {
            // Arrange: MockSequence para asegurar orden de llamadas
            var seq = new MockSequence();

            _asientoRepoMock.InSequence(seq)
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1L);

            _ezRepoMock.InSequence(seq)
                .Setup(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _zonaRepoMock.InSequence(seq)
                .Setup(r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var cmd = new EliminarZonaEventoCommand { EventId = _eventId, ZonaId = _zonaId };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.True(result);

            _asientoRepoMock.Verify(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
            _ezRepoMock.Verify(r => r.DeleteByZonaAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
            _zonaRepoMock.Verify(r => r.DeleteAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
