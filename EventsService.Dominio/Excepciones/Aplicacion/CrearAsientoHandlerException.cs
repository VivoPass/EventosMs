using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class CrearAsientoHandlerException : Exception
    {
        public CrearAsientoHandlerException(Exception inner)
            : base("Error inesperado al procesar la creación del asiento.", inner)
        {
        }
    }
}
