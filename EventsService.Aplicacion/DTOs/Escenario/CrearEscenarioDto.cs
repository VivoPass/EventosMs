using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Escenario
{
    public record CrearEscenarioDto(
        string Nombre,
        string Descripcion,
        string Ubicacion,
        string Ciudad,
        string Estado,
        string Pais
    );
}
