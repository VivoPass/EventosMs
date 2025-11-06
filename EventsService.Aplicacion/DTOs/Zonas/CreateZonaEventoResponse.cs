using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Zonas
{
    public record CreateZonaEventoResponse(Guid ZonaEventoId, Guid EscenarioZonaId, int? AsientosCreados);
}
  