using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace EventsService.Dominio.Entidades;

public class Evento
{
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid(); // se genera en C#
    public string Nombre { get; set; } = null!;

    [BsonRepresentation(BsonType.String)]
    public Guid CategoriaId { get; set; }


    [BsonRepresentation(BsonType.String)]
    public Guid EscenarioId { get; set; }
    public Guid OrganizadorId { get; set; }
    public DateTimeOffset Inicio { get; set; }
    public DateTimeOffset Fin { get; set; }
    public int AforoMaximo { get; set; }
    public string Estado { get; set; } = "Draft";
    public string? Tipo { get; set; }
    public string? Lugar { get; set; }
    public string? Descripcion { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}