using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.Commands.Zonas.EliminarZonaEvento
{
    public class EliminarZonaEventoHandler : IRequestHandler<EliminarZonaEventoCommand, bool>
    {
        private readonly IZonaEventoRepository _zonaRepo;
        private readonly IAsientoRepository _asientoRepo;
        private readonly IEscenarioZonaRepository _ezRepo;

        public EliminarZonaEventoHandler(
            IZonaEventoRepository zonaRepo,
            IAsientoRepository asientoRepo,
            IEscenarioZonaRepository ezRepo)
        {
            _zonaRepo = zonaRepo;
            _asientoRepo = asientoRepo;
            _ezRepo = ezRepo;
        }

        public async Task<bool> Handle(EliminarZonaEventoCommand cmd, CancellationToken ct)
        {
            await _asientoRepo.DeleteByZonaAsync(cmd.EventId, cmd.ZonaId, ct);
            await _ezRepo.DeleteByZonaAsync(cmd.EventId, cmd.ZonaId, ct);
            return await _zonaRepo.DeleteAsync(cmd.EventId, cmd.ZonaId, ct);
        }
    }
}
