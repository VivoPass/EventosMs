using EventsService.Dominio.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace EventsService.Aplicacion.Commands.Zonas.ModificarZonaEvento
{
    public  class ModificarZonaEventoCommnand : IRequest<bool>
    {
        public Guid EventId { get; set; }
        public Guid ZonaId { get; set; }

        public string? Nombre { get; set; }
        public decimal? Precio { get; set; }
        public string? Estado { get; set; }
        public GridRef? Grid { get; set; } // opcional, solo si mueves la zona
    }
}
