using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Entidades;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Dominio.Interfaces;
using MediatR;
using log4net;

namespace EventsService.Aplicacion.Commands.CrearEscenario
{
    public class CreateEscenarioHandler : IRequestHandler<CreateEscenarioCommand, string>
    {
        private readonly IScenarioRepository _repo;
        private readonly ILog _log;

        public CreateEscenarioHandler(IScenarioRepository repo, ILog log)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<string> Handle(CreateEscenarioCommand r, CancellationToken ct)
        {
            _log.Info($"Iniciando CreateEscenarioCommand para Nombre='{r.Nombre}', Ciudad='{r.Ciudad}', Pais='{r.Pais}'.");

            try
            {
                _log.Debug("Construyendo entidad Escenario en memoria.");

                var escenario = new Escenario
                {
                    Id = Guid.NewGuid(),
                    Nombre = r.Nombre.Trim(),
                    Descripcion = r.Descripcion,
                    Ubicacion = r.Ubicacion,
                    Ciudad = r.Ciudad,
                    Estado = r.Estado,
                    Pais = r.Pais
                };

                _log.Debug($"Escenario construido con ID='{escenario.Id}'. Llamando a IScenarioRepository.CrearAsync.");
                var id = await _repo.CrearAsync(escenario, ct);

                _log.Info($"Escenario creado y persistido correctamente. ID='{id}'.");
                return id;
            }
            catch (Exception ex)
            {
                _log.Error("Error inesperado al ejecutar CreateEscenarioCommand.", ex);
                throw new CreateEscenarioHandlerException(ex);
            }
        }
    }
}
