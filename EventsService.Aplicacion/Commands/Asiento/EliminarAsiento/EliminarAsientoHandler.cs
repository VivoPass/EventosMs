using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Dominio.Interfaces;
using MediatR;
using log4net;

namespace EventsService.Aplicacion.Commands.Asiento.EliminarAsiento
{
    public class EliminarAsientoHandler : IRequestHandler<EliminarAsientoCommand, bool>
    {
        private readonly IAsientoRepository _asientos;
        private readonly IZonaEventoRepository _zonas;
        private readonly ILog _log;

        public EliminarAsientoHandler(
            IAsientoRepository asientos,
            IZonaEventoRepository zonas,
            ILog log)
        {
            _asientos = asientos ?? throw new ArgumentNullException(nameof(asientos));
            _zonas = zonas ?? throw new ArgumentNullException(nameof(zonas));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<bool> Handle(EliminarAsientoCommand r, CancellationToken ct)
        {
            _log.Info($"Iniciando EliminarAsientoCommand. EventId='{r.EventId}', ZonaId='{r.ZonaId}', AsientoId='{r.AsientoId}'.");

            try
            {
                // 1) Validar zona ↔ evento
                _log.Debug($"Validando zona para EventId='{r.EventId}', ZonaId='{r.ZonaId}'.");
                var zona = await _zonas.GetAsync(r.EventId, r.ZonaId, ct);
                if (zona is null || zona.EventId != r.EventId)
                {
                    _log.Warn($"Zona inválida o no asociada al evento. EventId='{r.EventId}', ZonaId='{r.ZonaId}'. Se retornará false.");
                    return false;
                }

                // 2) Validar que el asiento pertenezca
                _log.Debug($"Buscando asiento AsientoId='{r.AsientoId}'.");
                var seat = await _asientos.GetByIdAsync(r.AsientoId, ct);
                if (seat is null || seat.ZonaEventoId != r.ZonaId || seat.EventId != r.EventId)
                {
                    _log.Warn($"Asiento no encontrado o no pertenece a la zona/evento. AsientoId='{r.AsientoId}', EventId='{r.EventId}', ZonaId='{r.ZonaId}'. Se retornará false.");
                    return false;
                }

                // 3) Eliminar asiento
                _log.Debug($"Ejecutando DeleteByIdAsync para AsientoId='{r.AsientoId}'.");
                var deleted = await _asientos.DeleteByIdAsync(r.AsientoId, ct);

                if (deleted)
                {
                    _log.Info($"Asiento eliminado correctamente. AsientoId='{r.AsientoId}'.");
                }
                else
                {
                    _log.Warn($"DeleteByIdAsync retornó false. No se eliminó el asiento. AsientoId='{r.AsientoId}'.");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _log.Error($"Error inesperado al eliminar asiento AsientoId='{r.AsientoId}'.", ex);
                throw new EliminarAsientoHandlerException(ex);
            }
        }
    }
}
