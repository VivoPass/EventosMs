//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using EventsService.Aplicacion.Commands.Asiento.ActualizarAsiento;
//using EventsService.Dominio.Entidades;
//using EventsService.Dominio.Excepciones;
//using EventsService.Dominio.Interfaces;
//using Moq;
//using Xunit;

//namespace EventsService.Test.Aplication.Commands.Asiento
//{
//    public class ActualizarAsientoHandlerTests
//    {
//        private readonly Mock<IAsientoRepository> _asientosMock;
//        private readonly Mock<IZonaEventoRepository> _zonasMock;
//        private readonly ActualizarAsientoHandler _handler;

//        // datos fake
//        private readonly Guid _eventId;
//        private readonly Guid _zonaId;
//        private readonly Guid _asientoId;
//        private ZonaEvento _zonaExistente;
//        private Dominio.Entidades.Asiento _asientoExistente;

//        public ActualizarAsientoHandlerTests()
//        {
//            _asientosMock = new Mock<IAsientoRepository>();
//            _zonasMock = new Mock<IZonaEventoRepository>();

//            _handler = new ActualizarAsientoHandler(_asientosMock.Object, _zonasMock.Object);

//            _eventId = Guid.NewGuid();
//            _zonaId = Guid.NewGuid();
//            _asientoId = Guid.NewGuid();

//            // Zona existente (válida)
//            _zonaExistente = new ZonaEvento
//            {
//                Id = _zonaId,
//                EventId = _eventId,
//                Nombre = "Zona Test"
//            };

//            // Asiento existente y consistente con zona/evento
//            _asientoExistente = new Dominio.Entidades.Asiento
//            {
//                Id = _asientoId,
//                EventId = _eventId,
//                ZonaEventoId = _zonaId,
//                Label = "A1",
//                Estado = "available",
//                Meta = null
//            };

//            // Por defecto la zona existe
//            _zonasMock
//                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_zonaExistente);
//        }

//        [Fact]
//        public async Task Handle_ReturnsTrue_WhenUpdateSucceeds()
//        {
//            // Arrange
//            var newLabelWithSpaces = " B2 ";
//            var trimmedLabel = "B2";

//            // asiento existe
//            _asientosMock
//                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_asientoExistente);

//            // no hay duplicado
//            _asientosMock
//                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, trimmedLabel, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Dominio.Entidades.Asiento?)null);

//            // Preparar meta como diccionario
//            var metaDict = new Dictionary<string, string> { { "note", "meta-x" } };

//            bool updateCalled = false;
//            _asientosMock
//                .Setup(r => r.UpdateParcialAsync(
//                    _asientoId,
//                    trimmedLabel,
//                    "reserved",
//                    It.Is<Dictionary<string, string>?>(d => d != null && d.ContainsKey("note") && d["note"] == "meta-x"),
//                    It.IsAny<CancellationToken>()))
//                .Callback<Guid, string?, string?, Dictionary<string, string>?, CancellationToken>((id, lbl, est, meta, ct) => updateCalled = true)
//                .ReturnsAsync(true);

//            var cmd = new ActualizarAsientoCommand(
//                _eventId,
//                _zonaId,
//                _asientoId,
//                newLabelWithSpaces,
//                "reserved",
//                metaDict
//            );

//            // Act
//            var result = await _handler.Handle(cmd, CancellationToken.None);

//            // Assert
//            Assert.True(result);
//            Assert.True(updateCalled);

//            _zonasMock.Verify(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()), Times.Once);
//            _asientosMock.Verify(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()), Times.Once);
//            _asientosMock.Verify(r => r.GetByCompositeAsync(_eventId, _zonaId, trimmedLabel, It.IsAny<CancellationToken>()), Times.Once);
//            _asientosMock.Verify(r => r.UpdateParcialAsync(_asientoId, trimmedLabel, "reserved", It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()), Times.Once);
//        }

//        [Fact]
//        public async Task Handle_ReturnsFalse_WhenZonaInvalid()
//        {
//            // Arrange: zona no encontrada
//            _zonasMock
//                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((ZonaEvento?)null);

//            var cmd = new ActualizarAsientoCommand(
//                _eventId,
//                _zonaId,
//                _asientoId,
//                "X",
//                null,
//                null
//            );

//            // Act
//            var result = await _handler.Handle(cmd, CancellationToken.None);

//            // Assert: devuelve false y no llama a repos de asientos
//            Assert.False(result);
//            _asientosMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
//            _asientosMock.Verify(r => r.UpdateParcialAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public async Task Handle_ReturnsFalse_WhenSeatInvalid()
//        {
//            // Arrange: zona existe, pero asiento no existe
//            _asientosMock
//                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Dominio.Entidades.Asiento?)null);

//            var cmd = new ActualizarAsientoCommand(
//                _eventId,
//                _zonaId,
//                _asientoId,
//                "X",
//                null,
//                null
//            );

//            // Act
//            var result = await _handler.Handle(cmd, CancellationToken.None);

//            // Assert
//            Assert.False(result);
//            _asientosMock.Verify(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()), Times.Once);
//            _asientosMock.Verify(r => r.UpdateParcialAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public async Task Handle_ThrowsEventoException_WhenDuplicateLabelFound()
//        {
//            // Arrange
//            var newLabel = "C3";

//            _asientosMock
//                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_asientoExistente);

//            // Simulamos que hay un asiento duplicado con ese label
//            var dup = new Dominio.Entidades.Asiento { Id = Guid.NewGuid(), EventId = _eventId, ZonaEventoId = _zonaId, Label = newLabel };
//            _asientosMock
//                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, newLabel, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(dup);

//            var cmd = new ActualizarAsientoCommand(
//                _eventId,
//                _zonaId,
//                _asientoId,
//                newLabel,
//                null,
//                null
//            );

//            // Act & Assert
//            await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(cmd, CancellationToken.None));

//            // Verificamos que no se llamó a UpdateParcialAsync
//            _asientosMock.Verify(r => r.UpdateParcialAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public async Task Handle_ReturnsFalse_WhenUpdateParcialFails()
//        {
//            // Arrange
//            var newLabel = "D4";

//            _asientosMock
//                .Setup(r => r.GetByIdAsync(_asientoId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_asientoExistente);

//            _asientosMock
//                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, newLabel, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Dominio.Entidades.Asiento?)null);

//            // UpdateParcial devuelve false (fallo) — aquí aceptamos cualquier meta
//            _asientosMock
//                .Setup(r => r.UpdateParcialAsync(_asientoId, newLabel, "locked", It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(false);

//            var metaDict = new Dictionary<string, string> { { "note", "meta-z" } };

//            var cmd = new ActualizarAsientoCommand(
//                _eventId,
//                _zonaId,
//                _asientoId,
//                newLabel,
//                "locked",
//                metaDict
//            );

//            // Act
//            var result = await _handler.Handle(cmd, CancellationToken.None);

//            // Assert
//            Assert.False(result);
//            _asientosMock.Verify(r => r.UpdateParcialAsync(_asientoId, newLabel, "locked", It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()), Times.Once);
//        }
//    }
//}
