using Eventos.IntegrationTests.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Eventos.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
[Trait("Integração", "AdministracaoController")]
[Trait("Classe", "AdministracaoController")]
public class AdministracaoControllerTests : IntegrationTestBase
{
    public AdministracaoControllerTests(EventosWebApplicationFactory factory) : base(factory) { }

    private async Task AdicionarConvidadoAsync(string nome)
    {
        var request = new AdicionarConvidadoRequest(
            nome: nome,
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>());

        await Client.PostAsJsonAsync("/api/convidado/adicionar", request);
    }

    #region DELETE /api/administracao/zerar-tabelas

    [Fact(DisplayName = "Deve retornar 200 ao zerar tabelas com dados")]
    [Trait("Categoria", "Sucesso")]
    public async Task ZerarTabelas_DeveRetornar200_QuandoHaDados()
    {
        // Arrange
        await AdicionarConvidadoAsync("Convidado Teste");

        // Act
        var response = await Client.DeleteAsync("/api/administracao/zerar-tabelas");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body!.Mensagem);
    }

    [Fact(DisplayName = "Deve retornar 200 ao zerar tabelas quando banco está vazio")]
    [Trait("Categoria", "Sucesso")]
    public async Task ZerarTabelas_DeveRetornar200_QuandoBancoVazio()
    {
        // Act
        var response = await Client.DeleteAsync("/api/administracao/zerar-tabelas");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "Deve remover todos os convidados após zerar tabelas")]
    [Trait("Categoria", "Sucesso")]
    public async Task ZerarTabelas_DeveRemoverTodosConvidados_QuandoExecutado()
    {
        // Arrange
        await AdicionarConvidadoAsync("Convidado Um");
        await AdicionarConvidadoAsync("Convidado Dois");

        // Act
        await Client.DeleteAsync("/api/administracao/zerar-tabelas");

        var listarResponse = await Client.GetAsync("/api/convidado/listar");
        var convidados = await listarResponse.Content.ReadFromJsonAsync<ListarConvidadosResponse>();

        // Assert
        Assert.NotNull(convidados);
        Assert.Empty(convidados.Convidados);
    }

    #endregion

    #region DELETE /api/administracao/remover-duplicatas

    [Fact(DisplayName = "Deve retornar 200 quando não há duplicatas")]
    [Trait("Categoria", "Sucesso")]
    public async Task RemoverDuplicatas_DeveRetornar200_QuandoNaoHaDuplicatas()
    {
        // Arrange
        await AdicionarConvidadoAsync("Convidado Unico");

        // Act
        var response = await Client.DeleteAsync("/api/administracao/remover-duplicatas");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.Contains("Convidados removidos: 0", body!.Mensagem);
        Assert.Contains("Acompanhantes removidos: 0", body.Mensagem);
    }

    [Fact(DisplayName = "Deve retornar 200 e contar duplicatas removidas")]
    [Trait("Categoria", "Sucesso")]
    public async Task RemoverDuplicatas_DeveRetornar200EContarDuplicatas_QuandoHaDuplicatas()
    {
        // Arrange Ã¢Â€Â” força inserção de duplicata diretamente no DB
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Eventos.Infrastructure.Data.EventosDbContext>();

        db.Convidado.AddRange(
            new Eventos.Domain.Entities.Convidado { Nome = "Duplicado Silva", PresencaConfirmada = true, Participacao = "Sozinho", QuantidadeAcompanhantes = 0 },
            new Eventos.Domain.Entities.Convidado { Nome = "Duplicado Silva", PresencaConfirmada = true, Participacao = "Sozinho", QuantidadeAcompanhantes = 0 },
            new Eventos.Domain.Entities.Convidado { Nome = "Duplicado Silva", PresencaConfirmada = true, Participacao = "Sozinho", QuantidadeAcompanhantes = 0 }
        );
        await db.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync("/api/administracao/remover-duplicatas");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.Contains("Convidados removidos: 2", body!.Mensagem);
    }

    [Fact(DisplayName = "Deve manter apenas um registro após remover duplicatas")]
    [Trait("Categoria", "Sucesso")]
    public async Task RemoverDuplicatas_DeveManterApenasUmRegistro_QuandoHaDuplicatas()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Eventos.Infrastructure.Data.EventosDbContext>();

        db.Convidado.AddRange(
            new Eventos.Domain.Entities.Convidado { Nome = "Repetido Costa", PresencaConfirmada = true, Participacao = "Sozinho", QuantidadeAcompanhantes = 0 },
            new Eventos.Domain.Entities.Convidado { Nome = "Repetido Costa", PresencaConfirmada = true, Participacao = "Sozinho", QuantidadeAcompanhantes = 0 }
        );
        await db.SaveChangesAsync();

        // Act
        await Client.DeleteAsync("/api/administracao/remover-duplicatas");

        var verificar = await Client.GetAsync("/api/convidado/verificar?nome=Repetido");
        var listar = await Client.GetAsync("/api/convidado/listar");
        var convidados = await listar.Content.ReadFromJsonAsync<ListarConvidadosResponse>();

        // Assert
        Assert.NotNull(convidados);
        Assert.Single(convidados.Convidados);
        Assert.Equal("Repetido Costa", convidados.Convidados[0].Nome);
    }

    #endregion
}
