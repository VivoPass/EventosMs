using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace EventsService.Aplicacion.Commands.CrearEscenario
{
    public record CreateEscenarioCommand(
        string Nombre,
        string? Descripcion,
        string? Ubicacion,
        string? Ciudad,
        string? Estado,
        string? Pais
    ) : IRequest<string>;
}
