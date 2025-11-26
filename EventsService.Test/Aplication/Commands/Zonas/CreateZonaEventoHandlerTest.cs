//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using EventsService.Aplicacion.Commands.Zonas.CrearZonaEvento;
//using EventsService.Dominio.Entidades;
//using EventsService.Dominio.Excepciones;
//using EventsService.Dominio.Interfaces;
//using EventsService.Dominio.ValueObjects;
//using Moq;
//using Xunit;

//namespace EventsService.Test.Aplication.Commands.Zonas
//{
//    public class CreateZonaEventoHandlerTests
//    {
//        private readonly Mock<IZonaEventoRepository> _zonaRepoMock;
//        private readonly Mock<IEscenarioZonaRepository> _escenarioZonaRepoMock;
//        private readonly Mock<IAsientoRepository> _asientoRepoMock;
//        private readonly Mock<IScenarioRepository> _escenarioRepoMock;
//        private readonly CreateZonaEventoHandler _handler;

//        // datos fake
//        private readonly Guid _eventId;
//        private readonly Guid _escenarioId;
//        private readonly string _nombre;
//        private readonly Dominio.Entidades.Escenario _escenarioExistente;

//        // capturas
//        private ZonaEvento? _capturedZona;
//        private EscenarioZona? _capturedEscenarioZona;

//        public CreateZonaEventoHandlerTests()
//        {
//            _zonaRepoMock = new Mock<IZonaEventoRepository>();
//            _escenarioZonaRepoMock = new Mock<IEscenarioZonaRepository>();
//            _asientoRepoMock = new Mock<IAsientoRepository>();
//            _escenarioRepoMock = new Mock<IScenarioRepository>();

//            _handler = new CreateZonaEventoHandler(
//                _zonaRepoMock.Object,
//                _escenarioZonaRepoMock.Object,
//                _asientoRepoMock.Object,
//                _escenarioRepoMock.Object
//            );

//            _eventId = Guid.NewGuid();
//            _escenarioId = Guid.NewGuid();
//            _nombre = "Plate A";

//            _escenarioExistente = new Dominio.Entidades.Escenario
//            {
//                Id = _escenarioId,
//                Nombre = "Main Stage",
//                Descripcion = "Escenario prueba",
//                Ubicacion = "Av Test 1",
//                Ciudad = "Ciudad",
//                Estado = "State",
//                Pais = "Pais",
//                CapacidadTotal = 1000,
//                Activo = true
//            };

//            // por defecto: escenario existe
//            _escenarioRepoMock
//                .Setup(r => r.ObtenerEscenario(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_escenarioExistente);
//        }

//        [Fact]
//        public async Task CreatesZoneAndGeneratesSeats_WhenTipoSentadoAndAutoGenerate()
//        {
//            // Arrange
//            var filas = 5;
//            var cols = 4;
//            var capacidad = filas * cols;
//            var numeracion = new Numeracion { Filas = filas, Columnas = cols }; // asumo tu VO

//            // existsByNombre -> false
//            _zonaRepoMock
//                .Setup(r => r.ExistsByNombreAsync(_eventId, _nombre, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(false);

//            // Capturamos el zona que el handler crea
//            _zonaRepoMock
//                .Setup(r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
//                .Callback<ZonaEvento, CancellationToken>((z, ct) => _capturedZona = z)
//                .Returns(Task.CompletedTask);

//            _escenarioZonaRepoMock
//                .Setup(r => r.AddAsync(It.IsAny<EscenarioZona>(), It.IsAny<CancellationToken>()))
//                .Callback<EscenarioZona, CancellationToken>((ez, ct) => _capturedEscenarioZona = ez)
//                .Returns(Task.CompletedTask);

//            _asientoRepoMock
//                .Setup(r => r.BulkInsertAsync(It.IsAny<IEnumerable<Dominio.Entidades.Asiento>>(), It.IsAny<CancellationToken>()))
//                .Returns(Task.CompletedTask);

//            var command = new CreateZonaEventoCommand
//            {
//                EventId = _eventId,
//                EscenarioId = _escenarioId,
//                Nombre = _nombre,
//                Tipo = "sentado",
//                Capacidad = capacidad,
//                Numeracion = numeracion,
//                Precio = 10m,
//                Estado = "active",
//                AutogenerarAsientos = true
//            };

//            // Act
//            var result = await _handler.Handle(command, CancellationToken.None);

//            // Assert: se devolvió el Id de la zona creada
//            Assert.NotEqual(Guid.Empty, result);
//            Assert.NotNull(_capturedZona);
//            Assert.Equal(result, _capturedZona!.Id);

//            // Verificamos que AddAsync zona fue llamado
//            _zonaRepoMock.Verify(r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()), Times.Once);

//            // EscenarioZona agregado
//            _escenarioZonaRepoMock.Verify(r => r.AddAsync(It.IsAny<EscenarioZona>(), It.IsAny<CancellationToken>()), Times.Once);
//            Assert.NotNull(_capturedEscenarioZona);
//            Assert.Equal(_capturedZona!.Id, _capturedEscenarioZona!.ZonaEventoId);

//            // BulkInsert de asientos llamado con la cantidad filas*cols
//            _asientoRepoMock.Verify(r => r.BulkInsertAsync(It.Is<IEnumerable<Dominio.Entidades.Asiento>>(seq => seq.Count() == capacidad), It.IsAny<CancellationToken>()), Times.Once);
//        }

//        [Fact]
//        public async Task CreatesZoneWithoutGeneratingSeats_WhenTipoGeneral()
//        {
//            // Arrange
//            _zonaRepoMock
//                .Setup(r => r.ExistsByNombreAsync(_eventId, _nombre, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(false);

//            _zonaRepoMock
//                .Setup(r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()))
//                .Callback<ZonaEvento, CancellationToken>((z, ct) => _capturedZona = z)
//                .Returns(Task.CompletedTask);

//            _escenarioZonaRepoMock
//                .Setup(r => r.AddAsync(It.IsAny<EscenarioZona>(), It.IsAny<CancellationToken>()))
//                .Callback<EscenarioZona, CancellationToken>((ez, ct) => _capturedEscenarioZona = ez)
//                .Returns(Task.CompletedTask);

//            var command = new CreateZonaEventoCommand
//            {
//                EventId = _eventId,
//                EscenarioId = _escenarioId,
//                Nombre = _nombre,
//                Tipo = "general",
//                Capacidad = 200,
//                Numeracion = null,
                
//                Precio = 5m,
//                Estado = "active",
//                AutogenerarAsientos = false
//            };

//            // Act
//            var result = await _handler.Handle(command, CancellationToken.None);

//            // Assert
//            Assert.NotNull(_capturedZona);
//            Assert.Equal(result, _capturedZona!.Id);

//            _zonaRepoMock.Verify(r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()), Times.Once);
//            _escenarioZonaRepoMock.Verify(r => r.AddAsync(It.IsAny<EscenarioZona>(), It.IsAny<CancellationToken>()), Times.Once);

//            // BulkInsert nunca llamado
//            _asientoRepoMock.Verify(r => r.BulkInsertAsync(It.IsAny<IEnumerable<Dominio.Entidades.Asiento>>(), It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public async Task Throws_WhenEscenarioNotFound()
//        {
//            // Arrange
//            _escenarioRepoMock
//                .Setup(r => r.ObtenerEscenario(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Dominio.Entidades.Escenario?)null);

//            var command = new CreateZonaEventoCommand
//            {
//                EventId = _eventId,
//                EscenarioId = _escenarioId,
//                Nombre = _nombre,
//                Tipo = "general",
//                Capacidad = 100,
//                AutogenerarAsientos = false
//            };

//            // Act & Assert
//            await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(command, CancellationToken.None));

//            // No se debe haber intentado crear nada
//            _zonaRepoMock.Verify(r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public async Task Throws_WhenNombreDuplicado()
//        {
//            // Arrange
//            _zonaRepoMock
//                .Setup(r => r.ExistsByNombreAsync(_eventId, _nombre, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            var command = new CreateZonaEventoCommand
//            {
//                EventId = _eventId,
//                EscenarioId = _escenarioId,
//                Nombre = _nombre,
//                Tipo = "general",
//                Capacidad = 100,
//                AutogenerarAsientos = false
//            };

//            // Act & Assert
//            await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(command, CancellationToken.None));

//            _zonaRepoMock.Verify(r => r.ExistsByNombreAsync(_eventId, _nombre, It.IsAny<CancellationToken>()), Times.Once);
//            _zonaRepoMock.Verify(r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public async Task Throws_WhenFilasOrColumnasInvalid_ForSentado()
//        {
//            // Arrange: filas = 0
//            var numeracion = new Numeracion { Filas = 0, Columnas = 5 };

//            _zonaRepoMock
//                .Setup(r => r.ExistsByNombreAsync(_eventId, _nombre, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(false);

//            var command = new CreateZonaEventoCommand
//            {
//                EventId = _eventId,
//                EscenarioId = _escenarioId,
//                Nombre = _nombre,
//                Tipo = "sentado",
//                Numeracion = numeracion,
//                Capacidad = 0,
//                AutogenerarAsientos = false
//            };

//            await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(command, CancellationToken.None));

//            _zonaRepoMock.Verify(r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public async Task Throws_WhenCapacidadMismatch_ForSentado()
//        {
//            // Arrange: filas*cols = 6*6 = 36 but capacidad = 10
//            var numeracion = new Numeracion { Filas = 6, Columnas = 6 };

//            _zonaRepoMock
//                .Setup(r => r.ExistsByNombreAsync(_eventId, _nombre, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(false);

//            var command = new CreateZonaEventoCommand
//            {
//                EventId = _eventId,
//                EscenarioId = _escenarioId,
//                Nombre = _nombre,
//                Tipo = "sentado",
//                Numeracion = numeracion,
//                Capacidad = 10, // mismatch
//                AutogenerarAsientos = false
//            };

//            await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(command, CancellationToken.None));

//            _zonaRepoMock.Verify(r => r.AddAsync(It.IsAny<ZonaEvento>(), It.IsAny<CancellationToken>()), Times.Never);
//        }
//    }
//}
