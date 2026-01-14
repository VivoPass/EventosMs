using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace EventsService.Aplicacion.Commands.ModificarEvento
{
    public sealed record UpdateEventCommand(
        Guid Id,
        string? Nombre,
        Guid? CategoriaId,
        Guid? EscenarioId,
        DateTimeOffset? Inicio,
        DateTimeOffset? Fin,
        int? AforoMaximo,
        string? Tipo,
        string? Lugar,
        string? Descripcion,
        string? OnlineMeetingUrl
    ) : IRequest<bool>;
}
