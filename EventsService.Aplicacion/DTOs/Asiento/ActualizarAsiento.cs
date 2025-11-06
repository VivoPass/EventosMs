using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Asiento
{
    public class ActualizarAsientoDto
    {
        public string? Label { get; set; }                   
        public string? Estado { get; set; }                   
        public Dictionary<string, string>? Meta { get; set; }  
    }
}
