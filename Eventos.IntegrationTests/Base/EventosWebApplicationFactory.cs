using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Eventos.Infrastructure.Data;
using Eventos.Domain.Entities;

namespace Eventos.IntegrationTests.Base;

public class EventosWebApplicationFactory : WebApplicationFactory<EventosAPI.Program>, IAsyncLifetime
{
    private readonly string? _ciConnectionString =
        Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

    private PostgreSqlContainer? _postgres;

    // Definido em InitializeAsync antes de qualquer chamada a CreateClient()
    private string? _resolvedConnectionString;

    private bool UseInMemory => string.IsNullOrWhiteSpace(_resolvedConnectionString);

    public async Task InitializeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_ciConnectionString))
        {
            _resolvedConnectionString = _ciConnectionString;
            return;
        }

        try
        {
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .Build();

            await _postgres.StartAsync();
            _resolvedConnectionString = _postgres.GetConnectionString();
        }
        catch
        {
            // Falha ao iniciar Docker/Testcontainers â fallback para InMemory
            _resolvedConnectionString = null;
        }
    }

    public new async Task DisposeAsync()
    {
        if (_postgres is not null)
            await _postgres.StopAsync();

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove os DbContextOptions registrados pelo Program.cs para trocar o provider
            services.RemoveAll<DbContextOptions<EventosDbContext>>();
            services.RemoveAll<DbContextOptions<OrigemDbContext>>();

            services.AddDbContext<EventosDbContext>(options =>
            {
                if (UseInMemory)
                    options.UseInMemoryDatabase("EventosTestsDb");
                else
                    options.UseNpgsql(_resolvedConnectionString);
            });

            services.AddDbContext<OrigemDbContext>(options =>
            {
                if (UseInMemory)
                    options.UseInMemoryDatabase("OrigemTestsDb");
                else
                    options.UseNpgsql(_resolvedConnectionString);
            });
        });

        builder.UseEnvironment("Development");
    }

    public async Task ApplyMigrationsAsync()
    {
        if (UseInMemory) return;

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventosDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventosDbContext>();

        if (!UseInMemory)
        {
            await db.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"Acompanhante\", \"Convidado\" RESTART IDENTITY CASCADE");
        }
        else
        {
            db.Set<Acompanhante>().RemoveRange(db.Set<Acompanhante>());
            db.Set<Convidado>().RemoveRange(db.Set<Convidado>());
            await db.SaveChangesAsync();
        }
    }
}
