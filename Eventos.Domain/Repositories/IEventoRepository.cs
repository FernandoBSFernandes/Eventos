using Eventos.Domain.Entities;

namespace Eventos.Domain.Repositories;

public interface IEventoRepository
{
    Task<Evento?> GetByIdAsync(Guid id);
}