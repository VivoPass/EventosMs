using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Asiento
{
    public class CrearAsientoDto
    {
        public int? FilaIndex { get; set; }
        public int? ColIndex { get; set; }
        public string Label { get; set; } = default!;
        public string? Estado { get; set; }
        public Dictionary<string, string>? Meta { get; set; }
    }
}
