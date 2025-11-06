namespace EventsService.Api.DTOs;

public record CreateEventRequest(
    string Nombre,
    Guid CategoriaId,
    Guid EscenarioId,
    DateTimeOffset Inicio,
    DateTimeOffset Fin,
    int AforoMaximo,
    string? Tipo,
    string? Lugar,
    string? Descripcion
);