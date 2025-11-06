using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Escenario
{
    public record  BuscarEscenarioDto(
        string Id,
        string Nombre,
        string Descripcion,
        string Ubicacion,
        string Ciudad,
        string Estado,
        string Pais,
        int CapacidadTotal,
        bool Activo,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
