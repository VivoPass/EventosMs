using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Infraestructura.Repositories;
using EventsService.Infrastructura.mongo;
using Mongo2Go;
using MongoDB.Driver;
using Xunit;

namespace EventsService.Tests.Infraestructura.Repositories
{
    public class ScenarioRepositoryMongoTests : IDisposable
    {
        private readonly MongoDbRunner _runner;
        private readonly IMongoDatabase _db;
        private readonly ScenarioRepositoryMongo _sut;

        public ScenarioRepositoryMongoTests()
        {
            // Configurar mapeos
            MongoMappings.Configure();

            // Mongo embebido
            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            _db = client.GetDatabase("events_service_scenario_tests");

            var collections = new EventCollections(_db);
            _sut = new ScenarioRepositoryMongo(collections);
        }

        [Fact]
        public async Task CrearAsync_Should_Insert_Scenario_And_Return_Id()
        {
            // Arrange
            var ct = CancellationToken.None;
            var escenario = CrearEscenario("Escenario Principal", "Caracas", true);

            // Act
            var idString = await _sut.CrearAsync(escenario, ct);
            var loaded = await _sut.ObtenerEscenario(idString, ct);

            // Assert
            Assert.Equal(escenario.Id.ToString(), idString);
            Assert.NotNull(loaded);
            Assert.Equal("Escenario Principal", loaded.Nombre);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_True_When_Scenario_Exists()
        {
            // Arrange
            var ct = CancellationToken.None;
            var escenario = CrearEscenario("Escenario 1", "Caracas", true);
            await _sut.CrearAsync(escenario, ct);

            // Act
            var exists = await _sut.ExistsAsync(escenario.Id, ct);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_When_Scenario_Does_Not_Exist()
        {
            // Arrange
            var ct = CancellationToken.None;
            var randomId = Guid.NewGuid();

            // Act
            var exists = await _sut.ExistsAsync(randomId, ct);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ObtenerEscenario_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            var ct = CancellationToken.None;
            var randomId = Guid.NewGuid().ToString();

            // Act
            var result = await _sut.ObtenerEscenario(randomId, ct);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ModificarEscenario_Should_Update_Fields()
        {
            // Arrange
            var ct = CancellationToken.None;
            var original = CrearEscenario("Teatro Viejo", "Caracas", true);
            await _sut.CrearAsync(original, ct);

            var cambios = new Escenario
            {
                Id = original.Id, // importante
                Nombre = "Teatro Renovado",
                Descripcion = "Ahora con mejor acústica",
                Ubicacion = "Centro"
            };

            // Act
            await _sut.ModificarEscenario(original.Id.ToString(), cambios, ct);
            var updated = await _sut.ObtenerEscenario(original.Id.ToString(), ct);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal("Teatro Renovado", updated.Nombre);
            Assert.Equal("Ahora con mejor acústica", updated.Descripcion);
            Assert.Equal("Centro", updated.Ubicacion);
        }

        [Fact]
        public async Task EliminarEscenario_Should_Remove_Scenario()
        {
            // Arrange
            var ct = CancellationToken.None;
            var escenario = CrearEscenario("Escenario a borrar", "Valencia", true);
            await _sut.CrearAsync(escenario, ct);

            // Act
            await _sut.EliminarEscenario(escenario.Id.ToString(), ct);
            var loaded = await _sut.ObtenerEscenario(escenario.Id.ToString(), ct);
            var exists = await _sut.ExistsAsync(escenario.Id, ct);

            // Assert
            Assert.Null(loaded);
            Assert.False(exists);
        }

        [Fact]
        public async Task SearchAsync_Should_Filter_By_Search_City_And_Activo()
        {
            // Arrange
            var ct = CancellationToken.None;

            var e1 = CrearEscenario("Teatro Municipal", "Caracas", true);
            var e2 = CrearEscenario("Teatro Nacional", "Caracas", false);
            var e3 = CrearEscenario("Plaza Central", "Valencia", true);

            await _sut.CrearAsync(e1, ct);
            await _sut.CrearAsync(e2, ct);
            await _sut.CrearAsync(e3, ct);

            // Act
            var (items, total) = await _sut.SearchAsync(
                search: "Teatro",   // coincide con e1 y e2
                ciudad: "Caracas",  // filtra solo Caracas
                activo: true,       // filtra solo activos
                page: 1,
                pageSize: 10,
                ct: ct);

            // Assert
            Assert.Equal(1, total);
            Assert.Single(items);
            Assert.Equal("Teatro Municipal", items[0].Nombre);
        }

        private static Escenario CrearEscenario(string nombre, string ciudad, bool activo)
        {
            return new Escenario
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = $"Descripción de {nombre}",
                Ubicacion = "Ubicación X",
                Ciudad = ciudad,
                Activo = activo
            };
        }

        public void Dispose()
        {
            _runner?.Dispose();
        }
    }
}
