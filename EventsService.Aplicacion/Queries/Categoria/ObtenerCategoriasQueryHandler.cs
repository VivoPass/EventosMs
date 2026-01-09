using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Queries.Categoria
{
    public class ObtenerCategoriasQueryHandler : IRequestHandler<ObtenerCategoriasQuery, List<Dominio.Entidades.Categoria>>
    {
        private readonly ICategoryRepository _repository;

        public ObtenerCategoriasQueryHandler(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Dominio.Entidades.Categoria>> Handle(ObtenerCategoriasQuery request, CancellationToken ct)
        {
            return await _repository.GetAllAsync(ct);
        }
    }
}
