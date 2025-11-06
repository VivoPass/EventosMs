using EventsService.Dominio.Entidades;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Zonas;

namespace EventsService.Aplicacion.Queries.Zona.ListarZonasEvento
{
    public class ListarZonasEventoQuery : IRequest<IReadOnlyList<ZonaEventoDto>>
    {
        public Guid EventId { get; set; }
        public string? Tipo { get; set; }
        public string? Estado { get; set; }
        public string? Search { get; set; }
        public bool IncludeSeats { get; set; } = false;
    }
}
