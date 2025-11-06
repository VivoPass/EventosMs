using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Asiento;
using MediatR;

namespace EventsService.Aplicacion.Queries.Asiento.ListarAsientos
{
    public record ListarAsientosQuery(Guid EventId, Guid ZonaId) : IRequest<IReadOnlyList<AsientoDto>>;
}
