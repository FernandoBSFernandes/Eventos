using Eventos.Application.DTOs.Request;
using Eventos.Application.DTOs.Response;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Eventos.Tests.Integration;

/// <summary>
/// Testes de integra��o da API Convidado
/// Testa os endpoints reais com toda a stack de DI
/// </summary>
public class ConvidadoControllerIntegrationTests
{
    private readonly HttpClient _client;
    private const string BaseUrl = "http://localhost:5000";
    private const string ApiRoute = "api/convidado";

    public ConvidadoControllerIntegrationTests()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    #region Testes de Endpoint Exist�ncia

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task Post_AdicionarConvidado_DeveEstarDisponivel()
    {
        // Act
        var response = await _client.PostAsync($"{ApiRoute}/adicionar", null);

        // Assert
        Assert.NotNull(response);
        // Pode retornar 400 Bad Request, mas n�o 404 Not Found
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task Swagger_DeveEstarDisponivel()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerUI_DeveEstarDisponivel()
    {
        // Act
        var response = await _client.GetAsync("/swagger");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect);
    }

    #endregion

    #region Testes de Resposta (quando API estiver rodando)

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task Post_AdicionarConvidado_DeveRetornar201_QuandoDadosValidos()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Jo�o Silva",
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
    }

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
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

    #endregion

    #region Testes de Content-Type

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerJson_DeveRetornarContentTypeJson()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Contains("application/json", response.Content.Headers.ContentType?.ToString() ?? "");
    }

    #endregion
}
