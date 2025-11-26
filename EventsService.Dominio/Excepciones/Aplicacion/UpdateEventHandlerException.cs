using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class UpdateEventHandlerException : Exception
    {
        public UpdateEventHandlerException(Exception inner)
            : base("Error inesperado al procesar la modificación del evento.", inner)
        {
        }
    }
}
