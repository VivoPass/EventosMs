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

namespace EventsService.Aplicacion.Commands.Zonas.CrearZonaEvento
{
    public class CreateZonaEventoHandler : IRequestHandler<CreateZonaEventoCommand, Guid>
    {
        private readonly IZonaEventoRepository _zonaRepo;
        private readonly IEscenarioZonaRepository _escenarioZonaRepo;
        private readonly IAsientoRepository _asientoRepo;
        private readonly IScenarioRepository _escenarioRepo;
        private readonly ILog _log;

        public CreateZonaEventoHandler(
            IZonaEventoRepository zonaRepo,
            IEscenarioZonaRepository escenarioZonaRepo,
            IAsientoRepository asientoRepo,
            IScenarioRepository escenarioRepo,
            ILog log)
        {
            _zonaRepo = zonaRepo ?? throw new ArgumentNullException(nameof(zonaRepo));
            _escenarioZonaRepo = escenarioZonaRepo ?? throw new ArgumentNullException(nameof(escenarioZonaRepo));
            _asientoRepo = asientoRepo ?? throw new ArgumentNullException(nameof(asientoRepo));
            _escenarioRepo = escenarioRepo ?? throw new ArgumentNullException(nameof(escenarioRepo));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<Guid> Handle(CreateZonaEventoCommand request, CancellationToken ct)
        {
            _log.Info(
                $"Iniciando CreateZonaEventoCommand. EventId='{request.EventId}', EscenarioId='{request.EscenarioId}', Nombre='{request.Nombre}', Tipo='{request.Tipo}'.");

            try
            {
                // 1) Validaciones básicas
                _log.Debug($"Verificando existencia de escenario ID='{request.EscenarioId}'.");
                var escenario = await _escenarioRepo.ObtenerEscenario(request.EscenarioId.ToString(), ct);
                if (escenario is null)
                {
                    _log.Warn($"Creación de zona cancelada. Escenario ID='{request.EscenarioId}' no existe.");
                    throw new EventoException("El escenario no existe.");
                }

                _log.Debug(
                    $"Verificando duplicado de nombre de zona. EventId='{request.EventId}', Nombre='{request.Nombre}'.");
                var nombreDuplicado = await _zonaRepo.ExistsByNombreAsync(request.EventId, request.Nombre, ct);
                if (nombreDuplicado)
                {
                    _log.Warn(
                        $"Creación de zona cancelada. Ya existe zona con nombre='{request.Nombre}' para EventId='{request.EventId}'.");
                    throw new EventoException($"Ya existe una zona llamada '{request.Nombre}' en este evento.");
                }

                if (string.Equals(request.Tipo, "sentado", StringComparison.OrdinalIgnoreCase))
                {
                    var filas = request.Numeracion?.Filas ?? 0;
                    var cols = request.Numeracion?.Columnas ?? 0;

                    _log.Debug($"Validando numeración para zona SENTADA. Filas={filas}, Columnas={cols}, Capacidad={request.Capacidad}.");

                    if (filas <= 0 || cols <= 0)
                        throw new EventoException("Filas y Columnas deben ser > 0 para zonas sentadas.");

                    if (request.Capacidad != filas * cols)
                        throw new EventoException("Capacidad debe ser igual a filas × columnas.");
                }

                // 2) Crear ZonaEvento
                _log.Debug("Construyendo entidad ZonaEvento en memoria.");
                var zona = new ZonaEvento
                {
                    EventId = request.EventId,
                    EscenarioId = request.EscenarioId,
                    Nombre = request.Nombre.Trim(),
                    Tipo = request.Tipo,
                    Capacidad = request.Capacidad,
                    Numeracion = request.Numeracion,
                    Precio = request.Precio,
                    Estado = request.Estado,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _log.Debug("Persistiendo ZonaEvento en repositorio.");
                await _zonaRepo.AddAsync(zona, ct);

                // 3) Registrar bloque visual (EscenarioZona)
                _log.Debug("Creando bloque visual EscenarioZona asociado a la zona.");
                var ez = new EscenarioZona
                {
                    EventId = request.EventId,
                    EscenarioId = request.EscenarioId,
                    ZonaEventoId = zona.Id,
                    Grid = request.Grid,
                    Color = "#CCCCCC",
                    Visible = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _escenarioZonaRepo.AddAsync(ez, ct);

                // 4) Generar asientos si corresponde
                if (request.AutogenerarAsientos &&
                    string.Equals(request.Tipo, "sentado", StringComparison.OrdinalIgnoreCase))
                {
                    _log.Info(
                        $"Autogenerando asientos para ZonaEventoId='{zona.Id}', EventId='{request.EventId}'.");
                    var seats = zona.GenerarAsientos(request.EventId);   // método de la entidad
                    await _asientoRepo.BulkInsertAsync(seats, ct);
                    var totalAsientos = seats.Count();   
                    _log.Info($"Se generaron automáticamente {totalAsientos} asientos para la zona '{zona.Nombre}'.");

                }

                _log.Info($"ZonaEvento creada correctamente. ZonaEventoId='{zona.Id}', EventId='{zona.EventId}'.");
                return zona.Id;
            }
            catch (EventoException)
            {
                // Errores de dominio ya logueados como Warn.
                throw;
            }
            catch (Exception ex)
            {
                _log.Error("Error inesperado al ejecutar CreateZonaEventoCommand.", ex);
                throw new CreateZonaEventoHandlerException(ex);
            }
        }

    }
}
