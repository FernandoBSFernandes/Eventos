using Eventos.Domain.Entities;
using Eventos.Domain.Repositories;
using Eventos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Infrastructure.Repositories;

public class OrigemRepository : IOrigemRepository
{
    private readonly OrigemDbContext _context;

    public OrigemRepository(OrigemDbContext context)
    {
        _context = context;
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
}
