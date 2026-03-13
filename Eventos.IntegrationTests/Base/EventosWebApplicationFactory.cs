using Eventos.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Eventos.IntegrationTests.Base;

public class EventosWebApplicationFactory : WebApplicationFactory<EventosAPI.Program>, IAsyncLifetime
{
    // Quando rodando no CI, a connection string vem da variável de ambiente.
    // Localmente, sobe um container PostgreSQL via Testcontainers.
    private readonly string? _ciConnectionString =
        Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

    private readonly PostgreSqlContainer? _postgres;

    public EventosWebApplicationFactory()
    {
        if (string.IsNullOrWhiteSpace(_ciConnectionString))
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .Build();
    }

    private string ConnectionString =>
        _ciConnectionString ?? _postgres!.GetConnectionString();

    public async Task InitializeAsync()
    {
        if (_postgres is not null)
            await _postgres.StartAsync();
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
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<EventosDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<EventosDbContext>(options =>
                options.UseNpgsql(ConnectionString));
        });

        builder.UseEnvironment("Development");
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventosDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }
}
