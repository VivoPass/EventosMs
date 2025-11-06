using EventsService.Dominio.Entidades;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Queries.ObtenerEvento
{
    public sealed record GetEventByIdQuery(Guid Id) : IRequest<Evento?>;
}



