using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Infraestructura
{
    public class LoggerNullException : Exception
    {
        public LoggerNullException()
            : base("El logger proporcionado es nulo.")
        {
        }
    }
}
