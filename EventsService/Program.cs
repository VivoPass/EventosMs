using EventsService.Aplicacion.Commands.CrearEvento;   // CreateEventCommand
using EventsService.Dominio.Interfaces;                 // IEventRepository, ICategoryRepository, IScenarioRepository
//using EventsService.Infraestructura.Mongo;              // EventCollections
using EventsService.Infraestructura.Repositories;       // EventRepositoryMongo, etc.
using EventsService.Infrastructura.mongo;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using EventsService.Aplicacion.Commands.Zonas.CrearZonaEvento;
using EventsService.Infrastructura.Repositorios;
using System.Reflection;
using Microsoft.OpenApi.Models;
using EventsService.Infrastructura.Cloudinary;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Swagger ----------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EventsService API",
        Version = "v1",
        Description = "Microservicio de eventos (zonas, asientos, escenarios)."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

// ---------------- Mongo (desde appsettings) ----------------
// Sugerencia: en appsettings.json -> "Mongo": { "ConnectionString": "mongodb://127.0.0.1:27017", "Database": "events" }
var conn = builder.Configuration["Mongo:ConnectionString"] ?? "mongodb://127.0.0.1:27017";
var dbName = builder.Configuration["Mongo:Database"] ?? "Events";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(conn));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(dbName);
});
builder.Services.AddSingleton<EventCollections>();

// ---------------- Repositorios ----------------
builder.Services.AddScoped<IEventRepository, EventRepositoryMongo>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepositoryMongo>();
builder.Services.AddScoped<IScenarioRepository, ScenarioRepositoryMongo>();
builder.Services.AddScoped<IAsientoRepository, AsientoRepository>();
builder.Services.AddScoped<IZonaEventoRepository, ZonaEventoRepository>();
builder.Services.AddScoped<IEscenarioZonaRepository, EscenarioZonaRepository>();

// ---------------- MediatR (v12+) ----------------
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateEventCommand).Assembly));

// ---------------- FluentValidation ----------------
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreateEventCommand).Assembly);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateZonaEventoHandler).Assembly));
// ---------------- Cloudinary ----------------
builder.Services.AddInfrastructure(builder.Configuration);   // ← AQUÍ LO REGISTRAS

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
var app = builder.Build();

// ---------------- Crear índices mínimos (sin depender de tus clases) ----------------
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    var eventos = db.GetCollection<BsonDocument>("eventos");

    var models = new[]
    {
        new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("Inicio")),
        new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("Estado"))
    };

    await eventos.Indexes.CreateManyAsync(models);
});

// ---------------- Pipeline HTTP ----------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventsService v1");
        c.DocumentTitle = "EventsService API";
    });
}


app.UseMiddleware<MiddlewareExceptions>();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
