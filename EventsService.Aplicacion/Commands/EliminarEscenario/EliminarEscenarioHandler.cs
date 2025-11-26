using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Dominio.Interfaces;
using MediatR;
using log4net;

namespace EventsService.Aplicacion.Commands.EliminarEscenario
{
    public class EliminarEscenarioHandler : IRequestHandler<EliminarEscenarioCommand, Unit>
    {
        private readonly IScenarioRepository _repo;
        private readonly ILog _log;

        public EliminarEscenarioHandler(IScenarioRepository repo, ILog log)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<Unit> Handle(EliminarEscenarioCommand r, CancellationToken ct)
        {
            _log.Info($"Iniciando EliminarEscenarioCommand para ID='{r.Id}'.");

            try
            {
                await _repo.EliminarEscenario(r.Id, ct);

                // El repositorio ya loguea si no encontró nada o si eliminó correctamente.
                _log.Info($"EliminarEscenarioCommand completado para ID='{r.Id}'.");
                return Unit.Value;
            }
            catch (Exception ex)
            {
                _log.Error($"Error inesperado al eliminar escenario ID='{r.Id}'.", ex);
                throw new EliminarEscenarioHandlerException(ex);
            }
        }
    }
}