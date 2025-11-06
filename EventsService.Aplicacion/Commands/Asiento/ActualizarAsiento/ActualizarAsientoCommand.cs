using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.Asiento.ActualizarAsiento
{
    public record ActualizarAsientoCommand(
        Guid EventId,
        Guid ZonaId,
        Guid AsientoId,
        string? Label,
        string? Estado,
        Dictionary<string, string>? Meta
    ) : IRequest<bool>;
}
