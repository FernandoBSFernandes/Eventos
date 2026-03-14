using Eventos.Application.Interfaces;
using Eventos.IntegrationTests.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Eventos.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
[Trait("Integração", "RelatorioEmailController")]
public class RelatorioEmailIntegrationTests : IntegrationTestBase
{
    private readonly IRelatorioEmailService _mockEmailService;
    private readonly HttpClient _clientWithMock;

    public RelatorioEmailIntegrationTests(EventosWebApplicationFactory factory) : base(factory)
    {
        _mockEmailService = Substitute.For<IRelatorioEmailService>();

        _clientWithMock = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IRelatorioEmailService));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddScoped(_ => _mockEmailService);
            });
        }).CreateClient();
    }

    [Fact(DisplayName = "Deve retornar 200 quando endpoint enviar relatório for chamado")]
    [Trait("Categoria", "Integração")]
    public async Task DeveRetornar200_QuandoEndpointEnviarRelatorioForChamado()
    {
        // Arrange
        _mockEmailService.EnviarRelatorioAsync().Returns(Task.CompletedTask);

        // Act
        var response = await _clientWithMock.PostAsync("/api/relatorio/enviar", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "Deve retornar 200 quando não há convidados confirmados")]
    [Trait("Categoria", "Integração")]
    public async Task DeveRetornar200_QuandoNaoHaConvidadosConfirmados()
    {
        // Arrange — banco está vazio (resetado pelo InitializeAsync da classe base)
        _mockEmailService.EnviarRelatorioAsync().Returns(Task.CompletedTask);

        // Act
        var response = await _clientWithMock.PostAsync("/api/relatorio/enviar", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "Deve retornar Content-Type application/json quando enviar relatório")]
    [Trait("Categoria", "Integração")]
    public async Task DeveRetornarContentTypeJson_QuandoEnviarRelatorio()
    {
        // Arrange
        _mockEmailService.EnviarRelatorioAsync().Returns(Task.CompletedTask);

        // Act
        var response = await _clientWithMock.PostAsync("/api/relatorio/enviar", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Contains("application/json", response.Content.Headers.ContentType.MediaType);
    }
}
