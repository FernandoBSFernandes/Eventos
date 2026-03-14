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

    // Criado apenas no InitializeAsync, evitando validação do Docker no construtor
    private PostgreSqlContainer? _postgres;

    private string ConnectionString =>
        _ciConnectionString ?? _postgres!.GetConnectionString();

    // Ações de configuração de serviços extras registradas pelos testes
    private readonly List<Action<IServiceCollection>> _serviceOverrides = new();

    public void AddServiceOverride(Action<IServiceCollection> configure)
        => _serviceOverrides.Add(configure);

    public void ClearServiceOverrides()
        => _serviceOverrides.Clear();

    public async Task InitializeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_ciConnectionString))
            return;

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

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

            foreach (var configure in _serviceOverrides)
                configure(services);
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
