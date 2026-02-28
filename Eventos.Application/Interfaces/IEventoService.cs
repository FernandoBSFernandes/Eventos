using Eventos.Domain.Entities;

namespace Eventos.Application.Interfaces;

public interface IEventoService
{
    Task<Evento?> GetByIdAsync(Guid id);
}