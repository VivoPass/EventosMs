using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.Excepciones.Infraestructura;
using MediatR;
using log4net;

namespace EventsService.Aplicacion.Commands.EliminarEvento
{
    public sealed class DeleteEventHandler : IRequestHandler<DeleteEventCommand, bool>
    {
        private readonly IEventRepository _repo;
        private readonly ILog _log;

        public DeleteEventHandler(IEventRepository repo, ILog log)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<bool> Handle(DeleteEventCommand request, CancellationToken ct)
        {
            _log.Info($"Iniciando DeleteEventCommand para ID='{request.Id}'.");

            try
            {
                var eliminado = await _repo.DeleteAsync(request.Id, ct);

                if (eliminado)
                {
                    _log.Info($"Evento eliminado correctamente. ID='{request.Id}'.");
                }
                else
                {
                    _log.Warn($"No se encontró evento para eliminar. ID='{request.Id}'. Operación retornó false.");
                }

                return eliminado;
            }
            catch (Exception ex)
            {
                _log.Error($"Error inesperado al eliminar evento ID='{request.Id}'.", ex);
                throw new DeleteEventHandlerException(ex);
            }
        }
    }
}