using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Infraestructura
{
    public class AuditoriaRepositoryException : Exception
    {
        public AuditoriaRepositoryException(Exception inner)
            : base("Error en la ejecución del repositorio de Auditoría.", inner)
        {
        }
    }
}
