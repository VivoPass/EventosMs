// EventsService.Aplicacion/Commands/Eventos/SubirFolletoEvento/SubirFolletoEventoHandler.cs

using EventsService.Aplicacion.Commands.Evento;
using EventsService.Dominio.Interfaces;
using MediatR;

namespace EventsService.Aplicacion.Commands.Eventos.SubirFolletoEvento
{
    public sealed class SubirFolletoEventoHandler
        : IRequestHandler<SubirFolletoEventoCommand, string>
    {
        private readonly IEventRepository _eventos;
        private readonly IFileStorageService _fileStorage;

        public SubirFolletoEventoHandler(
            IEventRepository eventos,
            IFileStorageService fileStorage)
        {
            _eventos = eventos;
            _fileStorage = fileStorage;
        }

        public async Task<string> Handle(
            SubirFolletoEventoCommand request,
            CancellationToken cancellationToken)
        {
            var evento = await _eventos.GetByIdAsync(request.EventoId, cancellationToken);
            if (evento is null)
                throw new Exception("Evento no encontrado.");

            var url = await _fileStorage.UploadFileAsync(
                request.FileStream,
                request.FileName,
                cancellationToken);

            evento.AsignarFolleto(url);

            await _eventos.UpdateAsync(evento, cancellationToken);

            return url;
        }
    }
}