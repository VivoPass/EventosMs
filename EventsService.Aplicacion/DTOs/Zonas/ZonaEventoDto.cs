using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Asiento;

namespace EventsService.Aplicacion.DTOs.Zonas
{
    public class ZonaEventoDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid EscenarioId { get; set; }
        public string Nombre { get; set; } = default!;
        public string Tipo { get; set; } = default!;
        public int Capacidad { get; set; }
        public decimal? Precio { get; set; }
        public string Estado { get; set; } = default!;
        public GridDto Grid { get; set; } = new();
        public List<AsientoDto>? Asientos { get; set; } // null si includeSeats=false
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
