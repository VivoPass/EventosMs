using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones.Infraestructura
{
    public class NombreBdInvalido : Exception
    {
        public NombreBdInvalido()
            : base("El nombre de la base de datos es inválido o no fue proporcionado.")
        {
        }
    }
}
