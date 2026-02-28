using Eventos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Infrastructure.Data;

public class EventosDbContext : DbContext
{
    public EventosDbContext(DbContextOptions<EventosDbContext> options) : base(options) { }

    public DbSet<Convidado> Convidado { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Aplicar configura��es de entidades
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventosDbContext).Assembly);
    }
}
