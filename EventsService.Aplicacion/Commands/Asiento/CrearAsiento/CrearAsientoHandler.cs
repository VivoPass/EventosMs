using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Dominio.Interfaces;
using AsientoEntity = EventsService.Dominio.Entidades.Asiento;

using MediatR;
using EventsService.Dominio.Excepciones;

namespace EventsService.Aplicacion.Commands.Asiento.CrearAsiento
{
    public  class CrearAsientoHandler : IRequestHandler<CrearAsientoCommand, CrearAsientoResult>
    {
        private readonly IAsientoRepository _asientos;
        private readonly IZonaEventoRepository _zonas;

        public CrearAsientoHandler(IAsientoRepository asientos, IZonaEventoRepository zonas)
        {
            _asientos = asientos;
            _zonas = zonas;
        }

        public async Task<CrearAsientoResult> Handle(CrearAsientoCommand request, CancellationToken ct)
        {
            var zona = await _zonas.GetAsync(request.EventId, request.ZonaEventoId, ct);

            if (zona is null || zona.EventId != request.EventId)
            {
                throw new EventoException(" La Zona No existe o no pertence al evento");
            }

            if (string.IsNullOrWhiteSpace(request.Label))
                throw new ArgumentException("Label es obligatorio.", nameof(request.Label));



            // 3) Evitar duplicados por (EventId, ZonaEventoId, Label)
            var duplicado =
                await _asientos.GetByCompositeAsync(request.EventId, request.ZonaEventoId, request.Label, ct);
            if (duplicado is not null)
                throw new EventoException("Ya existe un asiento con ese label en esta zona.");

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

            await _asientos.InsertAsync(seat, ct);

            return new CrearAsientoResult(seat.Id);
        }
    }
}