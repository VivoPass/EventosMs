using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.Evento
{
    public sealed record SubirImagenEventoCommand(
        Guid EventoId,
        Stream FileStream,
        string FileName
    ) : IRequest<string>;
}
