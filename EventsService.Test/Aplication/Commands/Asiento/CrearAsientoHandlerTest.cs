using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.Asiento.CrearAsiento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Interfaces;
using Moq;
using Xunit;
using AsientoEntity = EventsService.Dominio.Entidades.Asiento;

namespace EventsService.Test.Aplication.Commands.Asiento
{
    public class CrearAsientoHandlerTests
    {
        private readonly Mock<IAsientoRepository> _asientosMock;
        private readonly Mock<IZonaEventoRepository> _zonasMock;
        private readonly CrearAsientoHandler _handler;

        // datos fake
        private readonly Guid _eventId;
        private readonly Guid _zonaId;
        private readonly Guid _asientoId;
        private ZonaEvento _zonaExistente;

        // captura del asiento insertado
        private AsientoEntity? _capturedSeat;

        public CrearAsientoHandlerTests()
        {
            _asientosMock = new Mock<IAsientoRepository>();
            _zonasMock = new Mock<IZonaEventoRepository>();

            _handler = new CrearAsientoHandler(_asientosMock.Object, _zonasMock.Object);

            _eventId = Guid.NewGuid();
            _zonaId = Guid.NewGuid();
            _asientoId = Guid.NewGuid();

            _zonaExistente = new ZonaEvento
            {
                Id = _zonaId,
                EventId = _eventId,
                Nombre = "Zona Test"
            };

            // Por defecto: la zona existe
            _zonasMock
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_zonaExistente);

            // Por defecto: no hay duplicado
            _asientosMock
                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AsientoEntity?)null);

            // InsertAsync captura el asiento y devuelve completed task
            _asientosMock
                .Setup(r => r.InsertAsync(It.IsAny<AsientoEntity>(), It.IsAny<CancellationToken>()))
                .Callback<AsientoEntity, CancellationToken>((s, ct) =>
                {
                    _capturedSeat = s;
                })
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task CreatesSeat_WhenInputValid()
        {
            // Arrange
            var label = " A-12 ";
            var trimmedLabel = "A-12";
            var cmd = new CrearAsientoCommand(
                EventId: _eventId,
                ZonaEventoId: _zonaId,
                FilaIndex: 1,
                ColIndex: 2,
                Label: label,
                Estado: null,
                Meta: null
            );

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(_capturedSeat);
            //Assert.Equal(_capturedSeat!.Id, result.Id);
            Assert.Equal(_eventId, _capturedSeat.EventId);
            Assert.Equal(_zonaId, _capturedSeat.ZonaEventoId);
            Assert.Equal(trimmedLabel, _capturedSeat.Label);
            Assert.Equal("disponible", _capturedSeat.Estado); // default cuando request.Estado == null
        }

        [Fact]
        public async Task Throws_WhenZonaNotFound()
        {
            // Arrange
            _zonasMock
                .Setup(r => r.GetAsync(_eventId, _zonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ZonaEvento?)null);

            var cmd = new CrearAsientoCommand(
                EventId: _eventId,
                ZonaEventoId: _zonaId,
                FilaIndex: 0,
                ColIndex: 0,
                Label: "X1",
                Estado: null,
                Meta: null
            );

            // Act & Assert
            await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(cmd, CancellationToken.None));

            // Insert no debe haberse llamado
            _asientosMock.Verify(r => r.InsertAsync(It.IsAny<AsientoEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Throws_WhenLabelIsNullOrWhitespace(string? badLabel)
        {
            // Arrange
            var cmd = new CrearAsientoCommand(
                EventId: _eventId,
                ZonaEventoId: _zonaId,
                FilaIndex: 0,
                ColIndex: 0,
                Label: badLabel,
                Estado: null,
                Meta: null
            );

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(cmd, CancellationToken.None));

            _asientosMock.Verify(r => r.InsertAsync(It.IsAny<AsientoEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Throws_WhenDuplicateExists()
        {
            // Arrange
            var label = "C-1";
            var dup = new AsientoEntity
            {
                Id = Guid.NewGuid(),
                EventId = _eventId,
                ZonaEventoId = _zonaId,
                Label = label
            };

            _asientosMock
                .Setup(r => r.GetByCompositeAsync(_eventId, _zonaId, label, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dup);

            var cmd = new CrearAsientoCommand(
                EventId: _eventId,
                ZonaEventoId: _zonaId,
                FilaIndex: 1,
                ColIndex: 1,
                Label: label,
                Estado: null,
                Meta: null
            );

            // Act & Assert
            await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(cmd, CancellationToken.None));

            // No debe insertar
            _asientosMock.Verify(r => r.InsertAsync(It.IsAny<AsientoEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
