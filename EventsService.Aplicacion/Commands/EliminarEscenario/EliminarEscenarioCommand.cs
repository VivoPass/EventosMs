using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.EliminarEscenario
{

    public record EliminarEscenarioCommand(string Id) : IRequest<Unit>;
}
