using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Infrastructura.Repositorios;
using EventsService.Infrastructura.Settings;
using log4net;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace EventsService.Test.Infraestructura.Repositorios
{
    public class Repository_AuditoriaRepository_Tests
    {
        private readonly Mock<IMongoDatabase> MockMongoDb;
        private readonly Mock<IMongoCollection<BsonDocument>> MockAuditoriaCollection;
        private readonly Mock<ILog> MockLogger;

        private readonly AuditoriaRepository Repository;

        // --- DATOS ---
        private readonly string TestEntidadId = Guid.NewGuid().ToString();
        private const string TestLevel = "INFO";
        private const string TestTipo = "EVENTO_PRUEBA";
        private const string TestMensaje = "Mensaje de prueba";

        public Repository_AuditoriaRepository_Tests()
        {
            // IConfiguration en memoria (no toca nada real)
            var settings = new Dictionary<string, string?>
            {
                ["Mongo:ConnectionString"] = "mongodb://localhost:27017",
                ["Mongo:AuditoriasDatabase"] = "db_auditoria_test"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();

            // Instancia real de AuditoriaDbConfig (va a crear un MongoClient real, pero lo vamos a sobreescribir)
            var mongoConfig = new AuditoriaDbConfig(configuration);

            // Mocks de Mongo y logger
            MockMongoDb = new Mock<IMongoDatabase>();
            MockAuditoriaCollection = new Mock<IMongoCollection<BsonDocument>>();
            MockLogger = new Mock<ILog>();

            MockMongoDb.Setup(d =>
                    d.GetCollection<BsonDocument>("auditoriaEventos",
                        It.IsAny<MongoCollectionSettings>()))
                .Returns(MockAuditoriaCollection.Object);

            // Inyectar el IMongoDatabase mock en la propiedad Db (auto-prop readonly)
            var dbField = typeof(AuditoriaDbConfig)
                .GetField("<Db>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

            if (dbField == null)
                throw new InvalidOperationException("No se encontró el backing field de Db en AuditoriaDbConfig.");

            dbField.SetValue(mongoConfig, MockMongoDb.Object);

            // Repositorio bajo prueba
            Repository = new AuditoriaRepository(mongoConfig, MockLogger.Object);
        }

        #region Ctor_LoggerNull_DebeLanzarLoggerNullException
        [Fact]
        public void Ctor_LoggerNull_DebeLanzarLoggerNullException()
        {
            // ARRANGE
            var settings = new Dictionary<string, string?>
            {
                ["Mongo:ConnectionString"] = "mongodb://localhost:27017",
                ["Mongo:AuditoriasDatabase"] = "db_auditoria_test"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();

            var mongoConfig = new AuditoriaDbConfig(configuration);

            // ACT & ASSERT
            Assert.Throws<LoggerNullException>(() =>
                new AuditoriaRepository(mongoConfig, null!));
        }
        #endregion

        #region InsertarAuditoriaEvento_InsercionExitosa_ShouldCompleteTask
        [Fact]
        public async Task InsertarAuditoriaEvento_InsercionExitosa_ShouldCompleteTask()
        {
            // ARRANGE
            MockAuditoriaCollection.Setup(c => c.InsertOneAsync(
                    It.IsAny<BsonDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // ACT
            await Repository.InsertarAuditoriaEvento(TestEntidadId, TestLevel, TestTipo, TestMensaje);

            // ASSERT
            MockAuditoriaCollection.Verify(c => c.InsertOneAsync(
                    It.IsAny<BsonDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region InsertarAuditoriaEvento_LanzaExcepcionMongo_ShouldThrowMongoException
        [Fact]
        public async Task InsertarAuditoriaEvento_LanzaExcepcionMongo_ShouldThrowMongoException()
        {
            // ARRANGE
            var mongoException = new MongoException("Error de inserción simulado.");
            MockAuditoriaCollection.Setup(c => c.InsertOneAsync(
                    It.IsAny<BsonDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(mongoException);

            // ACT & ASSERT
            var capturedException = await Assert.ThrowsAsync<MongoException>(
                () => Repository.InsertarAuditoriaEvento(TestEntidadId, TestLevel, TestTipo, TestMensaje));

            Assert.Equal(mongoException.Message, capturedException.Message);
        }
        #endregion

        #region InsertarAuditoriaEvento_LanzaExcepcion_ShouldThrowAuditoriaRepositoryException
        [Fact]
        public async Task InsertarAuditoriaEvento_LanzaExcepcion_ShouldThrowAuditoriaRepositoryException()
        {
            // ARRANGE
            var generalException = new Exception("Error general simulado.");
            MockAuditoriaCollection.Setup(c => c.InsertOneAsync(
                    It.IsAny<BsonDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(generalException);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<AuditoriaRepositoryException>(
                () => Repository.InsertarAuditoriaEvento(TestEntidadId, TestLevel, TestTipo, TestMensaje));

            Assert.Equal(generalException, exception.InnerException);
        }
        #endregion


        #region InsertarAuditoriaHistorial_InsercionExitosa_ShouldCompleteTask
        [Fact]
        public async Task InsertarAuditoriaHistorial_InsercionExitosa_ShouldCompleteTask()
        {
            // ARRANGE
            MockAuditoriaCollection.Setup(c => c.InsertOneAsync(
                    It.IsAny<BsonDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // ACT
            await Repository.InsertarAuditoriaHistorial(TestEntidadId, TestLevel, TestTipo, TestMensaje);

            // ASSERT
            MockAuditoriaCollection.Verify(c => c.InsertOneAsync(
                    It.IsAny<BsonDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region InsertarAuditoriaHistorial_LanzaExcepcionMongo_ShouldThrowMongoException
        [Fact]
        public async Task InsertarAuditoriaHistorial_LanzaExcepcionMongo_ShouldThrowMongoException()
        {
            // ARRANGE
            var mongoException = new MongoException("Error de inserción simulado.");
            MockAuditoriaCollection.Setup(c => c.InsertOneAsync(
                    It.IsAny<BsonDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(mongoException);

            // ACT & ASSERT
            var capturedException = await Assert.ThrowsAsync<MongoException>(
                () => Repository.InsertarAuditoriaHistorial(TestEntidadId, TestLevel, TestTipo, TestMensaje));

            Assert.Equal(mongoException.Message, capturedException.Message);
        }
        #endregion

        #region InsertarAuditoriaHistorial_LanzaExcepcion_ShouldThrowAuditoriaRepositoryException
        [Fact]
        public async Task InsertarAuditoriaHistorial_LanzaExcepcion_ShouldThrowAuditoriaRepositoryException()
        {
            // ARRANGE
            var generalException = new Exception("Error general simulado.");
            MockAuditoriaCollection.Setup(c => c.InsertOneAsync(
                    It.IsAny<BsonDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(generalException);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<AuditoriaRepositoryException>(
                () => Repository.InsertarAuditoriaHistorial(TestEntidadId, TestLevel, TestTipo, TestMensaje));

            Assert.Equal(generalException, exception.InnerException);
        }
        #endregion
    }
}
