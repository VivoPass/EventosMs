// EventsService.Aplicacion/Commands/Eventos/SubirImagenEvento/SubirImagenEventoHandler.cs

using EventsService.Aplicacion.Commands.Evento;
using EventsService.Dominio.Interfaces;
using MediatR;

namespace EventsService.Aplicacion.Commands.Eventos.SubirImagenEvento
{
    public sealed class SubirImagenEventoHandler
        : IRequestHandler<SubirImagenEventoCommand, string>
    {
        private readonly IEventRepository _eventos;
        private readonly IFileStorageService _fileStorage;

        public SubirImagenEventoHandler(
            IEventRepository eventos,
            IFileStorageService fileStorage)
        {
            _eventos = eventos;
            _fileStorage = fileStorage;
        }

        public async Task<string> Handle(
            SubirImagenEventoCommand request,
            CancellationToken cancellationToken)
        {
            var evento = await _eventos.GetByIdAsync(request.EventoId, cancellationToken);
            if (evento is null)
                throw new Exception("Evento no encontrado."); // si tienes tu propia excepción, úsala aquí

            var url = await _fileStorage.UploadImageAsync(
                request.FileStream,
                request.FileName,
                cancellationToken);

            // Método de dominio para asignar la imagen
            evento.AsignarImagen(url);

            await _eventos.UpdateAsync(evento, cancellationToken);

            return url;
        }
    }
}