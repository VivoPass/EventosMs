using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Excepciones
{
    public  class EventoException : Exception
    {
        public EventoException(string msg) : base(msg) { }
    }
}
