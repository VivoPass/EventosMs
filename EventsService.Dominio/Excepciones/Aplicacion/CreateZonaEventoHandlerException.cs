using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class CreateZonaEventoHandlerException : Exception
    {
        public CreateZonaEventoHandlerException(Exception inner)
            : base("Error inesperado al procesar la creación de la zona del evento.", inner)
        {
        }
    }
}
