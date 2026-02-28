namespace Eventos.Application.DTOs.Request;

public class CriarEventoRequest
{
    public string Titulo { get; set; } = string.Empty;
    public DateTime Data { get; set; }
}
