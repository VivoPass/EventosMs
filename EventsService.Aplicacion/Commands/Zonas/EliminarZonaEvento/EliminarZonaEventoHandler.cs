using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Dominio.Interfaces;
using MediatR;
using log4net;

namespace EventsService.Aplicacion.Commands.Zonas.EliminarZonaEvento
{
    public class EliminarZonaEventoHandler : IRequestHandler<EliminarZonaEventoCommand, bool>
    {
        private readonly IZonaEventoRepository _zonaRepo;
        private readonly IAsientoRepository _asientoRepo;
        private readonly IEscenarioZonaRepository _ezRepo;
        private readonly ILog _log;

        public EliminarZonaEventoHandler(
            IZonaEventoRepository zonaRepo,
            IAsientoRepository asientoRepo,
            IEscenarioZonaRepository ezRepo,
            ILog log)
        {
            _zonaRepo = zonaRepo ?? throw new ArgumentNullException(nameof(zonaRepo));
            _asientoRepo = asientoRepo ?? throw new ArgumentNullException(nameof(asientoRepo));
            _ezRepo = ezRepo ?? throw new ArgumentNullException(nameof(ezRepo));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<bool> Handle(EliminarZonaEventoCommand cmd, CancellationToken ct)
        {
            _log.Info($"Iniciando EliminarZonaEventoCommand. EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'.");

            try
            {
                // 1) Eliminar asientos de la zona
                _log.Debug($"Eliminando asientos asociados. EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'.");
                var deletedSeats = await _asientoRepo.DeleteByZonaAsync(cmd.EventId, cmd.ZonaId, ct);
                _log.Info($"Se eliminaron {deletedSeats} asientos de la zona '{cmd.ZonaId}'.");

                // 2) Eliminar bloque visual (EscenarioZona)
                _log.Debug($"Eliminando bloque visual (EscenarioZona) para EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'.");
                var ezDeleted = await _ezRepo.DeleteByZonaAsync(cmd.EventId, cmd.ZonaId, ct);
                _log.Info($"Resultado eliminación EscenarioZona para ZonaId='{cmd.ZonaId}': {(ezDeleted ? "eliminado" : "no encontrado")}.");

                // 3) Eliminar la zona en sí
                _log.Debug($"Eliminando ZonaEvento. EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'.");
                var zonaDeleted = await _zonaRepo.DeleteAsync(cmd.EventId, cmd.ZonaId, ct);

                if (zonaDeleted)
                {
                    _log.Info($"ZonaEvento eliminada correctamente. ZonaId='{cmd.ZonaId}', EventId='{cmd.EventId}'.");
                }
                else
                {
                    _log.Warn($"No se encontró ZonaEvento para eliminar. ZonaId='{cmd.ZonaId}', EventId='{cmd.EventId}'.");
                }

                return zonaDeleted;
            }
            catch (Exception ex)
            {
                _log.Error($"Error inesperado al eliminar zona del evento. EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'.", ex);
                throw new EliminarZonaEventoHandlerException(ex);
            }
        }
    }
}
