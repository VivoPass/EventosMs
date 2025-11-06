using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Zonas
{
    public record NumeracionBlock(
        string Modo,               // "sin-asientos" | "filas-columnas" | "lista-manual"
        int? Filas,
        int? Columnas,
        string? PrefijoFila,
        string? PrefijoAsiento
    );

}
