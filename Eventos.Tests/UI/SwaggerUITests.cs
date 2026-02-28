using Microsoft.AspNetCore.Builder;
using Microsoft.Playwright;
using System.Text.Json;
using Xunit;

namespace Eventos.Tests.UI;

/// <summary>
/// Fixture compartilhada: inicia a API e o browser uma única vez
/// para todos os testes da classe, evitando conflito de porta.
/// </summary>
public sealed class SwaggerUIFixture : IAsyncLifetime
{
    public WebApplication App { get; private set; } = null!;
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        BaseUrl = $"http://localhost:{ReadHttpPortFromLaunchSettings()}";

        App = EventosAPI.Program.CreateApp(
            ["--urls", BaseUrl, "--environment", "Development"]
        );
        await App.StartAsync();

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        Playwright.Dispose();
        await App.StopAsync();
        await App.DisposeAsync();
    }

    private static int ReadHttpPortFromLaunchSettings()
    {
        var launchSettingsPath = Path.Combine(
            Directory.GetParent(AppContext.BaseDirectory)!
                .Parent!.Parent!.Parent!.Parent!.FullName,
            "EventosAPI", "Properties", "launchSettings.json"
        );

        if (!File.Exists(launchSettingsPath))
            return 5123;

        var doc = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(launchSettingsPath));
        doc.TryGetProperty("profiles", out var profiles);

        foreach (var profile in profiles.EnumerateObject())
        {
            if (!profile.Value.TryGetProperty("applicationUrl", out var appUrl))
                continue;

            var httpUrl = appUrl.GetString()?.Split(';')
                .FirstOrDefault(u => u.StartsWith("http://") && !u.StartsWith("https://"));

            if (httpUrl is not null && Uri.TryCreate(httpUrl, UriKind.Absolute, out var uri))
                return uri.Port;
        }

        return 5123;
    }
}

/// <summary>
/// Testes de automação de UI com Playwright.
/// Simula um usuário real abrindo o browser, navegando no Swagger UI,
/// preenchendo os campos e disparando requests.
/// A URL base é lida dinamicamente do launchSettings.json.
/// </summary>
[Collection("SwaggerUI")]
public class SwaggerUITests : IClassFixture<SwaggerUIFixture>
{
    private readonly SwaggerUIFixture _fixture;

    public SwaggerUITests(SwaggerUIFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> AbrirNovaPaginaAsync()
    {
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(15_000);
        return page;
    }

    #region Testes de Navegação

    [Fact]
    public async Task UsuarioAbreSwaggerUI_PaginaDeveCarregar()
    {
        var page = await AbrirNovaPaginaAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/swagger");
        await page.WaitForSelectorAsync(".swagger-ui");

        var title = await page.TitleAsync();
        Assert.Contains("Swagger", title, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UsuarioVerEndpoint_AdicionarConvidadoDeveEstarVisivel()
    {
        var page = await AbrirNovaPaginaAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/swagger");

        // Espera o endpoint POST aparecer
        await page.WaitForSelectorAsync(".opblock.opblock-post");

        var postEndpoint = page.Locator(".opblock.opblock-post");
        Assert.True(await postEndpoint.First.IsVisibleAsync());
    }

    [Fact]
    public async Task UsuarioVerEndpoint_DeveMostrarRotaAdicionarConvidado()
    {
        var page = await AbrirNovaPaginaAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/swagger");

        await page.WaitForSelectorAsync(".opblock.opblock-post");

        var routeText = await page.Locator(".opblock.opblock-post .opblock-summary-path").First.InnerTextAsync();
        Assert.Contains("adicionar", routeText, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Testes de Interação com o Endpoint

    [Fact]
    public async Task UsuarioExpandeEndpoint_PainelDeveAbrir()
    {
        var page = await AbrirNovaPaginaAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/swagger");

        await page.WaitForSelectorAsync(".opblock.opblock-post");

        // Clica no endpoint para expandir
        await page.Locator(".opblock.opblock-post .opblock-summary").First.ClickAsync();

        // O corpo do endpoint deve aparecer
        var opblockBody = page.Locator(".opblock.opblock-post .opblock-body");
        await opblockBody.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });

        Assert.True(await opblockBody.First.IsVisibleAsync());
    }

    [Fact]
    public async Task UsuarioClicarTryItOut_CampoDeBodyDeveAparecer()
    {
        var page = await AbrirNovaPaginaAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/swagger");

        await page.WaitForSelectorAsync(".opblock.opblock-post");

        // Expande o endpoint
        await page.Locator(".opblock.opblock-post .opblock-summary").First.ClickAsync();

        // Clica em "Try it out"
        var tryItOutBtn = page.Locator(".opblock.opblock-post .try-out__btn").First;
        await tryItOutBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await tryItOutBtn.ClickAsync();

        // O textarea do body deve aparecer
        var textarea = page.Locator(".opblock.opblock-post textarea.body-param__text").First;
        await textarea.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });

        Assert.True(await textarea.IsVisibleAsync());
    }

    [Fact]
    public async Task UsuarioPreencheBodyEExecuta_ComDadosValidos_DeveRetornar201()
    {
        var page = await AbrirNovaPaginaAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/swagger");

        await page.WaitForSelectorAsync(".opblock.opblock-post");
        await page.Locator(".opblock.opblock-post .opblock-summary").First.ClickAsync();

        var tryItOutBtn = page.Locator(".opblock.opblock-post .try-out__btn").First;
        await tryItOutBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await tryItOutBtn.ClickAsync();

        var textarea = page.Locator(".opblock.opblock-post textarea.body-param__text").First;
        await textarea.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await textarea.ClickAsync();
        await textarea.SelectTextAsync();
        await textarea.FillAsync(JsonSerializer.Serialize(new
        {
            nome = "João Silva",
            iraAoRodizio = true,
            participacao = "Sozinho",
            quantidadeAcompanhantes = 0,
            nomesAcompanhantes = Array.Empty<string>()
        }, new JsonSerializerOptions { WriteIndented = true }));

        await page.Locator(".opblock.opblock-post .execute.opblock-control__btn").First.ClickAsync();

        // Aguarda a linha de resposta atual aparecer (.response_current = a resposta do request executado)
        var responseRow = page.Locator(".opblock.opblock-post .responses-table .response_current .response-col_status").First;
        await responseRow.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });

        Assert.Contains("201", await responseRow.InnerTextAsync());
    }

    [Fact]
    public async Task UsuarioPreencheBodyEExecuta_ComNomeVazio_DeveRetornar400()
    {
        var page = await AbrirNovaPaginaAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/swagger");

        await page.WaitForSelectorAsync(".opblock.opblock-post");
        await page.Locator(".opblock.opblock-post .opblock-summary").First.ClickAsync();

        var tryItOutBtn = page.Locator(".opblock.opblock-post .try-out__btn").First;
        await tryItOutBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await tryItOutBtn.ClickAsync();

        var textarea = page.Locator(".opblock.opblock-post textarea.body-param__text").First;
        await textarea.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await textarea.ClickAsync();
        await textarea.SelectTextAsync();
        await textarea.FillAsync(JsonSerializer.Serialize(new
        {
            nome = "",
            iraAoRodizio = true,
            participacao = "Sozinho",
            quantidadeAcompanhantes = 0,
            nomesAcompanhantes = Array.Empty<string>()
        }, new JsonSerializerOptions { WriteIndented = true }));

        await page.Locator(".opblock.opblock-post .execute.opblock-control__btn").First.ClickAsync();

        var responseRow = page.Locator(".opblock.opblock-post .responses-table .response_current .response-col_status").First;
        await responseRow.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });

        Assert.Contains("400", await responseRow.InnerTextAsync());
    }

    [Fact]
    public async Task UsuarioPreencheBodyEExecuta_ComAcompanhantes_DeveRetornar201()
    {
        var page = await AbrirNovaPaginaAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/swagger");

        await page.WaitForSelectorAsync(".opblock.opblock-post");
        await page.Locator(".opblock.opblock-post .opblock-summary").First.ClickAsync();

        var tryItOutBtn = page.Locator(".opblock.opblock-post .try-out__btn").First;
        await tryItOutBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await tryItOutBtn.ClickAsync();

        var textarea = page.Locator(".opblock.opblock-post textarea.body-param__text").First;
        await textarea.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await textarea.ClickAsync();
        await textarea.SelectTextAsync();
        await textarea.FillAsync(JsonSerializer.Serialize(new
        {
            nome = "Maria Santos",
            iraAoRodizio = true,
            participacao = "Acompanhado",
            quantidadeAcompanhantes = 2,
            nomesAcompanhantes = new[] { "Ana Costa", "Pedro Costa" }
        }, new JsonSerializerOptions { WriteIndented = true }));

        await page.Locator(".opblock.opblock-post .execute.opblock-control__btn").First.ClickAsync();

        var responseRow = page.Locator(".opblock.opblock-post .responses-table .response_current .response-col_status").First;
        await responseRow.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });

        Assert.Contains("201", await responseRow.InnerTextAsync());
    }

    #endregion

    #region Testes de Response visível na UI

    [Fact]
    public async Task UsuarioExecutaRequest_ResponseBodyDeveAparecer()
    {
        var page = await AbrirNovaPaginaAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/swagger");

        await page.WaitForSelectorAsync(".opblock.opblock-post");
        await page.Locator(".opblock.opblock-post .opblock-summary").First.ClickAsync();

        var tryItOutBtn = page.Locator(".opblock.opblock-post .try-out__btn").First;
        await tryItOutBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await tryItOutBtn.ClickAsync();

        var textarea = page.Locator(".opblock.opblock-post textarea.body-param__text").First;
        await textarea.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await textarea.ClickAsync();
        await textarea.SelectTextAsync();
        await textarea.FillAsync(JsonSerializer.Serialize(new
        {
            nome = "Carlos Lima",
            iraAoRodizio = true,
            participacao = "Sozinho",
            quantidadeAcompanhantes = 0,
            nomesAcompanhantes = Array.Empty<string>()
        }, new JsonSerializerOptions { WriteIndented = true }));

        await page.Locator(".opblock.opblock-post .execute.opblock-control__btn").First.ClickAsync();

        // O Swagger UI mostra o response body na div .responses-inner > .request-url
        // e o corpo real dentro de .highlight-code ou .microlight após o execute
        // O seletor correto para o body da resposta ao vivo é o bloco acima da tabela de responses
        var liveResponseBlock = page.Locator(".opblock.opblock-post .responses-inner .highlight-code").First;
        await liveResponseBlock.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });

        var responseText = await liveResponseBlock.InnerTextAsync();
        Assert.NotEmpty(responseText);
    }

    #endregion
}
