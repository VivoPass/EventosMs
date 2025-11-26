using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.ValueObjects
{
    public class Numeracion
    {
        [BsonElement("modo")]
        public string Modo { get; set; } = "filas-columnas";

        [BsonElement("filas")]
        public int? Filas { get; set; }

        [BsonElement("columnas")]
        public int? Columnas { get; set; }

        [BsonElement("prefijoFila")]
        public string? PrefijoFila { get; set; } 

        [BsonElement("prefijoAsiento")]
        public string? PrefijoAsiento { get; set; } 
    }
}
