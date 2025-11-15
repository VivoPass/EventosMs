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
    public class EventRepositoryMongoTests : IDisposable
    {
        private readonly MongoDbRunner _runner;
        private readonly IMongoDatabase _db;
        private readonly EventRepositoryMongo _sut; // System Under Test

        public EventRepositoryMongoTests()
        {
            // 1) Configurar mapeos de Mongo
            MongoMappings.Configure();

            // 2) Levantar Mongo embebido
            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            _db = client.GetDatabase("events_service_tests");

            var collections = new EventCollections(_db);
            _sut = new EventRepositoryMongo(collections);
        }

        [Fact]
        public async Task InsertAsync_Then_GetByIdAsync_Should_Return_Inserted_Event()
        {
            // Arrange
            var ct = CancellationToken.None;
            var evento = CrearEventoDePrueba();

            // Act
            await _sut.InsertAsync(evento, ct);
            var result = await _sut.GetByIdAsync(evento.Id, ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(evento.Id, result!.Id);
            Assert.Equal(evento.Nombre, result.Nombre);
            Assert.Equal(evento.Estado, result.Estado);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            var ct = CancellationToken.None;
            var randomId = Guid.NewGuid();

            // Act
            var result = await _sut.GetByIdAsync(randomId, ct);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Inserted_Events()
        {
            // Arrange
            var ct = CancellationToken.None;
            var e1 = CrearEventoDePrueba();
            var e2 = CrearEventoDePrueba();
            e2.Id = Guid.NewGuid();
            e2.Nombre = "Otro evento";

            await _sut.InsertAsync(e1, ct);
            await _sut.InsertAsync(e2, ct);

            // Act
            var result = await _sut.GetAllAsync(ct);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_True_And_Persist_Changes()
        {
            // Arrange
            var ct = CancellationToken.None;
            var evento = CrearEventoDePrueba();
            await _sut.InsertAsync(evento, ct);

            var nuevoNombre = "Nombre Actualizado";
            evento.Nombre = nuevoNombre;
            evento.Estado = "Published";

            // Act
            var updated = await _sut.UpdateAsync(evento, ct);
            var recargado = await _sut.GetByIdAsync(evento.Id, ct);

            // Assert
            Assert.True(updated);
            Assert.NotNull(recargado);
            Assert.Equal(nuevoNombre, recargado!.Nombre);
            Assert.Equal("Published", recargado.Estado);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_Event_Does_Not_Exist()
        {
            // Arrange
            var ct = CancellationToken.None;
            var evento = CrearEventoDePrueba();
            evento.Id = Guid.NewGuid(); // no insertado

            // Act
            var updated = await _sut.UpdateAsync(evento, ct);

            // Assert
            Assert.False(updated);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_True_And_Remove_Event()
        {
            // Arrange
            var ct = CancellationToken.None;
            var evento = CrearEventoDePrueba();
            await _sut.InsertAsync(evento, ct);

            // Act
            var deleted = await _sut.DeleteAsync(evento.Id, ct);
            var recargado = await _sut.GetByIdAsync(evento.Id, ct);

            // Assert
            Assert.True(deleted);
            Assert.Null(recargado);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_Event_Does_Not_Exist()
        {
            // Arrange
            var ct = CancellationToken.None;
            var randomId = Guid.NewGuid();

            // Act
            var deleted = await _sut.DeleteAsync(randomId, ct);

            // Assert
            Assert.False(deleted);
        }

        private static Evento CrearEventoDePrueba()
        {
            return new Evento
            {
                Id = Guid.NewGuid(),
                Nombre = "Concierto Sinfónico",
                CategoriaId = Guid.NewGuid(),
                EscenarioId = Guid.NewGuid(),
                OrganizadorId = Guid.NewGuid(),
                Inicio = DateTimeOffset.UtcNow.AddDays(7),
                Fin = DateTimeOffset.UtcNow.AddDays(7).AddHours(2),
                AforoMaximo = 500,
                Estado = "Draft",
                Tipo = "Concierto",
                Lugar = "Teatro Municipal",
                Descripcion = "Evento de prueba para tests de integración."
            };
        }

        public void Dispose()
        {
            _runner?.Dispose();
        }
    }
}
