using EventosAPI;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Xunit;

namespace Eventos.Tests.Integration;

/// <summary>
/// Testes para validar a especificação OpenAPI/Swagger
/// Levanta a API automaticamente usando WebApplicationFactory
/// </summary>
public class OpenApiSpecificationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

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

    #region Testes de Especificação OpenAPI

    [Fact]
    public async Task SwaggerJson_DeveEstarDisponivel()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerJson_DeveRetornarJsonValido()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Tentar deserializar como JSON
        var document = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.Equal(JsonValueKind.Object, document.ValueKind);
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

        Assert.True(hasEndpoint, "Endpoint de convidado não encontrado no Swagger");
    }

    [Fact]
    public async Task SwaggerJson_EndpointAdicionarConvidado_DeveTermoMethodoPost()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonElement>(content);

        document.TryGetProperty("paths", out var paths);

        var endpoint = paths.EnumerateObject()
            .FirstOrDefault(p => p.Name.Contains("convidado", StringComparison.OrdinalIgnoreCase) &&
                                  p.Name.Contains("adicionar", StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.True(endpoint.Value.TryGetProperty("post", out _), "POST não encontrado no endpoint");
    }

    [Fact]
    public async Task SwaggerJson_EndpointAdicionarConvidado_DeveTermoRespostas()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonElement>(content);

        document.TryGetProperty("paths", out var paths);

        var endpoint = paths.EnumerateObject()
            .FirstOrDefault(p => p.Name.Contains("convidado", StringComparison.OrdinalIgnoreCase) &&
                                  p.Name.Contains("adicionar", StringComparison.OrdinalIgnoreCase));

        endpoint.Value.TryGetProperty("post", out var post);
        post.TryGetProperty("responses", out var responses);

        // Assert
        Assert.True(responses.TryGetProperty("201", out _), "Resposta 201 não documentada");
        Assert.True(responses.TryGetProperty("400", out _), "Resposta 400 não documentada");
    }

    #endregion

    #region Testes de Schemas

    [Fact]
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

    [Fact]
    public async Task SwaggerUI_DeveEstarDisponivel()
    {
        // Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.OK || 
            response.StatusCode == System.Net.HttpStatusCode.Redirect,
            "Swagger UI não está disponível"
        );
    }

    #endregion
}
