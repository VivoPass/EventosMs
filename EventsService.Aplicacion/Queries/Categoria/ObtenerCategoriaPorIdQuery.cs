using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Queries.Categoria
{
    public record ObtenerCategoriaPorIdQuery(Guid Id) : IRequest<Dominio.Entidades.Categoria>;
}
