using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.DTOs.Asiento;
using MediatR;

namespace EventsService.Aplicacion.Queries.Asiento.ObtenerAsiento
{
    public record ObtenerAsientoQuery(Guid EventId, Guid ZonaId, Guid AsientoId) : IRequest<AsientoDto?>;
    
}
