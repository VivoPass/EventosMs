namespace EventsService.Dominio.Entidades;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Categoria
{
    [BsonId] 
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("Nombre")]
    public string Nombre { get; set; } = null!;

    [BsonElement("Descripcion")]
    public string? Descripcion { get; set; }
}