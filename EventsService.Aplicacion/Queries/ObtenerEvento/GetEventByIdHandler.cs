using EventsService.Aplicacion.Queries.ObtenerEvento;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Queries.ObtenerEvento
{
    public sealed class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, Evento?>
    {
        private readonly IEventRepository _repository;

        public GetEventByIdHandler(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<Evento?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(request.Id, cancellationToken);
        }
    }
}

