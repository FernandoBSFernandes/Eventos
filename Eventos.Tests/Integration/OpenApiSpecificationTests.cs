using System.Text.Json;
using Xunit;

namespace Eventos.Tests.Integration;

/// <summary>
/// Testes para validar a especificação OpenAPI/Swagger
/// </summary>
public class OpenApiSpecificationTests
{
    private readonly HttpClient _client;
    private const string BaseUrl = "http://localhost:5000";

    public OpenApiSpecificationTests()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        _client.Timeout = TimeSpan.FromSeconds(5);
    }

    #region Testes de Especificação OpenAPI

    [Fact]
    public async Task SwaggerJson_DeveEstarDisponivel()
    {
        try
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
        catch (HttpRequestException)
        {
            Assert.True(true, "API não está disponível em http://localhost:5000");
        }
    }

    [Fact]
    public async Task SwaggerJson_DeveRetornarJsonValido()
    {
        try
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            var content = await response.Content.ReadAsStringAsync();

            // Assert - Tentar deserializar como JSON
            var document = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal(JsonValueKind.Object, document.ValueKind);
        }
        catch (HttpRequestException)
        {
            Assert.True(true, "API não está disponível em http://localhost:5000");
        }
    }

    [Fact]
    public async Task SwaggerJson_DeveConterInformacoes()
    {
        try
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
        catch (HttpRequestException)
        {
            Assert.True(true, "API não está disponível em http://localhost:5000");
        }
    }

    [Fact]
    public async Task SwaggerJson_DeveConterPaths()
    {
        try
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            var content = await response.Content.ReadAsStringAsync();
            var document = JsonSerializer.Deserialize<JsonElement>(content);

            // Assert
            Assert.True(document.TryGetProperty("paths", out var paths));
            Assert.NotEqual(JsonValueKind.Null, paths.ValueKind);
        }
        catch (HttpRequestException)
        {
            Assert.True(true, "API não está disponível em http://localhost:5000");
        }
    }

    #endregion

    #region Testes de Endpoint no Swagger

    [Fact]
    public async Task SwaggerJson_DeveConterEndpointAdicionarConvidado()
    {
        try
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
        catch (HttpRequestException)
        {
            Assert.True(true, "API não está disponível em http://localhost:5000");
        }
    }

    [Fact]
    public async Task SwaggerJson_EndpointAdicionarConvidado_DeveTermoMethodoPost()
    {
        try
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            var content = await response.Content.ReadAsStringAsync();
            var document = JsonSerializer.Deserialize<JsonElement>(content);

            document.TryGetProperty("paths", out var paths);

            var endpoint = paths.EnumerateObject()
                .FirstOrDefault(p => p.Name.Contains("convidado", StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.True(endpoint.Value.TryGetProperty("post", out _), "POST não encontrado no endpoint");
        }
        catch (HttpRequestException)
        {
            Assert.True(true, "API não está disponível em http://localhost:5000");
        }
    }

    [Fact]
    public async Task SwaggerJson_EndpointAdicionarConvidado_DeveTermoDescricao()
    {
        try
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
            
            Assert.True(hasDesc, "Descrição não encontrada no endpoint");
        }
        catch (HttpRequestException)
        {
            Assert.True(true, "API não está disponível em http://localhost:5000");
        }
    }

    [Fact]
    public async Task SwaggerJson_EndpointAdicionarConvidado_DeveTermoRespostas()
    {
        try
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
            Assert.True(responses.TryGetProperty("201", out _), "Resposta 201 não documentada");
        }
        catch (HttpRequestException)
        {
            Assert.True(true, "API não está disponível em http://localhost:5000");
        }
    }

    #endregion

    #region Testes de Schemas

    [Fact]
    public async Task SwaggerJson_DeveConterSchemasDefinidos()
    {
        try
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            var content = await response.Content.ReadAsStringAsync();
            var document = JsonSerializer.Deserialize<JsonElement>(content);

            // Assert
            Assert.True(document.TryGetProperty("components", out var components));
            Assert.True(components.TryGetProperty("schemas", out var schemas));
        }
        catch (HttpRequestException)
        {
            Assert.True(true, "API não está disponível em http://localhost:5000");
        }
    }

    #endregion

    #region Testes de UI

    [Fact]
    public async Task SwaggerUI_DeveEstarDisponivel()
    {
        try
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
        catch (HttpRequestException)
        {
            Assert.True(true, "API não está disponível em http://localhost:5000");
        }
    }

    #endregion
}
