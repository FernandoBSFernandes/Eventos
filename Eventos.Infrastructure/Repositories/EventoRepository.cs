using Eventos.Domain.Entities;
using Eventos.Domain.Repositories;

namespace Eventos.Infrastructure.Repositories;

public class EventoRepository : IEventoRepository
{
    // In-memory sample implementation
    private static readonly List<Evento> _store = new()
    {
        new Evento(Guid.NewGuid(), "Conferência .NET", DateTime.UtcNow.AddDays(10))
    };

    public Task<Evento?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_store.FirstOrDefault(e => e.Id == id));
    }
}