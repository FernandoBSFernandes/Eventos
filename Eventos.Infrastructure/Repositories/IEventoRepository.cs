using Eventos.Domain.Entities;

namespace Eventos.Infrastructure.Repositories;

public interface IEventoRepository
{
    Task<Evento?> GetByIdAsync(Guid id);
}