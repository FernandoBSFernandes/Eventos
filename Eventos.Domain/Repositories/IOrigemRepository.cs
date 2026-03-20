using Eventos.Domain.Entities;

namespace Eventos.Domain.Repositories;

public interface IOrigemRepository
{
    Task<List<Convidado>> ObterConvidadosConfirmadosAsync();
}
