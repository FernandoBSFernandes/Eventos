namespace Eventos.Application.DTOs.Response;

public class EventoResponse
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateTime Data { get; set; }
}
