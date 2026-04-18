namespace Eventos.IntegrationTests.Base;

/// <summary>
/// Classe base que compartilha uma única instÃƒÂ¢ncia do factory por coleção de testes,
/// evitando a inicialização do container PostgreSQL a cada classe de teste.
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<EventosWebApplicationFactory>
{
    public const string Name = "Integration Tests";
}

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly EventosWebApplicationFactory Factory;

    protected IntegrationTestBase(EventosWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public virtual async ValueTask InitializeAsync()
    {
        // Migrations aplicadas aqui Ã¢Â€Â” após CreateClient() ter construído o host,
        // garantindo que _resolvedConnectionString já foi definido em InitializeAsync da factory
        await Factory.ApplyMigrationsAsync();
        await Factory.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
