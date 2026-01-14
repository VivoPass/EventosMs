namespace EventsService.Api.DTOs
{
    public record UpdateEventRequest(
        string? Nombre,
        Guid? CategoriaId,
        Guid? EscenarioId,
        DateTimeOffset? Inicio,
        DateTimeOffset? Fin,
        int? AforoMaximo,
        string? Tipo,
        string? Lugar,
        string? Descripcion,
        string? OnlineMeetingUrl
    );
}
