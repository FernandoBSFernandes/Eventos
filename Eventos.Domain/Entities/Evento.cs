namespace Eventos.Domain.Entities;

public class Evento
{
    public Guid Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public DateTime Data { get; private set; }

    public Evento(Guid id, string titulo, DateTime data)
    {
        Id = id;
        Titulo = titulo;
        Data = data;
    }
}