using EventosAPI;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using System.Net;
using Xunit;

namespace Eventos.Tests.Integration;

/// <summary>
/// Simula um usuário abrindo o Swagger UI, lendo o schema,
/// preenchendo o corpo da requisição e disparando o request.
/// A URL base é lida dinamicamente do launchSettings.json.
/// </summary>
public class SwaggerUserSimulationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private JsonElement _swaggerDocument;
    private string _swaggerBaseUrl;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();

        // Lê dinamicamente a URL do launchSettings.json
        _swaggerBaseUrl = ReadSwaggerUrlFromLaunchSettings();

        // Carrega o documento Swagger uma única vez para todos os testes
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        _swaggerDocument = JsonSerializer.Deserialize<JsonElement>(content);

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    #region Helpers

    /// <summary>
    /// Lê a URL base do Swagger dinamicamente do launchSettings.json,
    /// usando o primeiro perfil HTTP disponível.
    /// </summary>
    private static string ReadSwaggerUrlFromLaunchSettings()
    {
        var launchSettingsPath = Path.Combine(
            Directory.GetParent(AppContext.BaseDirectory)!
                .Parent!.Parent!.Parent!.Parent!.FullName,
            "EventosAPI", "Properties", "launchSettings.json"
        );

        if (!File.Exists(launchSettingsPath))
            return "http://localhost:5123/swagger";

        var json = File.ReadAllText(launchSettingsPath);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);

        doc.TryGetProperty("profiles", out var profiles);

        foreach (var profile in profiles.EnumerateObject())
        {
            if (!profile.Value.TryGetProperty("applicationUrl", out var appUrl))
                continue;

            // Pega a primeira URL HTTP (não HTTPS) para evitar problemas de certificado em testes
            var urls = appUrl.GetString()?.Split(';') ?? [];
            var httpUrl = urls.FirstOrDefault(u => u.StartsWith("http://"));

            if (httpUrl is not null)
            {
                if (profile.Value.TryGetProperty("launchUrl", out var launchUrl))
                    return $"{httpUrl}/{launchUrl.GetString()}";

                return $"{httpUrl}/swagger";
            }
        }

        return "http://localhost:5123/swagger";
    }

    /// <summary>
    /// Simula o Swagger UI: lê o schema do endpoint no documento OpenAPI
    /// e retorna os campos com seus tipos para o usuário "preencher".
    /// </summary>
    private Dictionary<string, string> LerSchemaDoEndpoint(string path, string method)
    {
        var fields = new Dictionary<string, string>();

        if (!_swaggerDocument.TryGetProperty("paths", out var paths))
            return fields;

        var endpoint = paths.EnumerateObject()
            .FirstOrDefault(p => p.Name.Equals(path, StringComparison.OrdinalIgnoreCase));

        if (!endpoint.Value.TryGetProperty(method, out var operation))
            return fields;

        if (!operation.TryGetProperty("requestBody", out var requestBody))
            return fields;

        requestBody.TryGetProperty("content", out var content);
        content.TryGetProperty("application/json", out var appJson);
        appJson.TryGetProperty("schema", out var schema);

        // Resolve $ref se necessário
        if (schema.TryGetProperty("$ref", out var schemaRef))
        {
            var refPath = schemaRef.GetString()!.Replace("#/components/schemas/", "");
            _swaggerDocument.TryGetProperty("components", out var components);
            components.TryGetProperty("schemas", out var schemas);
            schemas.TryGetProperty(refPath, out schema);
        }

        if (!schema.TryGetProperty("properties", out var properties))
            return fields;

        foreach (var prop in properties.EnumerateObject())
        {
            var type = prop.Value.TryGetProperty("type", out var t) ? t.GetString() : "unknown";
            fields[prop.Name] = type!;
        }

        return fields;
    }

    #endregion

    #region Testes de Simulação do Usuário no Swagger

    [Fact]
    public void SwaggerUrl_DeveSerLidaDoLaunchSettings()
    {
        // Verifica que a URL foi lida corretamente do launchSettings.json
        Assert.NotNull(_swaggerBaseUrl);
        Assert.Contains("swagger", _swaggerBaseUrl);
        Assert.StartsWith("http", _swaggerBaseUrl);
    }

    [Fact]
    public async Task UsuarioAbreSwagger_DocumentoDeveEstarDisponivel()
    {
        // Simula: usuário abre o Swagger UI no browser
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json", response.Content.Headers.ContentType?.ToString() ?? "");
    }

    [Fact]
    public void UsuarioVerEndpoints_AdicionarConvidadoDeveApararecer()
    {
        // Simula: usuário visualiza a lista de endpoints no Swagger UI
        _swaggerDocument.TryGetProperty("paths", out var paths);

        var endpoint = paths.EnumerateObject()
            .FirstOrDefault(p => p.Name.Contains("convidado", StringComparison.OrdinalIgnoreCase) &&
                                  p.Name.Contains("adicionar", StringComparison.OrdinalIgnoreCase));

        Assert.NotEqual(default, endpoint);
        Assert.True(endpoint.Value.TryGetProperty("post", out _),
            $"Endpoint encontrado ({endpoint.Name}) mas não tem método POST");
    }

    [Fact]
    public void UsuarioExpandeEndpoint_DeveVerCamposDoBody()
    {
        // Simula: usuário clica no endpoint e lê o schema do body no Swagger UI
        var campos = LerSchemaDoEndpoint("/api/Convidado/adicionar", "post");

        Assert.NotEmpty(campos);
        Assert.Contains(campos.Keys, k => k.Equals("nome", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task UsuarioPreencheBodyEDispara_ComDadosValidos_DeveRetornar201()
    {
        // Simula: usuário lê o schema, preenche o body no Swagger UI e clica em "Execute"
        var campos = LerSchemaDoEndpoint("/api/Convidado/adicionar", "post");
        Assert.NotEmpty(campos);

        // Usuário preenche os campos conforme o schema
        var body = new
        {
            nome = "João Silva",
            iraAoRodizio = true,
            participacao = "Sozinho",
            quantidadeAcompanhantes = 0,
            nomesAcompanhantes = Array.Empty<string>()
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Usuário clica em "Execute"
        var response = await _client.PostAsync("/api/Convidado/adicionar", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UsuarioPreencheBodyEDispara_ComAcompanhantes_DeveRetornar201()
    {
        // Simula: usuário preenche body com acompanhantes e clica em "Execute"
        var campos = LerSchemaDoEndpoint("/api/Convidado/adicionar", "post");
        Assert.NotEmpty(campos);

        var body = new
        {
            nome = "Maria Santos",
            iraAoRodizio = true,
            participacao = "Acompanhado",
            quantidadeAcompanhantes = 2,
            nomesAcompanhantes = new[] { "Ana Costa", "Pedro Costa" }
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Convidado/adicionar", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UsuarioPreencheBodyEDispara_SemNome_DeveRetornar400()
    {
        // Simula: usuário deixa o campo "nome" vazio e clica em "Execute"
        var body = new
        {
            nome = "",
            iraAoRodizio = true,
            participacao = "Sozinho",
            quantidadeAcompanhantes = 0,
            nomesAcompanhantes = Array.Empty<string>()
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Convidado/adicionar", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UsuarioPreencheBodyEDispara_AcompanhantesExcedendo_DeveRetornar400()
    {
        // Simula: usuário preenche mais acompanhantes que o permitido
        var body = new
        {
            nome = "Carlos Lima",
            iraAoRodizio = true,
            participacao = "Acompanhado",
            quantidadeAcompanhantes = 6,
            nomesAcompanhantes = new[] { "A", "B", "C", "D", "E", "F" }
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Convidado/adicionar", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UsuarioVerRespostaDoSwagger_201_DeveConterMensagemDeSucesso()
    {
        // Simula: usuário executa e lê a resposta no painel do Swagger UI
        var body = new
        {
            nome = "Lucas Pereira",
            iraAoRodizio = true,
            participacao = "Sozinho",
            quantidadeAcompanhantes = 0,
            nomesAcompanhantes = Array.Empty<string>()
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Convidado/adicionar", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Usuário lê o response body retornado pelo Swagger
        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
        Assert.True(result.TryGetProperty("mensagem", out var mensagem));
        Assert.Contains("sucesso", mensagem.GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UsuarioVerRespostaDoSwagger_400_DeveConterMensagemDeErro()
    {
        // Simula: usuário vê resposta de erro 400 no painel do Swagger UI
        var body = new
        {
            nome = "AB", // Nome muito curto (menos de 3 chars)
            iraAoRodizio = true,
            participacao = "Sozinho",
            quantidadeAcompanhantes = 0,
            nomesAcompanhantes = Array.Empty<string>()
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Convidado/adicionar", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Usuário lê o response body de erro retornado pelo Swagger
        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
        Assert.True(result.TryGetProperty("mensagem", out var mensagem));
        Assert.NotNull(mensagem.GetString());
    }

    #endregion
}
