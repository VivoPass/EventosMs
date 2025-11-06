using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Zonas
{
    public record OpcionesBlock(
        bool AutogenerarAsientos = true,
        bool RegenerarSiInconsistente = false
    );
}
