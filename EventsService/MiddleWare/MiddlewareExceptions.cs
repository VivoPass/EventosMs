
using System.Text.Json;
using EventsService.Dominio.Excepciones;

public class MiddlewareExceptions
{
    private readonly RequestDelegate _next;
    public MiddlewareExceptions(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (NotFoundException ex)
        {
            await Write(ctx, 404, "Not Found", ex.Message);
        }
        catch (EventoException ex) 
        {
            await Write(ctx, 422, "Unprocessable Entity", ex.Message);
        }
        catch (Exception)
        {
            await Write(ctx, 500, "Internal Server Error", "Unexpected error.");
        }
    }

    static async Task Write(HttpContext ctx, int status, string title, string detail)
    {
        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = status;
        var payload = new { type = $"https://httpstatuses.com/{status}", title, status, detail, traceId = ctx.TraceIdentifier };
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
