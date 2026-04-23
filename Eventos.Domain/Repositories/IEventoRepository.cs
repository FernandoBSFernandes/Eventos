using Eventos.Domain.Entities;

namespace Eventos.Domain.Repositories;

public interface IEventoRepository
{
    Task AdicionarConvidadoAsync(Convidado convidado);
    Task<List<Convidado>> ObterTodosConvidadosAsync();
    Task<bool> ConvidadoExisteAsync(string nome);
    Task<bool> AcompanhanteExisteAsync(string nome);
    Task<List<Convidado>> ObterConvidadosConfirmadosAsync();
    Task<int> ObterTotalPessoasAsync();
    Task<List<Convidado>> BuscarConvidadosPorNomeAsync(string nome);
    Task RemoverConvidadoAsync(Convidado convidado);
}
