using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventsService.Dominio.Entidades;


public class Escenario
{
    //[BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("nombre")]
    public string Nombre { get; set; }

    [BsonElement("descripcion")]
    public string Descripcion { get; set; }

    [BsonElement("ubicacion")]
    public string Ubicacion { get; set; } // dirección o “Ciudad, Estado, País”

    [BsonElement("ciudad")]
    public string Ciudad { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; }

    [BsonElement("pais")]
    public string Pais { get; set; }

    [BsonElement("capacidadTotal")]
    public int CapacidadTotal { get; set; } // se recalcula desde Zonas

    [BsonElement("activo")]
    public bool Activo { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int GridRows { get; set; } = 20; // tamaño del grid para el layout
    public int GridCols { get; set; } = 20;

}

