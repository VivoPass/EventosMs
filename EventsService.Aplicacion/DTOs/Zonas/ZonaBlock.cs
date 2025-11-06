using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Zonas
{
    public record ZonaBlock(
        string Nombre,
        string Tipo,               // "general" | "sentado" | "manual"
        int Capacidad,
        NumeracionBlock Numeracion,
        decimal? Precio,
        string Estado = "activa"
    );
}
