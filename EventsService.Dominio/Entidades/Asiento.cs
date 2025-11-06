using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Entidades
{
    /// <summary>
    /// Representa un asiento individual dentro de una ZonaEvento.
    /// Cada asiento pertenece a un evento y a una zona específica.
    /// </summary>
    public class Asiento
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Evento al que pertenece el asiento.
        /// </summary>
        [BsonElement("eventId")]
        public Guid EventId { get; set; }

        /// <summary>
        /// Zona del evento donde se ubica este asiento.
        /// </summary>
        [BsonElement("zonaEventoId")]
        public Guid ZonaEventoId { get; set; }

        /// <summary>
        /// Fila del asiento (puede ser índice o etiqueta).
        /// </summary>
        [BsonElement("filaIndex")]
        public int? FilaIndex { get; set; }

        /// <summary>
        /// Columna del asiento (si aplica numeración por grilla).
        /// </summary>
        [BsonElement("colIndex")]
        public int? ColIndex { get; set; }

        /// <summary>
        /// Etiqueta visible del asiento, ejemplo: "A-1", "B-3".
        /// </summary>
        [BsonElement("label")]
        public string Label { get; set; } = default!;

        /// <summary>
        /// Estado actual del asiento.
        /// disponible | reservado | vendido | bloqueado
        /// </summary>
        [BsonElement("estado")]
        public string Estado { get; set; } = "disponible";

        /// <summary>
        /// Información adicional opcional (por ejemplo, tipo, visibilidad, precio dinámico).
        /// </summary>
        [BsonElement("meta")]
        public Dictionary<string, string>? Meta { get; set; }

        /// <summary>
        /// Fecha de creación del asiento.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización del asiento.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
