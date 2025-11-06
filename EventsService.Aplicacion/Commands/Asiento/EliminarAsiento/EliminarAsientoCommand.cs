using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.Asiento.EliminarAsiento
{
    public record EliminarAsientoCommand(Guid EventId, Guid ZonaId, Guid AsientoId) : IRequest<bool>;
}
