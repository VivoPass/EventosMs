using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Excepciones.Infraestructura;
using log4net;

namespace EventsService.Aplicacion.Commands.CrearEvento
{
    public sealed class CreateEventHandler : IRequestHandler<CreateEventCommand, Guid>
    {
        private readonly IEventRepository _events;
        private readonly ICategoryRepository _cats;
        private readonly IScenarioRepository _scen;
        private readonly ILog _log;

        public CreateEventHandler(
            IEventRepository eventsRepo,
            ICategoryRepository cats,
            IScenarioRepository scen,
            ILog log)
        {
            _events = eventsRepo ?? throw new ArgumentNullException(nameof(eventsRepo));
            _cats = cats ?? throw new ArgumentNullException(nameof(cats));
            _scen = scen ?? throw new ArgumentNullException(nameof(scen));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<Guid> Handle(CreateEventCommand r, CancellationToken ct)
        {
            _log.Info($"Iniciando CreateEventCommand para Nombre='{r.Nombre}', CategoriaId='{r.CategoriaId}', EscenarioId='{r.EscenarioId}'.");

            try
            {
                // 1) Validar categoría
                _log.Debug($"Verificando existencia de categoría ID='{r.CategoriaId}'.");
                var categoriaExiste = await _cats.ExistsAsync(r.CategoriaId, ct);
                if (!categoriaExiste)
                {
                    _log.Warn($"Creación de evento cancelada. La categoría ID='{r.CategoriaId}' no existe.");
                    throw new EventoException("La categoría no existe.");
                }

                // 2) Validar escenario
                _log.Debug($"Verificando existencia de escenario ID='{r.EscenarioId}'.");
                var escenarioExiste = await _scen.ExistsAsync(r.EscenarioId, ct);
                if (!escenarioExiste)
                {
                    _log.Warn($"Creación de evento cancelada. El escenario ID='{r.EscenarioId}' no existe.");
                    throw new EventoException("El escenario no existe.");
                }

                if (r.AforoMaximo < 10) throw new EventoException("El aforo tiene que ser mayor que 10");

                // 3) Construir entidad de dominio
                _log.Debug("Construyendo entidad Evento en memoria.");
                var e = new Dominio.Entidades.Evento
                {
                    Id = Guid.NewGuid(),
                    Nombre = r.Nombre.Trim(),
                    CategoriaId = r.CategoriaId,
                    EscenarioId = r.EscenarioId,
                    Inicio = r.Inicio,
                    Fin = r.Fin,
                    AforoMaximo = r.AforoMaximo,
                    Tipo = r.Tipo,
                    Lugar = r.Lugar,
                    Descripcion = r.Descripcion,
                    OrganizadorId = r.OrganizadorId
                };

                _log.Debug($"Evento construido en memoria con ID='{e.Id}'. Persistiendo en repositorio.");
                await _events.InsertAsync(e, ct);

                _log.Info($"Evento creado y persistido exitosamente. ID='{e.Id}'.");
                return e.Id;
            }
            catch (EventoException)
            {
                
                throw;
            }
            catch (Exception ex)
            {
                _log.Error("Fallo crítico al ejecutar CreateEventCommand.", ex);
                throw new CreateEventHandlerException(ex);
            }
        }
    }
}
