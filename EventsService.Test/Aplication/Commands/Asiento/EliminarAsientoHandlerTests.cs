//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using EventsService.Aplicacion.Commands.Asiento.EliminarAsiento;
//using EventsService.Dominio.Entidades;
//using EventsService.Dominio.Interfaces;
//using Moq;
//using Xunit;

//namespace EventsService.Test.Aplication.Commands.Asiento
//{
//    public class EliminarAsientoHandlerTests
//    {
//        private readonly Mock<IAsientoRepository> _asientosMock;
//        private readonly Mock<IZonaEventoRepository> _zonasMock;
//        private readonly EliminarAsientoHandler _handler;

//        private readonly Guid _eventId;
//        private readonly Guid _zonaId;
//        private readonly Guid _asientoId;

//        private ZonaEvento _zonaExistente;
//        private Dominio.Entidades.Asiento _asientoExistente;

//        public EliminarAsientoHandlerTests()
//        {
//            _asientosMock = new Mock<IAsientoRepository>();
//            _zonasMock = new Mock<IZonaEventoRepository>();

//            _handler = new EliminarAsientoHandler(_asientosMock.Object, _zonasMock.Object);

//            _eventId = Guid.NewGuid();
//            _zonaId = Guid.NewGuid();
//            _asientoId = Guid.NewGuid();

//            _zonaExistente = new ZonaEvento
//            {
//                Id = _zonaId,
//                EventId = _eventId,
//                Nombre = "Zona Test"
//            };

//            _asientoExistente = new Dominio.Entidades.Asiento
//            {
//                Id = _asientoId,
//                EventId = _eventId,
//                ZonaEventoId = _zonaId,
//                Label = "A1",
//                Estado = "available"
//            };

//            // Por defecto: zona existe
//            _zonasMock
//                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_zonaExistente);
//        }

//        [Fact]
//        public async Task Handle_ReturnsTrue_WhenDeleteSucceeds()
//        {
//            // Arrange
//            _asientosMock
//                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_asientoExistente);

//            _asientosMock
//                .Setup(r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            var cmd = new EliminarAsientoCommand(_eventId, _zonaId, _asientoId);

//            // Act
//            var result = await _handler.Handle(cmd, CancellationToken.None);

//            // Assert
//            Assert.True(result);
//            _zonasMock.Verify(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
//            _asientosMock.Verify(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()), Times.Once);
//            _asientosMock.Verify(r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()), Times.Once);
//        }

//        [Fact]
//        public async Task Handle_ReturnsFalse_WhenZonaInvalid()
//        {
//            // Arrange: zona no existe
//            _zonasMock
//                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((ZonaEvento?)null);

//            var cmd = new EliminarAsientoCommand(_eventId, _zonaId, _asientoId);

//            // Act
//            var result = await _handler.Handle(cmd, CancellationToken.None);

//            // Assert
//            Assert.False(result);
//            _asientosMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
//            _asientosMock.Verify(r => r.DeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public async Task Handle_ReturnsFalse_WhenSeatInvalid()
//        {
//            // Arrange: zona válida, pero asiento no existe
//            _asientosMock
//                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Dominio.Entidades.Asiento?)null);

//            var cmd = new EliminarAsientoCommand(_eventId, _zonaId, _asientoId);

//            // Act
//            var result = await _handler.Handle(cmd, CancellationToken.None);

//            // Assert
//            Assert.False(result);
//            _asientosMock.Verify(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()), Times.Once);
//            _asientosMock.Verify(r => r.DeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public async Task Handle_ReturnsFalse_WhenDeleteFails()
//        {
//            // Arrange: asiento existe, pero DeleteByIdAsync devuelve false
//            _asientosMock
//                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_asientoExistente);

//            _asientosMock
//                .Setup(r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(false);

//            var cmd = new EliminarAsientoCommand(_eventId, _zonaId, _asientoId);

//            // Act
//            var result = await _handler.Handle(cmd, CancellationToken.None);

//            // Assert
//            Assert.False(result);
//            _asientosMock.Verify(r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()), Times.Once);
//        }

//        [Fact]
//        public async Task Handle_PropagatesException_WhenDeleteThrows()
//        {
//            // Arrange: DeleteByIdAsync lanza excepción
//            _asientosMock
//                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_asientoExistente);

//            _asientosMock
//                .Setup(r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new InvalidOperationException("DB error"));

//            var cmd = new EliminarAsientoCommand(_eventId, _zonaId, _asientoId);

//            // Act & Assert
//            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(cmd, CancellationToken.None));
//            Assert.Equal("DB error", ex.Message);

//            // Se llamó GetByIdAsync, y luego se lanzó la excepción en DeleteByIdAsync
//            _asientosMock.Verify(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()), Times.Once);
//            _asientosMock.Verify(r => r.DeleteByIdAsync(_asientoId, It.IsAny<CancellationToken>()), Times.Once);
//        }
//    }
//}
