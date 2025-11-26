using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Excepciones.Infraestructura;
using MediatR;
using AsientoEntity = EventsService.Dominio.Entidades.Asiento;
using log4net;

namespace EventsService.Aplicacion.Commands.Asiento.CrearAsiento
{
    public class CrearAsientoHandler : IRequestHandler<CrearAsientoCommand, CrearAsientoResult>
    {
        private readonly IAsientoRepository _asientos;
        private readonly IZonaEventoRepository _zonas;
        private readonly ILog _log;

        public CrearAsientoHandler(
            IAsientoRepository asientos,
            IZonaEventoRepository zonas,
            ILog log)
        {
            _asientos = asientos ?? throw new ArgumentNullException(nameof(asientos));
            _zonas = zonas ?? throw new ArgumentNullException(nameof(zonas));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<CrearAsientoResult> Handle(CrearAsientoCommand request, CancellationToken ct)
        {
            _log.Info($"Iniciando CrearAsientoCommand. EventId='{request.EventId}', ZonaEventoId='{request.ZonaEventoId}', Label='{request.Label}'.");

            try
            {
                // 1) Validar existencia de la zona y su relación con el evento
                _log.Debug($"Obteniendo zona. EventId='{request.EventId}', ZonaEventoId='{request.ZonaEventoId}'.");
                var zona = await _zonas.GetAsync(request.EventId, request.ZonaEventoId, ct);

                if (zona is null || zona.EventId != request.EventId)
                {
                    _log.Warn($"Creación de asiento cancelada. Zona no existe o no pertenece al evento. EventId='{request.EventId}', ZonaEventoId='{request.ZonaEventoId}'.");
                    throw new EventoException("La zona no existe o no pertenece al evento.");
                }

                // 2) Validar label
                if (string.IsNullOrWhiteSpace(request.Label))
                {
                    _log.Warn("Creación de asiento cancelada. Label es obligatorio.");
                    throw new ArgumentException("Label es obligatorio.", nameof(request.Label));
                }

                // 3) Evitar duplicados por (EventId, ZonaEventoId, Label)
                _log.Debug($"Verificando duplicado de asiento. EventId='{request.EventId}', ZonaEventoId='{request.ZonaEventoId}', Label='{request.Label}'.");
                var duplicado = await _asientos.GetByCompositeAsync(request.EventId, request.ZonaEventoId, request.Label, ct);

                if (duplicado is not null)
                {
                    _log.Warn($"Creación de asiento cancelada. Ya existe un asiento con ese label. EventId='{request.EventId}', ZonaEventoId='{request.ZonaEventoId}', Label='{request.Label}'.");
                    throw new EventoException("Ya existe un asiento con ese label en esta zona.");
                }

                // 4) Construir entidad asiento
                _log.Debug("Construyendo entidad Asiento en memoria.");
                var seat = new AsientoEntity
                {
                    EventId = request.EventId,
                    ZonaEventoId = request.ZonaEventoId,
                    FilaIndex = request.FilaIndex,
                    ColIndex = request.ColIndex,
                    Label = request.Label.Trim(),
                    Estado = string.IsNullOrWhiteSpace(request.Estado) ? "disponible" : request.Estado!.Trim(),
                    Meta = request.Meta,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 5) Persistir
                _log.Debug("Insertando asiento en repositorio.");
                await _asientos.InsertAsync(seat, ct);

                _log.Info($"Asiento creado correctamente. AsientoId='{seat.Id}', EventId='{seat.EventId}', ZonaEventoId='{seat.ZonaEventoId}'.");
                return new CrearAsientoResult(seat.Id);
            }
            catch (EventoException)
            {
                // Ya se logueó como Warn arriba
                throw;
            }
            catch (ArgumentException)
            {
                // Validación de argumentos, también ya logueada
                throw;
            }
            catch (Exception ex)
            {
                _log.Error("Error inesperado al ejecutar CrearAsientoCommand.", ex);
                throw new CrearAsientoHandlerException(ex);
            }
        }
    }
}
