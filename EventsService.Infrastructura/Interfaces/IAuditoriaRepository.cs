using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Infrastructura.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task InsertarAuditoriaEvento(string idEntidad, string level, string tipo, string mensaje);
        Task InsertarAuditoriaHistorial(string idEntidad, string level, string tipo, string mensaje);
    }
}
