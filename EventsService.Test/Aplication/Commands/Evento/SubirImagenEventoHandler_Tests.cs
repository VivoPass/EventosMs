using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.Evento;
using EventsService.Aplicacion.Commands.Eventos.SubirImagenEvento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using Moq;
using Xunit;

namespace EventsService.Test.Aplicacion.CommandHandlers.Eventos
{
    public class SubirImagenEventoHandler_Tests
    {
        private readonly Mock<IEventRepository> _mockEventRepo;
        private readonly Mock<IFileStorageService> _mockFileStorage;
        private readonly SubirImagenEventoHandler _handler;

        // --- DATOS ---
        private readonly Guid _eventoId;
        private readonly string _fileName;
        private readonly MemoryStream _fileStream;
        private readonly string _expectedUrl;

        public SubirImagenEventoHandler_Tests()
        {
            _mockEventRepo = new Mock<IEventRepository>();
            _mockFileStorage = new Mock<IFileStorageService>();

            _handler = new SubirImagenEventoHandler(
                _mockEventRepo.Object,
                _mockFileStorage.Object);

            _eventoId = Guid.NewGuid();
            _fileName = "banner-test.jpg";
            _fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            _expectedUrl = "https://cdn.test/eventos/banner-test.jpg";
        }

        private SubirImagenEventoCommand BuildCommand()
            => new SubirImagenEventoCommand(
                EventoId: _eventoId,
                FileStream: _fileStream,
                FileName: _fileName);

        private Evento BuildEvento()
            => new Evento
            {
                Id = _eventoId,
                Nombre = "Evento de prueba",
                Descripcion = "Desc",
                AforoMaximo = 100,
                Estado = "Draft",
                Inicio = DateTimeOffset.UtcNow.AddDays(1),
                Fin = DateTimeOffset.UtcNow.AddDays(1).AddHours(2),
            };

        #region Handle_Valido_DeberiaSubirImagen_ActualizarEvento_YRetornarUrl()
        [Fact]
        public async Task Handle_Valido_DeberiaSubirImagen_ActualizarEvento_YRetornarUrl()
        {
            // ARRANGE
            var command = BuildCommand();
            var evento = BuildEvento();

            _mockEventRepo
                .Setup(r => r.GetByIdAsync(_eventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(evento);

            _mockFileStorage
                .Setup(s => s.UploadImageAsync(
                    command.FileStream,
                    command.FileName,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_expectedUrl);

            Evento? eventoActualizado = null;
            _mockEventRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
                .Callback<Evento, CancellationToken>((e, _) => eventoActualizado = e)
                .ReturnsAsync(true);

            // ACT
            var resultUrl = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.Equal(_expectedUrl, resultUrl);
            Assert.NotNull(eventoActualizado);
            Assert.Equal(_expectedUrl, eventoActualizado!.ImagenUrl);

            _mockEventRepo.Verify(r => r.GetByIdAsync(_eventoId, It.IsAny<CancellationToken>()), Times.Once);
            _mockFileStorage.Verify(s => s.UploadImageAsync(
                    command.FileStream,
                    command.FileName,
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _mockEventRepo.Verify(r => r.UpdateAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion

        #region Handle_EventoNoEncontrado_DeberiaLanzarException()
        [Fact]
        public async Task Handle_EventoNoEncontrado_DeberiaLanzarException()
        {
            // ARRANGE
            var command = BuildCommand();

            _mockEventRepo
                .Setup(r => r.GetByIdAsync(_eventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Evento)null);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Evento no encontrado.", ex.Message);

            _mockFileStorage.Verify(s => s.UploadImageAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
            _mockEventRepo.Verify(r => r.UpdateAsync(
                It.IsAny<Evento>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_FalloEnStorage_DeberiaPropagarExcepcion()
        [Fact]
        public async Task Handle_FalloEnStorage_DeberiaPropagarExcepcion()
        {
            // ARRANGE
            var command = BuildCommand();
            var evento = BuildEvento();

            _mockEventRepo
                .Setup(r => r.GetByIdAsync(_eventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(evento);

            var storageException = new InvalidOperationException("Error subiendo a storage.");
            _mockFileStorage
                .Setup(s => s.UploadImageAsync(
                    command.FileStream,
                    command.FileName,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(storageException);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Error subiendo a storage.", ex.Message);

            _mockEventRepo.Verify(r => r.UpdateAsync(
                It.IsAny<Evento>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region Handle_UpdateAsyncRetornaFalse_IgualRetornaUrlPeroUpdateEsLlamado()
        [Fact]
        public async Task Handle_UpdateAsyncRetornaFalse_IgualRetornaUrlPeroUpdateEsLlamado()
        {
            // ARRANGE
            var command = BuildCommand();
            var evento = BuildEvento();

            _mockEventRepo
                .Setup(r => r.GetByIdAsync(_eventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(evento);

            _mockFileStorage
                .Setup(s => s.UploadImageAsync(
                    command.FileStream,
                    command.FileName,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_expectedUrl);

            _mockEventRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT
            var resultUrl = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            Assert.Equal(_expectedUrl, resultUrl);
            _mockEventRepo.Verify(r => r.UpdateAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion
    }
}
