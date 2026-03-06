using Eventos.Domain.Entities;

namespace Eventos.Domain.Repositories;

public interface IEventoRepository
{
    Task AdicionarConvidadoAsync(Convidado convidado);
    Task<List<Convidado>> ObterTodosConvidadosAsync();
    Task<bool> ConvidadoExisteAsync(string nome);
    Task ZerarTabelasAsync();
    Task<List<Convidado>> ObterConvidadosConfirmadosAsync();
    Task<(int convidadosRemovidos, int acompanhantesRemovidos)> RemoverDuplicatasAsync();
    Task<int> ObterTotalPessoasAsync();
    Task<List<Convidado>> BuscarConvidadosPorNomeAsync(string nome);
    Task RemoverConvidadoAsync(Convidado convidado);
}
