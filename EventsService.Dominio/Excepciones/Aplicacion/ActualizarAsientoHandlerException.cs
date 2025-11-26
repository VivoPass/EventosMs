using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class ActualizarAsientoHandlerException : Exception
    {
        public ActualizarAsientoHandlerException(Exception inner)
            : base("Error inesperado al procesar la actualización del asiento.", inner)
        {
        }
    }
}
