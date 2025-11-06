using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.EliminarEvento
{
    public sealed class DeleteEventHandler : IRequestHandler<DeleteEventCommand, bool>
    {
        private readonly IEventRepository _repo;

        public DeleteEventHandler(IEventRepository repo) => _repo = repo;

        public Task<bool> Handle(DeleteEventCommand request, CancellationToken ct)
            => _repo.DeleteAsync(request.Id, ct);
    }
}
