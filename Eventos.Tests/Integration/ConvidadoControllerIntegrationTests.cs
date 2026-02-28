using Eventos.Application.DTOs.Request;
using Eventos.Application.DTOs.Response;
using EventosAPI;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Eventos.Tests.Integration;

/// <summary>
/// Testes de integração da API Convidado
/// Levanta a API automaticamente usando WebApplicationFactory
/// </summary>
public class ConvidadoControllerIntegrationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private const string ApiRoute = "api/convidado";

    public async Task InitializeAsync()
    {
        // Levanta a API automaticamente
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Desliga a API após os testes
        _client?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    #region Testes de Endpoint Existência

    [Fact]
    public async Task Post_AdicionarConvidado_DeveEstarDisponivel()
    {
        // Act
        var response = await _client.PostAsync($"{ApiRoute}/adicionar", null);

        // Assert
        Assert.NotNull(response);
        // Pode retornar 400 Bad Request, mas não 404 Not Found
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_DeveEstarDisponivel()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerUI_DeveEstarDisponivel()
    {
        // Act
        var response = await _client.GetAsync("/swagger");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect);
    }

    #endregion

    #region Testes de Resposta

    [Fact]
    public async Task Post_AdicionarConvidado_DeveRetornar201_QuandoDadosValidos()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"{ApiRoute}/adicionar", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BaseResponse>(responseContent, 
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        Assert.NotNull(result);
        Assert.True(result.CodigoStatus == 201, $"Status code esperado: 201, recebido: {result.CodigoStatus}");
        Assert.Contains("sucesso", result.Mensagem.ToLower());
    }

    [Fact]
    public async Task Post_AdicionarConvidado_DeveRetornar400_QuandoDadosInvalidos()
    {
        // Arrange
        var json = @"{ ""nome"": """" }";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"{ApiRoute}/adicionar", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_AdicionarConvidado_DeveRetornar201_ComAcompanhantes()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Maria Santos",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 2,
            nomesAcompanhantes: new List<string> { "Ana Costa", "Pedro Costa" }
        );

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"{ApiRoute}/adicionar", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    #endregion

    #region Testes de Content-Type

    [Fact]
    public async Task SwaggerJson_DeveRetornarContentTypeJson()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Contains("application/json", response.Content.Headers.ContentType?.ToString() ?? "");
    }

    [Fact]
    public async Task SwaggerJson_DeveSerJson()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        var document = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.Equal(JsonValueKind.Object, document.ValueKind);
    }

    #endregion
}
