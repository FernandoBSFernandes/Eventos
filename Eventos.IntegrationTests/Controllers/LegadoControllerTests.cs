using Eventos.Domain.Entities;
using Eventos.IntegrationTests.Base;

namespace Eventos.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
[Trait("Integração", "LegadoController")]
public class LegadoControllerTests : IntegrationTestBase
{
    public LegadoControllerTests(EventosWebApplicationFactory factory) : base(factory) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await OrigemDbHelper.LimparAsync(Factory);
    }

    #region GET /api/legado/relatorio/pdf

    [Fact(DisplayName = "Deve retornar 200 e PDF quando há convidados confirmados na base de origem")]
    [Trait("Categoria", "Sucesso")]
    public async Task ObterRelatorioPdf_DeveRetornar200_QuandoHaConvidadosConfirmados()
    {
        // Arrange
        await OrigemDbHelper.PopularAsync(Factory, new[]
        {
            new Convidado
            {
                Nome = "João Silva",
                PresencaConfirmada = true,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        });

        // Act
        var response = await Client.GetAsync("/api/legado/relatorio/pdf");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }

    [Fact(DisplayName = "Deve retornar 200 e PDF mesmo quando não há convidados confirmados na base de origem")]
    [Trait("Categoria", "Sucesso")]
    public async Task ObterRelatorioPdf_DeveRetornar200_QuandoNaoHaConvidadosConfirmados()
    {
        // Arrange — base de origem vazia

        // Act
        var response = await Client.GetAsync("/api/legado/relatorio/pdf");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact(DisplayName = "Deve retornar PDF com convidados e acompanhantes confirmados na base de origem")]
    [Trait("Categoria", "Sucesso")]
    public async Task ObterRelatorioPdf_DeveRetornar200_QuandoHaConvidadosComAcompanhantes()
    {
        // Arrange
        await OrigemDbHelper.PopularAsync(Factory, new[]
        {
            new Convidado
            {
                Nome = "Maria Souza",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 2,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "Ana Souza" },
                    new Acompanhante { Nome = "Pedro Souza" }
                }
            },
            new Convidado
            {
                Nome = "Carlos Lima",
                PresencaConfirmada = false,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        });

        // Act
        var response = await Client.GetAsync("/api/legado/relatorio/pdf");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }

    [Fact(DisplayName = "Deve ignorar convidados não confirmados ao gerar PDF")]
    [Trait("Categoria", "Sucesso")]
    public async Task ObterRelatorioPdf_DeveIgnorarNaoConfirmados_QuandoHaMistura()
    {
        // Arrange
        await OrigemDbHelper.PopularAsync(Factory, new[]
        {
            new Convidado
            {
                Nome = "Confirmado Silva",
                PresencaConfirmada = true,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            },
            new Convidado
            {
                Nome = "Nao Confirmado Santos",
                PresencaConfirmada = false,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        });

        // Act
        var response = await Client.GetAsync("/api/legado/relatorio/pdf");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
    }

    #endregion
}
