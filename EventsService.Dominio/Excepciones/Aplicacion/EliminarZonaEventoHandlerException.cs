using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class EliminarZonaEventoHandlerException : Exception
    {
        public EliminarZonaEventoHandlerException(Exception inner)
            : base("Error inesperado al procesar la eliminación de la zona del evento.", inner)
        {
        }
    }
}
