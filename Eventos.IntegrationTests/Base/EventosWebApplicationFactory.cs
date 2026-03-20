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

    private PostgreSqlContainer? _postgres;

    private string? ConnectionString =>
        _ciConnectionString ?? _postgres?.GetConnectionString();

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

        if (!IsDockerAvailable())
            throw new InvalidOperationException(
                "Docker não está disponível ou não está em execução. " +
                "Os testes de integração requerem Docker para subir o PostgreSQL via Testcontainers. " +
                "Inicie o Docker Desktop e execute os testes novamente.");

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await _postgres.StartAsync();
    }

    private static bool IsDockerAvailable()
    {
        try
        {
            new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .Build();
            return true;
        }
        catch (ArgumentException)
        {
            return false;
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
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<EventosDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<EventosDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            var origemDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<OrigemDbContext>));

            if (origemDescriptor != null)
                services.Remove(origemDescriptor);

            services.AddDbContext<OrigemDbContext>(options =>
                options.UseInMemoryDatabase("OrigemTestDb"));

            foreach (var configure in _serviceOverrides)
                configure(services);
        });

        builder.UseEnvironment("Development");
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventosDbContext>();

        // Garante que o schema existe antes de limpar
        await db.Database.MigrateAsync();

        // Limpa os dados sem dropar o banco, evitando conflito de conexões abertas
        await db.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"Acompanhante\", \"Convidado\" RESTART IDENTITY CASCADE");
    }
}
