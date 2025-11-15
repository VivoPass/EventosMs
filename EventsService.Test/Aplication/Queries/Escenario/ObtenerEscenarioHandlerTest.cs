using EventsService.Aplicacion.DTOs.Escenario;
using EventsService.Aplicacion.Queries.ObtenerEscenario;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Test.Aplication.Queries.Escenario
{
    public class ObtenerEscenarioHandlerTest
    {
        private readonly Mock<IScenarioRepository> _repoMock;
        private readonly ObtenerEscenarioHandler _handler;

        // Datos fake
        private readonly Guid _idEscenario;
        private readonly Dominio.Entidades.Escenario _escenarioExistente;

        private readonly ObtenerEscenarioQuery _query;

        public ObtenerEscenarioHandlerTest()
        {
            _idEscenario = Guid.NewGuid();

            _escenarioExistente = new Dominio.Entidades.Escenario
            {
                Id = _idEscenario,
                Nombre = "Sala Principal",
                Descripcion = "Escenario principal para conciertos",
                Ubicacion = "Av. Siempre Viva 123",
                Ciudad = "Ciudad Falsa",
                Estado = "Estado Ejemplo",
                Pais = "Pais Test",
                CapacidadTotal = 1500,
                Activo = true
            };

            _repoMock = new Mock<IScenarioRepository>();
            _handler = new ObtenerEscenarioHandler(_repoMock.Object);
            _query = new ObtenerEscenarioQuery(_idEscenario.ToString());
        }


        [Fact]
        public async Task Handle_ReturnsEscenarioDto_WhenEscenarioExists()
        {
            // Arrange
            _repoMock
                .Setup(r => r.ObtenerEscenario(_idEscenario.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_escenarioExistente);

            // Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<EscenarioDto>(result);

            Assert.Equal(_escenarioExistente.Id, result.Id);
            Assert.Equal(_escenarioExistente.Nombre, result.Nombre);
            Assert.Equal(_escenarioExistente.Descripcion, result.Descripcion);
            Assert.Equal(_escenarioExistente.Ubicacion, result.Ubicacion);
            Assert.Equal(_escenarioExistente.Ciudad, result.Ciudad);
            Assert.Equal(_escenarioExistente.Estado, result.Estado);
            Assert.Equal(_escenarioExistente.Pais, result.Pais);
            Assert.Equal(_escenarioExistente.CapacidadTotal, result.CapacidadTotal);
            Assert.Equal(_escenarioExistente.Activo, result.Activo);
        }

        [Fact]
        public async Task Handle_ThrowsNotFoundException_WhenEscenarioDoesNotExist()
        {
            // Arrange
            _repoMock
                .Setup(r => r.ObtenerEscenario(_idEscenario.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Dominio.Entidades.Escenario?)null);


            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(_query, CancellationToken.None)
            );

            // Opcional: verificar que la excepción contiene el id si tu NotFoundException lo expone (ajusta según implementación)
            Assert.Contains(_idEscenario.ToString(), ex.Message);

            _repoMock.Verify(r => r.ObtenerEscenario(_idEscenario.ToString(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
