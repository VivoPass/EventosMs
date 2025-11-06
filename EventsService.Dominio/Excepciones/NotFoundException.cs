using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones
{
    public  class NotFoundException : EventoException
    {
        public NotFoundException(string entity, object id)
            : base($"{entity} with id '{id}' was not found.") { }
    }
}
