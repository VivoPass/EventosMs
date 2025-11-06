using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Queries.ObtenerTodosEventos
{
    public sealed class GetAllEventsHandler : IRequestHandler<GetAllEventsQuery, List<Evento>>
    {
        private readonly IEventRepository _repo;
        public GetAllEventsHandler(IEventRepository repo) => _repo = repo;

        public Task<List<Evento>> Handle(GetAllEventsQuery request, CancellationToken ct)
            => _repo.GetAllAsync(ct);
    }
}
