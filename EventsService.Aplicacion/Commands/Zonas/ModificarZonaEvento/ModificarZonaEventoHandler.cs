using System;
using System.Threading;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Excepciones.Aplicacion;
using EventsService.Dominio.Excepciones.Infraestructura;
using EventsService.Dominio.Interfaces;
using MediatR;
using log4net;

namespace EventsService.Aplicacion.Commands.Zonas.ModificarZonaEvento
{
    public class ModificarZonaEventoHandler : IRequestHandler<ModificarZonaEventoCommnand, bool>
    {
        private readonly IZonaEventoRepository _zonaRepo;
        private readonly IEscenarioZonaRepository _escenarioZonaRepo;
        private readonly ILog _log;

        public ModificarZonaEventoHandler(
            IZonaEventoRepository zonaRepo,
            IEscenarioZonaRepository escenarioZonaRepo,
            ILog log)
        {
            _zonaRepo = zonaRepo ?? throw new ArgumentNullException(nameof(zonaRepo));
            _escenarioZonaRepo = escenarioZonaRepo ?? throw new ArgumentNullException(nameof(escenarioZonaRepo));
            _log = log ?? throw new LoggerNullException();
        }

        public async Task<bool> Handle(ModificarZonaEventoCommnand cmd, CancellationToken ct)
        {
            _log.Info($"Iniciando ModificarZonaEventoCommand. EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'.");

            try
            {
                // 1) Buscar zona
                _log.Debug($"Buscando ZonaEvento. EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'.");
                var zona = await _zonaRepo.GetAsync(cmd.EventId, cmd.ZonaId, ct);
                if (zona == null)
                {
                    _log.Warn($"ZonaEvento no encontrada. EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'. Lanzando NotFoundException.");
                    throw new NotFoundException("ZonaEvento", cmd.ZonaId);
                }

                // 2) Actualizar campos básicos
                _log.Debug("Aplicando cambios en campos básicos de ZonaEvento.");

                if (!string.IsNullOrWhiteSpace(cmd.Nombre))
                    zona.Nombre = cmd.Nombre.Trim();

                if (cmd.Precio.HasValue)
                    zona.Precio = cmd.Precio;

                if (!string.IsNullOrWhiteSpace(cmd.Estado))
                    zona.Estado = cmd.Estado!.Trim();

                zona.UpdatedAt = DateTime.UtcNow;

                _log.Debug($"Persistiendo cambios de ZonaEvento. ZonaId='{zona.Id}'.");
                await _zonaRepo.UpdateAsync(zona, ct);

                // 3) Si hay grid, actualizar el bloque visual (EscenarioZona)
                if (cmd.Grid != null)
                {
                    _log.Debug($"Actualizando grid de EscenarioZona asociado. EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'.");
                    var escenarioZona = await _escenarioZonaRepo.GetByZonaAsync(cmd.EventId, cmd.ZonaId, ct);

                    if (escenarioZona != null)
                    {
                        await _escenarioZonaRepo.UpdateGridAsync(
                            escenarioZona.Id,
                            cmd.Grid,
                            color: null,
                            zIndex: null,
                            visible: true,
                            ct: ct
                        );
                        _log.Info($"Grid de EscenarioZona actualizado. EscenarioZonaId='{escenarioZona.Id}'.");
                    }
                    else
                    {
                        _log.Warn($"No se encontró EscenarioZona para la ZonaEvento. EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'.");
                    }
                }

                _log.Info($"ZonaEvento modificada correctamente. ZonaId='{zona.Id}', EventId='{zona.EventId}'.");
                return true;
            }
            catch (NotFoundException)
            {
                // Ya lo logueamos como Warn arriba
                throw;
            }
            catch (Exception ex)
            {
                _log.Error($"Error inesperado al modificar ZonaEvento. EventId='{cmd.EventId}', ZonaId='{cmd.ZonaId}'.", ex);
                throw new ModificarZonaEventoHandlerException(ex);
            }
        }
    }
}
