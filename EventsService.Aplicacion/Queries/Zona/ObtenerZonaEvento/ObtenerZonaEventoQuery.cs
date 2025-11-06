using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Zonas;
using MediatR;

namespace EventsService.Aplicacion.Queries.Zona.ObtenerZonaEvento
{
    public class ObtenerZonaEventoQuery : IRequest<ZonaEventoDto>
    {
        public Guid EventId { get; set; }
        public Guid ZonaId { get; set; }
        public bool IncludeSeats { get; set; } = false;
    }
}
