using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Escenario;

namespace EventsService.Aplicacion.Queries.ObtenerEscenario
{
    public record ObtenerEscenarioQuery(string Id) : IRequest<EscenarioDto>;
}
