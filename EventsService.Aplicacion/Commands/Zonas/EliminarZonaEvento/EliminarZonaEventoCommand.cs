using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.Zonas.EliminarZonaEvento
{


    public class EliminarZonaEventoCommand : IRequest<bool>
    {
        public Guid EventId { get; set; }
        public Guid ZonaId { get; set; }
    }
}
