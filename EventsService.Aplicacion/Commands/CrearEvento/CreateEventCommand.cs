using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace EventsService.Aplicacion.Commands.CrearEvento
{
    public sealed record CreateEventCommand(
        string Nombre,
        Guid CategoriaId,
        Guid EscenarioId,
        DateTimeOffset Inicio,
        DateTimeOffset Fin,
        int AforoMaximo,
        string? Tipo,
        string? Lugar,
        string? Descripcion,
        Guid OrganizadorId,
        string? OnlineMeetingUrl
    ) : IRequest<Guid>;

}
