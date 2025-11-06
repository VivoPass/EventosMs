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
    public class ZonaEvento
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonElement("eventId")]
        public Guid EventId { get; set; }

        [BsonElement("escenarioId")]
        public Guid EscenarioId { get; set; }

        [BsonElement("zonaBaseId")]
        public Guid? ZonaBaseId { get; set; } // opcional si clonas de una zona maestra

        [BsonElement("nombre")]
        public string Nombre { get; set; } = default!;

        [BsonElement("tipo")]
        public string Tipo { get; set; } = "sentado"; // general | sentado | manual

        [BsonElement("capacidad")]
        public int Capacidad { get; set; }

        [BsonElement("numeracion")]
        public Numeracion Numeracion { get; set; } = new();

        [BsonElement("precio")]
        public decimal? Precio { get; set; }

        [BsonElement("estado")]
        public string Estado { get; set; } = "activa"; // activa | bloqueada | oculta

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        public IEnumerable<Asiento> GenerarAsientos(Guid eventId)
        {
            // Solo aplica para zonas “sentado”
            if (!string.Equals(Tipo, "sentado", StringComparison.OrdinalIgnoreCase))
                yield break;

            var filas = Numeracion?.Filas ?? 0;
            var cols = Numeracion?.Columnas ?? 0;
            if (filas <= 0 || cols <= 0)
                yield break;

            // Invariante: capacidad = filas x columnas
            if (Capacidad != filas * cols)
                yield break;

            var prefFila = Numeracion?.PrefijoFila ?? "";
            var prefSeat = Numeracion?.PrefijoAsiento ?? "";

            for (int f = 0; f < filas; f++)
            {
                string filaEtiqueta = BuildRowLabel(prefFila, f); // A..Z, AA..AZ, BA..
                for (int c = 1; c <= cols; c++)
                {
                    string numEtiqueta = string.IsNullOrEmpty(prefSeat) ? c.ToString() : $"{prefSeat}{c}";

                    yield return new Asiento
                    {
                        Id = Guid.NewGuid(),
                        EventId = eventId,
                        ZonaEventoId = this.Id,
                        FilaIndex = f,
                        ColIndex = c,
                        Label = $"{filaEtiqueta}-{numEtiqueta}",
                        Estado = "disponible",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                }
            }
        }

        /// <summary>
        /// Genera etiquetas de fila en base 26: A..Z, AA..AZ, BA.. etc.
        /// Si el prefijo no es letra, usa 1..N.
        /// </summary>
        private static string BuildRowLabel(string seed, int index)
        {
            if (!string.IsNullOrWhiteSpace(seed) && char.IsLetter(seed[0]))
            {
                var n = index;
                var sb = new StringBuilder();
                do
                {
                    sb.Insert(0, (char)('A' + (n % 26)));
                    n = n / 26 - 1;
                } while (n >= 0);
                return sb.ToString();
            }
            return (index + 1).ToString();
        }
    }



}
