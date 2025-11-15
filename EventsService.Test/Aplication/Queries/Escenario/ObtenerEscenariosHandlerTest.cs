using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Escenario;
using EventsService.Aplicacion.NewFolder;
using EventsService.Aplicacion.Queries.ObtenerEscenarios;
using EventsService.Dominio.Interfaces;
using Moq;
using Xunit;

namespace EventsService.Test.Aplication.Queries.Escenario
{
    public class ObtenerEscenariosHandlerTests
    {
        private readonly Mock<IScenarioRepository> _repoMock;
        private readonly ObtenerEscenariosHandler _handler;

        // Datos fake
        private readonly List<global::EventsService.Dominio.Entidades.Escenario> _escenarios;
        private readonly Guid _id1;
        private readonly Guid _id2;
        private readonly ObtenerEscenariosQuery _query;

        public ObtenerEscenariosHandlerTests()
        {
            _repoMock = new Mock<IScenarioRepository>();
            _handler = new ObtenerEscenariosHandler(_repoMock.Object);

            _id1 = Guid.NewGuid();
            _id2 = Guid.NewGuid();

            _escenarios = new List<global::EventsService.Dominio.Entidades.Escenario>
            {
                new global::EventsService.Dominio.Entidades.Escenario
                {
                    Id = _id1,
                    Nombre = "Sala A",
                    Descripcion = "Descripción A",
                    Ubicacion = "Ubic A",
                    Ciudad = "CiudadX",
                    Estado = "StateA",
                    Pais = "PaisA",
                    CapacidadTotal = 500,
                    Activo = true
                },
                new global::EventsService.Dominio.Entidades.Escenario
                {
                    Id = _id2,
                    Nombre = "Sala B",
                    Descripcion = "Descripción B",
                    Ubicacion = "Ubic B",
                    Ciudad = "CiudadY",
                    Estado = "StateB",
                    Pais = "PaisB",
                    CapacidadTotal = 300,
                    Activo = false
                }
            };

            // Ajusta según el constructor real de tu query. Esto coincide con tu uso previo:
            _query = new ObtenerEscenariosQuery("Sala", "x", null, 1, 20);
        }

        [Fact]
        public async Task Handle_ReturnsPagedResult_WithMappedDtos()
        {
            // Arrange
            long expectedTotal = 2L;

            _repoMock
                .Setup(r => r.SearchAsync(
                    _query.Q ?? "",
                    _query.Ciudad ?? "",
                    _query.Activo,
                    _query.Page,
                    _query.PageSize,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((items: (IReadOnlyList<global::EventsService.Dominio.Entidades.Escenario>)_escenarios.AsReadOnly(), total: expectedTotal));

            // Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResult<EscenarioDto>>(result);
            Assert.Equal(_query.Page, result.Page);
            Assert.Equal(_query.PageSize, result.PageSize);
            Assert.Equal(expectedTotal, result.Total);

            Assert.Equal(2, result.Items.Count);
            var dto1 = result.Items.SingleOrDefault(d => d.Id == _id1);
            var dto2 = result.Items.SingleOrDefault(d => d.Id == _id2);

            Assert.NotNull(dto1);
            Assert.Equal("Sala A", dto1.Nombre);
            Assert.Equal("Descripción A", dto1.Descripcion);
            Assert.Equal("Ubic A", dto1.Ubicacion);
            Assert.Equal("CiudadX", dto1.Ciudad);
            Assert.Equal(500, dto1.CapacidadTotal);
            Assert.True(dto1.Activo);

            Assert.NotNull(dto2);
            Assert.Equal("Sala B", dto2.Nombre);
            Assert.False(dto2.Activo);

            _repoMock.Verify(r => r.SearchAsync(
                _query.Q ?? "",
                _query.Ciudad ?? "",
                _query.Activo,
                _query.Page,
                _query.PageSize,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyPagedResult_WhenNoItems()
        {
            // Arrange
            _repoMock
                .Setup(r => r.SearchAsync(
                    _query.Q ?? "",
                    _query.Ciudad ?? "",
                    _query.Activo,
                    _query.Page,
                    _query.PageSize,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((items: (IReadOnlyList<global::EventsService.Dominio.Entidades.Escenario>)Array.Empty<global::EventsService.Dominio.Entidades.Escenario>().ToList().AsReadOnly(), total: 0L));


            // Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0L, result.Total);
            Assert.Equal(_query.Page, result.Page);
            Assert.Equal(_query.PageSize, result.PageSize);

            _repoMock.Verify(r => r.SearchAsync(
                _query.Q ?? "",
                _query.Ciudad ?? "",
                _query.Activo,
                _query.Page,
                _query.PageSize,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
