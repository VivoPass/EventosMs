using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Aplicacion
{
    public class ModificarZonaEventoHandlerException : Exception
    {
        public ModificarZonaEventoHandlerException(Exception inner)
            : base("Error inesperado al procesar la modificación de la zona del evento.", inner)
        {
        }
    }
}
