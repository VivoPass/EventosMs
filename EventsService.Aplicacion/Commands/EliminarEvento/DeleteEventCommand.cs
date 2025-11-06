using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.EliminarEvento
{
    public sealed record DeleteEventCommand(Guid Id) : IRequest<bool>;
}
