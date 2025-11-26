using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Infraestructura
{
    public class MongoDBUnnexpectedException : Exception
    {
        public MongoDBUnnexpectedException(Exception inner)
            : base("Error inesperado en la interacción con MongoDB.", inner)
        {
        }
    }
}
