using System.Text.Json;
using Xunit;

namespace Eventos.Tests.Integration;

/// <summary>
/// Testes para validar a especifica��o OpenAPI/Swagger
/// </summary>
public class OpenApiSpecificationTests
{
    private readonly HttpClient _client;
    private const string BaseUrl = "http://localhost:5000";

    public OpenApiSpecificationTests()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    #region Testes de Especifica��o OpenAPI

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerJson_DeveEstarDisponivel()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerJson_DeveRetornarJsonValido()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Tentar deserializar como JSON
        var document = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.Equal(JsonValueKind.Object, document.ValueKind);
    }

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerJson_DeveConterInformacoes()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        Assert.True(document.TryGetProperty("info", out var info));
        Assert.True(info.TryGetProperty("title", out _));
        Assert.True(info.TryGetProperty("version", out _));
    }

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerJson_DeveConterPaths()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        Assert.True(document.TryGetProperty("paths", out var paths));
        Assert.NotEqual(JsonValueKind.Null, paths.ValueKind);
    }

    #endregion

    #region Testes de Endpoint no Swagger

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerJson_DeveConterEndpointAdicionarConvidado()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonElement>(content);

        document.TryGetProperty("paths", out var paths);

        // Assert - Procurar por endpoint de convidado
        bool hasEndpoint = false;
        foreach (var path in paths.EnumerateObject())
        {
            if (path.Name.Contains("convidado", StringComparison.OrdinalIgnoreCase))
            {
                hasEndpoint = true;
                break;
            }
        }

        Assert.True(hasEndpoint, "Endpoint de convidado n�o encontrado no Swagger");
    }

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerJson_EndpointAdicionarConvidado_DeveTermoMethodoPost()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonElement>(content);

        document.TryGetProperty("paths", out var paths);

        var endpoint = paths.EnumerateObject()
            .FirstOrDefault(p => p.Name.Contains("convidado", StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.True(endpoint.Value.TryGetProperty("post", out _), "POST n�o encontrado no endpoint");
    }

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerJson_EndpointAdicionarConvidado_DeveTermoDescricao()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonElement>(content);

        document.TryGetProperty("paths", out var paths);

        var endpoint = paths.EnumerateObject()
            .FirstOrDefault(p => p.Name.Contains("convidado", StringComparison.OrdinalIgnoreCase));

        endpoint.Value.TryGetProperty("post", out var post);

        // Assert
        bool hasDesc = post.TryGetProperty("summary", out var summary) || 
                       post.TryGetProperty("description", out var desc);
        
        Assert.True(hasDesc, "Descri��o n�o encontrada no endpoint");
    }

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerJson_EndpointAdicionarConvidado_DeveTermoRespostas()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonElement>(content);

        document.TryGetProperty("paths", out var paths);

        var endpoint = paths.EnumerateObject()
            .FirstOrDefault(p => p.Name.Contains("convidado", StringComparison.OrdinalIgnoreCase));

        endpoint.Value.TryGetProperty("post", out var post);
        post.TryGetProperty("responses", out var responses);

        // Assert
        Assert.True(responses.TryGetProperty("201", out _), "Resposta 201 n�o documentada");
    }

    #endregion

    #region Testes de Schemas

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerJson_DeveConterSchemasDefinidos()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        Assert.True(document.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("schemas", out var schemas));
    }

    #endregion

    #region Testes de UI

    [Fact(Skip = "Requer API rodando em http://localhost:5000")]
    public async Task SwaggerUI_DeveEstarDisponivel()
    {
        // Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.OK || 
            response.StatusCode == System.Net.HttpStatusCode.Redirect,
            "Swagger UI n�o est� dispon�vel"
        );
    }

    #endregion
}
