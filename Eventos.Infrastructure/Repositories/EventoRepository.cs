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
        await _context.Convidado.AddAsync(convidado);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Convidado>> ObterTodosConvidadosAsync()
    {
        return await _context.Convidado.ToListAsync();
    }

    public async Task<bool> ConvidadoExisteAsync(string nome)
    {
        return await _context.Convidado
            .AnyAsync(c => c.Nome.ToLower() == nome.ToLower());
    }

}
