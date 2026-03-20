using Eventos.Domain.Entities;
using Eventos.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Eventos.IntegrationTests.Base;

public static class OrigemDbHelper
{
    public static async Task PopularAsync(EventosWebApplicationFactory factory, IEnumerable<Convidado> convidados)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrigemDbContext>();
        db.Convidado.AddRange(convidados);
        await db.SaveChangesAsync();
    }

    public static async Task LimparAsync(EventosWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrigemDbContext>();
        db.Convidado.RemoveRange(db.Convidado);
        await db.SaveChangesAsync();
    }
}
