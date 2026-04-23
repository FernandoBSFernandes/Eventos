using Eventos.Domain.Entities;
using Eventos.Domain.Repositories;
using Eventos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Infrastructure.Repositories;

public class EventoRepository : IEventoRepository
{

    private readonly EventosDbContext _context;

    public EventoRepository(EventosDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarConvidadoAsync(Convidado convidado)
    {
        _context.Convidado.Add(convidado);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Convidado>> ObterTodosConvidadosAsync()
    {
        return await _context.Convidado
            .AsNoTracking()
            .Include(c => c.Acompanhantes)
            .ToListAsync();
    }

    public async Task<bool> ConvidadoExisteAsync(string nome)
    {
        var nomeBusca = nome.Trim().ToLower();

        return await _context.Convidado
            .AsNoTracking()
            .AnyAsync(c => c.Nome.ToLower().Contains(nomeBusca));
    }

    public async Task<bool> AcompanhanteExisteAsync(string nome)
    {
        var nomeBusca = nome.Trim().ToLower();

        return await _context.Set<Acompanhante>()
            .AsNoTracking()
            .AnyAsync(a => a.Nome.ToLower().Contains(nomeBusca));
    }

    public async Task<List<Convidado>> ObterConvidadosConfirmadosAsync()
    {
        return await _context.Convidado
            .AsNoTracking()
            .Include(c => c.Acompanhantes)
            .Where(c => c.PresencaConfirmada)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<int> ObterTotalPessoasAsync()
    {
        return await _context.Convidado
            .AsNoTracking()
            .Where(c => c.PresencaConfirmada)
            .Select(c => 1 + c.Acompanhantes.Count)
            .SumAsync();
    }

    public async Task<List<Convidado>> BuscarConvidadosPorNomeAsync(string nome)
    {
        var nomeBusca = nome.Trim().ToLower();

        return await _context.Convidado
            .Include(c => c.Acompanhantes)
            .Where(c => c.Nome.ToLower().Contains(nomeBusca))
            .ToListAsync();
    }

    public async Task RemoverConvidadoAsync(Convidado convidado)
    {
        _context.Convidado.Remove(convidado);
        await _context.SaveChangesAsync();
    }
}
