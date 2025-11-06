using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Zonas
{
    public record CreateZonaEventoRequest(
        Guid EscenarioId,
        ZonaBlock Zona,
        EscenarioZonaBlock EscenarioZona,
        OpcionesBlock Opciones
    );
}
