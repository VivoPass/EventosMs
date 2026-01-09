using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Queries.Categoria
{
    public class ObtenerCategoriaPorIdQueryHandler : IRequestHandler<ObtenerCategoriaPorIdQuery, Dominio.Entidades.Categoria>
    {
        private readonly ICategoryRepository _repository;

        public ObtenerCategoriaPorIdQueryHandler(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Dominio.Entidades.Categoria> Handle(ObtenerCategoriaPorIdQuery request, CancellationToken ct)
        {
            var categoria = await _repository.GetByIdAsync(request.Id, ct);

            if (categoria is null)
                throw new Exception("No se encontro " + request.Id);

            return categoria;
        }
    }

}
