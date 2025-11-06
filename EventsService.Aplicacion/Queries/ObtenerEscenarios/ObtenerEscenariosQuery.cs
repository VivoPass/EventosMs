using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Escenario;
using EventsService.Aplicacion.NewFolder;
using MediatR;

namespace EventsService.Aplicacion.Queries.ObtenerEscenarios
{
    public record ObtenerEscenariosQuery(
        string? Q,
        string? Ciudad,
        bool? Activo,
        int Page = 1,
        int PageSize = 20
    ) : IRequest<PagedResult<EscenarioDto>>;
}
