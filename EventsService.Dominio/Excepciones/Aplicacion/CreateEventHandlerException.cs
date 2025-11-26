using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class CreateEventHandlerException : Exception
    {
        public CreateEventHandlerException(Exception inner)
            : base("Error inesperado al procesar la creación del evento.", inner)
        {
        }
    }
}
