using EventsService.Dominio.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Entidades
{
    public class EscenarioZona
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Evento al que pertenece esta relación.
        /// </summary>
        [BsonElement("eventId")]
        public Guid EventId { get; set; }

        /// <summary>
        /// Escenario base donde se dibuja esta zona.
        /// </summary>
        [BsonElement("escenarioId")]
        public Guid EscenarioId { get; set; }

        /// <summary>
        /// ZonaEvento asociada (la unidad de venta o inventario que se dibuja aquí).
        /// </summary>
        [BsonElement("zonaEventoId")]
        public Guid ZonaEventoId { get; set; }

        /// <summary>
        /// Posición y tamaño dentro del grid del escenario.
        /// </summary>
        [BsonElement("grid")]
        public GridRef Grid { get; set; } = new GridRef();

        /// <summary>
        /// Color representativo para distinguir la zona visualmente (opcional).
        /// </summary>
        [BsonElement("color")]
        public string? Color { get; set; }

        /// <summary>
        /// Nivel de superposición visual (capas del mapa).
        /// </summary>
        [BsonElement("zIndex")]
        public int? ZIndex { get; set; }

        /// <summary>
        /// Indica si el bloque está visible en el layout.
        /// </summary>
        [BsonElement("visible")]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Fecha de creación del bloque.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Última modificación del bloque.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
