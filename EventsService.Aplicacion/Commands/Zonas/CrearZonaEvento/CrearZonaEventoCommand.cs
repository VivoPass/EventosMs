using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Zonas;
using EventsService.Dominio.ValueObjects;
using MediatR;

namespace EventsService.Aplicacion.Commands.Zonas.CrearZonaEvento
{
    public class CreateZonaEventoCommand : IRequest<Guid>
    {
        public Guid EventId { get; set; }
        public Guid EscenarioId { get; set; }
        public string Nombre { get; set; } = default!;
        public string Tipo { get; set; } = "general"; // general | sentado | manual
        public int Capacidad { get; set; }
        public Numeracion Numeracion { get; set; } = new Numeracion();
        public decimal? Precio { get; set; }
        public string Estado { get; set; } = "activa";
        public GridRef Grid { get; set; } = new GridRef();
        public bool AutogenerarAsientos { get; set; } = true;
    }
}
