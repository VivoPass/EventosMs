using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace EventsService.Dominio.Entidades;

public class Evento
{
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid(); // se genera en C#
    public string Nombre { get; set; } = null!;

    [BsonRepresentation(BsonType.String)]
    public Guid CategoriaId { get; set; }


    [BsonRepresentation(BsonType.String)]
    public Guid EscenarioId { get; set; }
    public Guid OrganizadorId { get; set; }
    public DateTimeOffset Inicio { get; set; }
    public DateTimeOffset Fin { get; set; }
    public int AforoMaximo { get; set; }
    public string Estado { get; set; } = "Draft";
    public string? Tipo { get; set; }
    public string? Lugar { get; set; }
    public string? Descripcion { get; set; }

    public string? ImagenUrl { get; private set; }
    public string? FolletoUrl { get; private set; }

    public string? OnlineMeetingUrl { get; private set; }

    public void AsignarImagen(string url)
    {
        ImagenUrl = url;
    }

    public void AsignarFolleto(string url)
    {
        FolletoUrl = url;
    }


    public void AsignarOnlineMeetingUrl(string? url)
    {
        // Si el campo se envía vacío, permites borrarlo
        if (string.IsNullOrWhiteSpace(url))
        {
            OnlineMeetingUrl = null;
            return;
        }

        // Validación básica de URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
            throw new ArgumentException("La URL proporcionada no es válida.");

        // Validación adicional (opcional): solo HTTPS
        if (uriResult.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException("La URL debe usar HTTPS.");

        OnlineMeetingUrl = url;
    }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}