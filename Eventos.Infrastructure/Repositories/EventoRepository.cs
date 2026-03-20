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
        return await _context.Convidado
            .AsNoTracking()
            .AnyAsync(c => EF.Functions.ILike(c.Nome, $"%{nome}%"));
    }

    public async Task<bool> AcompanhanteExisteAsync(string nome)
    {
        return await _context.Set<Acompanhante>()
            .AsNoTracking()
            .AnyAsync(a => EF.Functions.ILike(a.Nome, $"%{nome}%"));
    }

    public async Task ZerarTabelasAsync()
    {
        await _context.Convidado.ExecuteDeleteAsync();
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

    public async Task<(int convidadosRemovidos, int acompanhantesRemovidos)> RemoverDuplicatasAsync()
    {
        var todosConvidados = await _context.Convidado
            .Include(c => c.Acompanhantes)
            .ToListAsync();

        var duplicatasConvidados = todosConvidados
            .GroupBy(c => c.Nome.Trim().ToLowerInvariant())
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Skip(1))
            .ToList();

        var acompanhantesRemovidos = 0;
        var convidadosManutidos = todosConvidados.Except(duplicatasConvidados);

        var duplicatasAcomp = convidadosManutidos
            .SelectMany(c => c.Acompanhantes
                .GroupBy(a => a.Nome.Trim().ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Skip(1)))
            .ToList();

        if (duplicatasAcomp.Count > 0)
        {
            _context.Set<Acompanhante>().RemoveRange(duplicatasAcomp);
            acompanhantesRemovidos = duplicatasAcomp.Count;
        }

        if (duplicatasConvidados.Count > 0)
            _context.Convidado.RemoveRange(duplicatasConvidados);

        if (acompanhantesRemovidos > 0 || duplicatasConvidados.Count > 0)
            await _context.SaveChangesAsync();

        return (duplicatasConvidados.Count, acompanhantesRemovidos);
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
        return await _context.Convidado
            .Include(c => c.Acompanhantes)
            .Where(c => EF.Functions.ILike(c.Nome, $"%{nome}%"))
            .ToListAsync();
    }

    public async Task RemoverConvidadoAsync(Convidado convidado)
    {
        _context.Convidado.Remove(convidado);
        await _context.SaveChangesAsync();
    }
}
