using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class EliminarEscenarioHandlerException : Exception
    {
        public EliminarEscenarioHandlerException(Exception inner)
            : base("Error inesperado al procesar la eliminación del escenario.", inner)
        {
        }
    }
}
