using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class DeleteEventHandlerException : Exception
    {
        public DeleteEventHandlerException(Exception inner)
            : base("Error inesperado al procesar la eliminación del evento.", inner)
        {
        }
    }
}
