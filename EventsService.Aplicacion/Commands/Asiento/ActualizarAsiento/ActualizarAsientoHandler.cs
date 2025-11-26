using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Dominio.Interfaces;
using MediatR;
using log4net;

namespace EventsService.Aplicacion.Commands.Asiento.ActualizarAsiento
{
    public class ActualizarAsientoHandler : IRequestHandler<ActualizarAsientoCommand, bool>
    {
        private readonly IAsientoRepository _asientos;
        private readonly IZonaEventoRepository _zonas;
        private readonly ILog _log;

        public ActualizarAsientoHandler(
            IAsientoRepository asientos,
            IZonaEventoRepository zonas,
            ILog log)
        {
            _asientos = asientos ?? throw new ArgumentNullException(nameof(asientos));
            _zonas = zonas ?? throw new ArgumentNullException(nameof(zonas));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<bool> Handle(ActualizarAsientoCommand r, CancellationToken ct)
        {
            _log.Info($"Iniciando ActualizarAsientoCommand. EventId='{r.EventId}', ZonaId='{r.ZonaId}', AsientoId='{r.AsientoId}'.");

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

                // 2) Cargar asiento
                _log.Debug($"Cargando asiento AsientoId='{r.AsientoId}'.");
                var seat = await _asientos.GetByIdAsync(r.AsientoId, ct);
                if (seat is null || seat.ZonaEventoId != r.ZonaId || seat.EventId != r.EventId)
                {
                    _log.Warn($"Asiento no encontrado o no coincide con zona/evento. AsientoId='{r.AsientoId}', EventId='{r.EventId}', ZonaId='{r.ZonaId}'. Se retornará false.");
                    return false;
                }

                // 3) Validaciones puntuales: normalizar label si viene
                if (!string.IsNullOrWhiteSpace(r.Label))
                {
                    var trimmed = r.Label!.Trim();
                    _log.Debug($"Normalizando label. Original='{r.Label}', Trimmed='{trimmed}'.");
                    r = r with { Label = trimmed };
                }

                // 4) Evitar duplicado si cambia Label
                if (!string.IsNullOrWhiteSpace(r.Label) &&
                    !r.Label!.Equals(seat.Label, StringComparison.Ordinal))
                {
                    _log.Debug($"Verificando duplicado para nuevo label. EventId='{r.EventId}', ZonaId='{r.ZonaId}', NuevoLabel='{r.Label}'.");
                    var dup = await _asientos.GetByCompositeAsync(r.EventId, r.ZonaId, r.Label!, ct);
                    if (dup is not null)
                    {
                        _log.Warn($"Actualización de asiento cancelada. Ya existe asiento con label='{r.Label}' en la misma zona. AsientoId='{r.AsientoId}'.");
                        throw new EventoException("Ya existe un asiento con ese label en esta zona.");
                    }
                }

                // 5) Update parcial
                _log.Debug($"Ejecutando UpdateParcialAsync para AsientoId='{r.AsientoId}'.");
                var updated = await _asientos.UpdateParcialAsync(
                    r.AsientoId,
                    nuevoLabel: r.Label,
                    nuevoEstado: r.Estado,
                    nuevaMeta: r.Meta,
                    ct: ct
                );

                if (updated)
                {
                    _log.Info($"Asiento actualizado correctamente. AsientoId='{r.AsientoId}'.");
                }
                else
                {
                    _log.Warn($"UpdateParcialAsync retornó false. No se aplicaron cambios. AsientoId='{r.AsientoId}'.");
                }

                return updated;
            }
            catch (EventoException)
            {
                // Error de dominio ya logueado como Warn
                throw;
            }
            catch (Exception ex)
            {
                _log.Error($"Error inesperado al actualizar asiento AsientoId='{r.AsientoId}'.", ex);
                throw new ActualizarAsientoHandlerException(ex);
            }
        }
    }
}
