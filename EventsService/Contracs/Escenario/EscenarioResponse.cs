namespace EventsService.Api.Contracs.Escenario
{
    public record EscenarioResponse(
        Guid Id,
        string Nombre,
        string? Descripcion,
        string? Ubicacion,
        string? Ciudad,
        string? Estado,
        string? Pais,
        int CapacidadTotal,
        bool Activo
    );
}
