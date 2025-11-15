using EventsService.Aplicacion.Commands.ModificarEscenario;
using EventsService.Dominio.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using EventsService.Dominio.Excepciones;

namespace EventsService.Test.Aplication.Commands.Escenario
{
    public class ModificarEscenarioHandlerTest
    {


        private readonly Mock<IScenarioRepository> _repoMock;
        private readonly ModificarEscenarioHandler _handler;

        // Datos fake
        private readonly Guid _idEscenario;
        private readonly string _id;
        private readonly Dominio.Entidades.Escenario _escenarioExistente;
        private readonly ModificarEscenarioCommand _command;

        public ModificarEscenarioHandlerTest()
        {
            _idEscenario = Guid.NewGuid();

            _id = _idEscenario.ToString();
            _escenarioExistente = new Dominio.Entidades.Escenario
            {
                Id = _idEscenario,
                Nombre = "Nombre existente",
                Descripcion = "Desc old",
                Ubicacion = "Ubic old",
                Ciudad = "City old",
                Estado = "State old",
                Pais = "Country old"
            };

            _repoMock = new Mock<IScenarioRepository>();

            _handler = new ModificarEscenarioHandler(_repoMock.Object);

            _command = new ModificarEscenarioCommand
            (
                Id: _idEscenario.ToString(),
                Nombre: "   Nuevo Nombre   ",
                Descripcion: "Nueva desc",
                Ubicacion: "Nueva ubic",
                Ciudad: "Nueva city",
                Estado: "Nuevo state",
                Pais: "Nuevo country");
        }

        [Fact]
        public async Task Handle_DebeModificarEscenario_CuandoExiste()
        {
            // Arrange
            _repoMock
                .Setup(r => r.ObtenerEscenario(_idEscenario.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_escenarioExistente);

            _repoMock
                .Setup(r => r.ModificarEscenario(_idEscenario.ToString(), It.IsAny<Dominio.Entidades.Escenario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(_command, CancellationToken.None);

            // Assert
            Assert.Equal(Unit.Value, result);

            _repoMock.Verify(r => r.ObtenerEscenario(_idEscenario.ToString(), It.IsAny<CancellationToken>()), Times.Once);

            _repoMock.Verify(r => r.ModificarEscenario(
                    _id,
                    It.Is<Dominio.Entidades.Escenario>(e =>
                        e.Id == _idEscenario &&
                        e.Nombre == "Nuevo Nombre" &&        
                        e.Descripcion == _command.Descripcion &&
                        e.Ubicacion == _command.Ubicacion &&
                        e.Ciudad == _command.Ciudad &&
                        e.Estado == _command.Estado &&
                        e.Pais == _command.Pais
                    ),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DebeLanzarExcepcion_CuandoEscenarioNoExiste()
        {
            // Arrange
            _repoMock
                .Setup(r => r.ObtenerEscenario(_idEscenario.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Dominio.Entidades.Escenario?)null);

            // Act + Assert
            await Assert.ThrowsAsync<EventoException>(() =>
                _handler.Handle(_command, CancellationToken.None)
            );

            _repoMock.Verify(r => r.ObtenerEscenario(_id, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.ModificarEscenario(It.IsAny<String>(), It.IsAny<Dominio.Entidades.Escenario>(), It.IsAny<CancellationToken>()), Times.Never);
        }

    };
        
    
}
