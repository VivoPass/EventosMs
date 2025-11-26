using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Infraestructura
{
    public class ConexionBdInvalida : Exception
    {
        public ConexionBdInvalida()
            : base("La cadena de conexión a MongoDB no fue proporcionada o es inválida.")
        {
        }
    }
}
