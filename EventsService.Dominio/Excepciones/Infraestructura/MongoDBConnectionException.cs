using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Infraestructura
{
    public class MongoDBConnectionException : Exception
    {
        public MongoDBConnectionException(Exception inner)
            : base("Error al conectar con la base de datos MongoDB.", inner)
        {
        }
    }
}
