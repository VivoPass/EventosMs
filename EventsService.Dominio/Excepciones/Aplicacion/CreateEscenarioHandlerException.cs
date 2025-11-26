using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class CreateEscenarioHandlerException : Exception
    {
        public CreateEscenarioHandlerException(Exception inner)
            : base("Error inesperado al procesar la creación del escenario.", inner)
        {
        }
    }
}
