namespace Eventos.IntegrationTests.Base;

/// <summary>
/// Classe base que compartilha uma única instância do factory por coleção de testes,
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

    public async Task InitializeAsync() => await Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}
