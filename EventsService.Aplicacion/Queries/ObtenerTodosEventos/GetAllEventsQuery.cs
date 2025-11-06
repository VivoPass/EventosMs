using EventsService.Dominio.Entidades;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Queries.ObtenerTodosEventos
{
    public sealed record GetAllEventsQuery() : IRequest<List<Evento>>;
}
