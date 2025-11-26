//using EventsService.Aplicacion.Commands.EliminarEscenario;
//using EventsService.Dominio.Interfaces;
//using MediatR;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EventsService.Test.Aplication.Commands.Escenario
//{
//    public class EliminarEscenarioHandlerTests
//    {
//        private readonly Mock<IScenarioRepository> _repoMock;
//        private readonly EliminarEscenarioHandler _handler;
//        private readonly Guid _idEscenario;
//        private readonly EliminarEscenarioCommand _command;

//        public EliminarEscenarioHandlerTests()
//        {
//            _repoMock = new Mock<IScenarioRepository>();
//            _handler = new EliminarEscenarioHandler(_repoMock.Object);
//            _idEscenario = Guid.NewGuid();
//            _command = new EliminarEscenarioCommand(_idEscenario.ToString());

//        }


//        [Fact]
//        public async Task Handle_DebeLlamarEliminarEscenario_UnaVezConIdCorrecto()
//        {
//            // Arrange
//            _repoMock
//                .Setup(r => r.EliminarEscenario(_idEscenario.ToString(), It.IsAny<CancellationToken>()))
//                .Returns(Task.CompletedTask)
//                .Verifiable();

//            // Act
//            var result = await _handler.Handle(_command, CancellationToken.None);

//            // Assert
//            Assert.Equal(Unit.Value, result);
//            _repoMock.Verify(r => r.EliminarEscenario(_idEscenario.ToString(), It.IsAny<CancellationToken>()), Times.Once);
//        }

//        [Fact]
//        public async Task Handle_SiRepositorioLanza_PropagaLaExcepcion()
//        {
//            // Arrange
//            var ex = new InvalidOperationException("Fallo al eliminar");
//            _repoMock
//                .Setup(r => r.EliminarEscenario(_idEscenario.ToString(), It.IsAny<CancellationToken>()))
//                .ThrowsAsync(ex);

//            // Act & Assert
//            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
//                _handler.Handle(_command, CancellationToken.None)
//            );

//            Assert.Equal("Fallo al eliminar", thrown.Message);
//            _repoMock.Verify(r => r.EliminarEscenario(_idEscenario.ToString(), It.IsAny<CancellationToken>()), Times.Once);
//        }
//    }
//}
