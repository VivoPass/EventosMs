using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Dominio.Interfaces;
using MediatR;
using log4net;

namespace EventsService.Aplicacion.Commands.ModificarEscenario
{
    public class ModificarEscenarioHandler : IRequestHandler<ModificarEscenarioCommand, Unit>
    {
        private readonly IScenarioRepository _repo;
        private readonly ILog _log;

        public ModificarEscenarioHandler(IScenarioRepository repo, ILog log)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<Unit> Handle(ModificarEscenarioCommand r, CancellationToken ct)
        {
            _log.Info($"Iniciando ModificarEscenarioCommand para ID='{r.Id}'.");

            try
            {
                var current = await _repo.ObtenerEscenario(r.Id, ct);

                if (current is null)
                {
                    _log.Warn($"Escenario no encontrado para ID='{r.Id}'. Lanzando EventoException.");
                    throw new EventoException("Escenario no encontrado");
                }

                _log.Debug($"Escenario actual encontrado. ID='{current.Id}', NombreActual='{current.Nombre}'.");

                var changes = new Escenario
                {
                    Id = current.Id,
                    Nombre = r.Nombre?.Trim() ?? current.Nombre,
                    Descripcion = r.Descripcion,
                    Ubicacion = r.Ubicacion,
                    Ciudad = r.Ciudad,
                    Estado = r.Estado,
                    Pais = r.Pais
                };

                _log.Debug($"Aplicando cambios al escenario ID='{r.Id}'. NombreNuevo='{changes.Nombre}'.");

                await _repo.ModificarEscenario(r.Id, changes, ct);

                _log.Info($"Escenario modificado correctamente. ID='{r.Id}'.");
                return Unit.Value;
            }
            catch (EventoException)
            {
                // Ya logueamos arriba antes de lanzar
                throw;
            }
            catch (Exception ex)
            {
                _log.Error($"Error inesperado al modificar escenario ID='{r.Id}'.", ex);
                throw new ModificarEscenarioHandlerException(ex);
            }
        }
    }
}
