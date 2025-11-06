using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Zonas
{
    public record EscenarioZonaBlock(
        GridRefBlock Grid,
        string? Color,
        int? ZIndex,
        bool Visible = true
    );

}

