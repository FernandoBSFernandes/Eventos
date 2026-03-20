using Eventos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Infrastructure.Data;

public class OrigemDbContext : DbContext
{
    public OrigemDbContext(DbContextOptions<OrigemDbContext> options) : base(options) { }

    public DbSet<Convidado> Convidado { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventosDbContext).Assembly);
    }
}
