using Eventos.IntegrationTests.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Eventos.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
[Trait("Integração", "AdministracaoController")]
public class AdministracaoControllerTests : IntegrationTestBase
{
    public AdministracaoControllerTests(EventosWebApplicationFactory factory) : base(factory) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await OrigemDbHelper.LimparAsync(Factory);
    }

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
        Assert.Equal("Tabelas zeradas com sucesso.", body!.Mensagem);
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
        var convidados = await listarResponse.Content.ReadFromJsonAsync<List<ConvidadoItem>>();

        // Assert
        Assert.Empty(convidados!);
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
        // Arrange — força inserção de duplicata diretamente no DB
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
        var convidados = await listar.Content.ReadFromJsonAsync<List<ConvidadoItem>>();

        // Assert
        Assert.Single(convidados!);
        Assert.Equal("Repetido Costa", convidados![0].Nome);
    }

    #endregion

    #region POST /api/administracao/migrar-dados

    [Fact(DisplayName = "Deve retornar 200 e migrar convidados da base de origem para o destino")]
    [Trait("Categoria", "Sucesso")]
    public async Task MigrarDados_DeveRetornar200EMigrarConvidados_QuandoOrigemTemDados()
    {
        // Arrange
        await OrigemDbHelper.PopularAsync(Factory, new[]
        {
            new Eventos.Domain.Entities.Convidado
            {
                Nome = "Migrado Silva",
                PresencaConfirmada = true,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Eventos.Domain.Entities.Acompanhante>()
            }
        });

        // Act
        var response = await Client.PostAsync("/api/administracao/migrar-dados", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.Contains("Convidados migrados: 1", body!.Mensagem);
        Assert.Contains("Acompanhantes migrados: 0", body.Mensagem);
    }

    [Fact(DisplayName = "Deve migrar convidado com acompanhantes da base de origem para o destino")]
    [Trait("Categoria", "Sucesso")]
    public async Task MigrarDados_DeveMigrarAcompanhantes_QuandoConvidadoTemAcompanhantes()
    {
        // Arrange
        await OrigemDbHelper.PopularAsync(Factory, new[]
        {
            new Eventos.Domain.Entities.Convidado
            {
                Nome = "Com Acompanhante",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 2,
                Acompanhantes = new List<Eventos.Domain.Entities.Acompanhante>
                {
                    new Eventos.Domain.Entities.Acompanhante { Nome = "Acomp Um" },
                    new Eventos.Domain.Entities.Acompanhante { Nome = "Acomp Dois" }
                }
            }
        });

        // Act
        var response = await Client.PostAsync("/api/administracao/migrar-dados", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.Contains("Convidados migrados: 1", body!.Mensagem);
        Assert.Contains("Acompanhantes migrados: 2", body.Mensagem);
    }

    [Fact(DisplayName = "Deve retornar 200 e não migrar convidado já existente no destino")]
    [Trait("Categoria", "Sucesso")]
    public async Task MigrarDados_DeveIgnorarConvidadoJaExistente_QuandoNomeJaEstaNaDestino()
    {
        // Arrange — mesmo nome já no destino
        await AdicionarConvidadoAsync("Ja Existente");
        await OrigemDbHelper.PopularAsync(Factory, new[]
        {
            new Eventos.Domain.Entities.Convidado
            {
                Nome = "Ja Existente",
                PresencaConfirmada = true,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Eventos.Domain.Entities.Acompanhante>()
            }
        });

        // Act
        var response = await Client.PostAsync("/api/administracao/migrar-dados", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.Contains("Convidados migrados: 0", body!.Mensagem);
    }

    [Fact(DisplayName = "Deve retornar 200 informando que não há dados para migrar quando a origem está vazia")]
    [Trait("Categoria", "Sucesso")]
    public async Task MigrarDados_DeveRetornar200SemMigrar_QuandoOrigemVazia()
    {
        // Arrange — origem vazia

        // Act
        var response = await Client.PostAsync("/api/administracao/migrar-dados", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.Contains("Convidados migrados: 0", body!.Mensagem);
    }

    [Fact(DisplayName = "Deve normalizar nome com espaços duplos ao migrar")]
    [Trait("Categoria", "Sucesso")]
    public async Task MigrarDados_DeveNormalizarNome_QuandoNomePossuiEspacosDuplos()
    {
        // Arrange
        await OrigemDbHelper.PopularAsync(Factory, new[]
        {
            new Eventos.Domain.Entities.Convidado
            {
                Nome = "Nome  Com  Espacos",
                PresencaConfirmada = true,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Eventos.Domain.Entities.Acompanhante>()
            }
        });

        // Act
        var response = await Client.PostAsync("/api/administracao/migrar-dados", null);
        var listar = await Client.GetAsync("/api/convidado/listar");
        var convidados = await listar.Content.ReadFromJsonAsync<List<ConvidadoItem>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(convidados!, c => c.Nome == "Nome Com Espacos");
    }

    #endregion
}
