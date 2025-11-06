using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones;

namespace EventsService.Aplicacion.Commands.Zonas.ModificarZonaEvento
{

    public class ModificarZonaEventoHandler : IRequestHandler<ModificarZonaEventoCommnand, bool>
    {
        private readonly IZonaEventoRepository _zonaRepo;
        private readonly IEscenarioZonaRepository _escenarioZonaRepo;

        public ModificarZonaEventoHandler(
            IZonaEventoRepository zonaRepo,
            IEscenarioZonaRepository escenarioZonaRepo)
        {
            _zonaRepo = zonaRepo;
            _escenarioZonaRepo = escenarioZonaRepo;
        }

        public async Task<bool> Handle(ModificarZonaEventoCommnand cmd, CancellationToken ct)
        {
            var zona = await _zonaRepo.GetAsync(cmd.EventId, cmd.ZonaId, ct);
            if (zona == null)
                throw new NotFoundException("ZonaEvento", cmd.ZonaId);

            // Actualizar campos básicos
            if (!string.IsNullOrWhiteSpace(cmd.Nombre))
                zona.Nombre = cmd.Nombre;

            if (cmd.Precio.HasValue)
                zona.Precio = cmd.Precio;

            if (!string.IsNullOrWhiteSpace(cmd.Estado))
                zona.Estado = cmd.Estado;

            zona.UpdatedAt = DateTime.UtcNow;
            await _zonaRepo.UpdateAsync(zona, ct);

            // Si hay grid, buscar el EscenarioZona vinculado y actualizarlo
            if (cmd.Grid != null)
            {
                var escenarioZona = await _escenarioZonaRepo.GetByZonaAsync(cmd.EventId, cmd.ZonaId, ct);
                if (escenarioZona != null)
                {
                    await _escenarioZonaRepo.UpdateGridAsync(
                        escenarioZona.Id,
                        cmd.Grid,
                        color: null,
                        zIndex: null,
                        visible: true,
                        ct
                    );
                }
            }

            return true;
        }
    }
}