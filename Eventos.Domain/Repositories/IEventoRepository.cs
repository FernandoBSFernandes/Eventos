using Eventos.Domain.Entities;

namespace Eventos.Domain.Repositories;

public interface IEventoRepository
{
    Task AdicionarConvidadoAsync(Convidado convidado);
    Task<List<Convidado>> ObterTodosConvidadosAsync();
}
