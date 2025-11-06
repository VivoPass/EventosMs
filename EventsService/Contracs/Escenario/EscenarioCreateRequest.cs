namespace EventsService.Api.Contracs.Escenario
{
    public record EscenarioCreateRequest(
        string Nombre,
        string? Descripcion,
        string? Ubicacion,
        string? Ciudad,
        string? Estado,
        string? Pais
    );
}
