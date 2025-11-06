using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace EventsService.Aplicacion.Commands.Asiento.CrearAsiento
{
    
        public record CrearAsientoCommand(
            Guid EventId,
            Guid ZonaEventoId,
            int? FilaIndex,
            int? ColIndex,
            string Label,
            string? Estado,
            Dictionary<string, string>? Meta
        ) : IRequest<CrearAsientoResult>;

        public record CrearAsientoResult(Guid AsientoId);
}
