using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class EliminarAsientoHandlerException : Exception
    {
        public EliminarAsientoHandlerException(Exception inner)
            : base("Error inesperado al procesar la eliminación del asiento.", inner)
        {
        }
    }
}
