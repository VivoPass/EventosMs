using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Dominio.Interfaces;
using MediatR;
using log4net;

namespace EventsService.Aplicacion.Commands.ModificarEvento
{
    public sealed class UpdateEventHandler : IRequestHandler<UpdateEventCommand, bool>
    {
        private readonly IEventRepository _events;
        private readonly ICategoryRepository _cats;
        private readonly IScenarioRepository _scens;
        private readonly ILog _log;

        public UpdateEventHandler(
            IEventRepository events,
            ICategoryRepository cats,
            IScenarioRepository scens,
            ILog log)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _cats = cats ?? throw new ArgumentNullException(nameof(cats));
            _scens = scens ?? throw new ArgumentNullException(nameof(scens));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<bool> Handle(UpdateEventCommand r, CancellationToken ct)
        {
            _log.Info($"Iniciando UpdateEventCommand para ID='{r.Id}'.");

            try
            {
                // 1) Obtener evento actual
                var current = await _events.GetByIdAsync(r.Id, ct);
                if (current is null)
                {
                    _log.Warn($"No se encontró evento con ID='{r.Id}'. Se retornará false (404).");
                    return false; // lo manejará el controller como 404
                }

                // 2) Validaciones referenciales si vienen cambios
                if (r.CategoriaId.HasValue)
                {
                    _log.Debug($"Verificando existencia de categoría ID='{r.CategoriaId.Value}' para actualización de evento ID='{r.Id}'.");
                    var ok = await _cats.ExistsAsync(r.CategoriaId.Value, ct);
                    if (!ok)
                    {
                        _log.Warn($"Actualización cancelada. La categoría ID='{r.CategoriaId.Value}' no existe.");
                        throw new EventoException("La categoría no existe.");
                    }
                    current.CategoriaId = r.CategoriaId.Value;
                }

                if (r.EscenarioId.HasValue)
                {
                    _log.Debug($"Verificando existencia de escenario ID='{r.EscenarioId.Value}' para actualización de evento ID='{r.Id}'.");
                    var ok = await _scens.ExistsAsync(r.EscenarioId.Value, ct);
                    if (!ok)
                    {
                        _log.Warn($"Actualización cancelada. El escenario ID='{r.EscenarioId.Value}' no existe.");
                        throw new EventoException("El escenario no existe.");
                    }
                    current.EscenarioId = r.EscenarioId.Value;
                }

                // 3) Merge de campos opcionales
                _log.Debug($"Aplicando cambios parciales al evento ID='{r.Id}'.");

                if (r.Nombre is not null) current.Nombre = r.Nombre.Trim();
                if (r.Inicio.HasValue) current.Inicio = r.Inicio.Value;
                if (r.Fin.HasValue) current.Fin = r.Fin.Value;
                if (r.AforoMaximo.HasValue) current.AforoMaximo = r.AforoMaximo.Value;
                if (r.Tipo is not null) current.Tipo = r.Tipo;
                if (r.Lugar is not null) current.Lugar = r.Lugar;
                if (r.Descripcion is not null) current.Descripcion = r.Descripcion;
                if (r.OnlineMeetingUrl is not null)
                {
                    current.AsignarOnlineMeetingUrl(r.OnlineMeetingUrl);
                }

                // 4) Persistir cambios
                var actualizado = await _events.UpdateAsync(current, ct);

                if (actualizado)
                {
                    _log.Info($"Evento actualizado correctamente. ID='{r.Id}'.");
                }
                else
                {
                    _log.Warn($"UpdateAsync retornó false para evento ID='{r.Id}'. No se realizaron modificaciones.");
                }

                return actualizado;
            }
            catch (EventoException)
            {
                // Ya logueamos el contexto con Warn arriba.
                throw;
            }
            catch (Exception ex)
            {
                _log.Error($"Error inesperado al actualizar evento ID='{r.Id}'.", ex);
                throw new UpdateEventHandlerException(ex);
            }
        }
    }
}
