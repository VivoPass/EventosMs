using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Dominio.Excepciones;

namespace EventsService.Aplicacion.Commands.Zonas.CrearZonaEvento
{
    public class CreateZonaEventoHandler : IRequestHandler<CreateZonaEventoCommand, Guid>
    {
        private readonly IZonaEventoRepository _zonaRepo;
        private readonly IEscenarioZonaRepository _escenarioZonaRepo;
        private readonly IAsientoRepository _asientoRepo;
        private readonly IScenarioRepository _escenarioRepo;

        public CreateZonaEventoHandler(
            IZonaEventoRepository zonaRepo,
            IEscenarioZonaRepository escenarioZonaRepo,
            IAsientoRepository asientoRepo,
            IScenarioRepository escenarioRepo)
        {
            _zonaRepo = zonaRepo;
            _escenarioZonaRepo = escenarioZonaRepo;
            _asientoRepo = asientoRepo;
            _escenarioRepo = escenarioRepo;
        }

        public async Task<Guid> Handle(CreateZonaEventoCommand request, CancellationToken ct)
        {
            // 1) Validaciones básicas
            var escenario = await _escenarioRepo.ObtenerEscenario(request.EscenarioId.ToString(), ct);
            if (escenario is null)
                throw new EventoException("El escenario no existe.");

            var nombreDuplicado = await _zonaRepo.ExistsByNombreAsync(request.EventId, request.Nombre, ct);
            if (nombreDuplicado)
                throw new EventoException($"Ya existe una zona llamada '{request.Nombre}' en este evento.");

            if (string.Equals(request.Tipo, "sentado", StringComparison.OrdinalIgnoreCase))
            {
                var filas = request.Numeracion?.Filas ?? 0;
                var cols = request.Numeracion?.Columnas ?? 0;
                if (filas <= 0 || cols <= 0)
                    throw new EventoException("Filas y Columnas deben ser > 0 para zonas sentadas.");

                if (request.Capacidad != filas * cols)
                    throw new EventoException("Capacidad debe ser igual a filas × columnas.");
            }

            // 2) Crear ZonaEvento
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
            await _zonaRepo.AddAsync(zona, ct);

            // 3) Registrar bloque visual (EscenarioZona)
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

            // 4) Generar asientos si corresponde (usa el método de la entidad)
            if (request.AutogenerarAsientos && string.Equals(request.Tipo, "sentado", StringComparison.OrdinalIgnoreCase))
            {
                var seats = zona.GenerarAsientos(request.EventId);   // ← método de tu entidad
                await _asientoRepo.BulkInsertAsync(seats, ct);
            }

            return zona.Id;
        }
    }
}
