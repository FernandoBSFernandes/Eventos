using Eventos.Application.DTOs.Response;

namespace Eventos.Tests.Services;

[Trait("Classe", "RelatorioService")]
[Trait("Serviço", "ObterListaFinalConfirmados")]
public class ObterListaFinalConfirmadosTests : RelatorioServiceTestBase
{
    [Fact(DisplayName = "Deve exportar lista final usando strategy de PDF")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveExportarListaFinalUsandoStrategyPdf()
    {
        // Arrange
        var bytesEsperados = new byte[] { 10, 20, 30 };
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());
        ListaFinalConfirmadosPdfStrategy.ExportarAsync(Arg.Any<ListaFinalConfirmadosResponse>())
            .Returns(bytesEsperados);

        // Act
        var (bytes, contentType, nomeArquivo) = await Service.ExportarListaFinalConfirmadosPdfAsync();

        // Assert
        Assert.Equal(bytesEsperados, bytes);
        Assert.Equal("application/pdf", contentType);
        Assert.Equal("Lista Final de Confirmados.pdf", nomeArquivo);
        await ListaFinalConfirmadosPdfStrategy.Received(1).ExportarAsync(Arg.Any<ListaFinalConfirmadosResponse>());
    }

    [Fact(DisplayName = "Deve exportar relação pessoa x mesa usando strategy dedicada")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveExportarRelacaoPessoaMesaUsandoStrategyDedicada()
    {
        // Arrange
        var bytesEsperados = new byte[] { 99, 77, 55 };
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());
        RelacaoPessoaMesaPdfStrategy.ExportarAsync(Arg.Any<ListaFinalConfirmadosResponse>())
            .Returns(bytesEsperados);

        // Act
        var (bytes, contentType, nomeArquivo) = await Service.ExportarRelacaoPessoaMesaPdfAsync();

        // Assert
        Assert.Equal(bytesEsperados, bytes);
        Assert.Equal("application/pdf", contentType);
        Assert.Equal("Relação Pessoa x Mesa.pdf", nomeArquivo);
        await RelacaoPessoaMesaPdfStrategy.Received(1).ExportarAsync(Arg.Any<ListaFinalConfirmadosResponse>());
    }

    [Fact(DisplayName = "Deve retornar nomes confirmados em ordem alfabética com campo pago vazio")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarListaOrdenadaComPagoNulo_QuandoHaConfirmados()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new()
            {
                Nome = "Carlos",
                PresencaConfirmada = true,
                Acompanhantes = new List<Acompanhante>
                {
                    new() { Nome = "Bruna" }
                }
            },
            new()
            {
                Nome = "Ana",
                PresencaConfirmada = true,
                Acompanhantes = new List<Acompanhante>()
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterListaFinalConfirmadosAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(3, response.Confirmados.Count);
        Assert.Equal(new[] { 1, 2, 3 }, response.Confirmados.Select(x => x.Numero).ToArray());
        Assert.Equal(new[] { "Ana", "Bruna", "Carlos" }, response.Confirmados.Select(x => x.Nome).ToArray());
        Assert.All(response.Confirmados, x => Assert.Null(x.Mesa));
        Assert.All(response.Confirmados, x => Assert.Null(x.Pago));
    }

    [Fact(DisplayName = "Deve preencher mesa quando nome existir no mapeamento")]
    [Trait("Categoria", "Sucesso")]
    public async Task DevePreencherMesa_QuandoNomeExistirNoMapeamento()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new()
            {
                Nome = "Lucas Fernandes",
                PresencaConfirmada = true,
                Acompanhantes = new List<Acompanhante>
                {
                    new() { Nome = "Cecilia Araujo" }
                }
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterListaFinalConfirmadosAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(2, response.Confirmados.Count);

        var lucas = response.Confirmados.Single(x => x.Nome == "Lucas Fernandes");
        var cecilia = response.Confirmados.Single(x => x.Nome == "Cecilia Araujo");

        Assert.Equal("Mesa 1", lucas.Mesa);
        Assert.Equal("Mesa 19", cecilia.Mesa);
    }

    [Fact(DisplayName = "Deve preencher mesa usando correspondência parcial de nome")]
    [Trait("Categoria", "Sucesso")]
    public async Task DevePreencherMesa_QuandoNomeForParcialDoMapeamento()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new()
            {
                Nome = "Katia Verônica de Souza",
                PresencaConfirmada = true,
                Acompanhantes = new List<Acompanhante>
                {
                    new() { Nome = "Gabriel Ferreira Lima" },
                    new() { Nome = "Clesley Silva Júnior" }
                }
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterListaFinalConfirmadosAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(3, response.Confirmados.Count);

        var katia = response.Confirmados.Single(x => x.Nome == "Katia Verônica de Souza");
        var gabriel = response.Confirmados.Single(x => x.Nome == "Gabriel Ferreira Lima");
        var clesley = response.Confirmados.Single(x => x.Nome == "Clesley Silva Júnior");

        Assert.Equal("Mesa 13", katia.Mesa);
        Assert.Equal("Mesa 13", gabriel.Mesa);
        Assert.Equal("Mesa 15", clesley.Mesa);
    }

    [Fact(DisplayName = "Deve preencher mesa quando nome da base tiver termos intermediários")]
    [Trait("Categoria", "Sucesso")]
    public async Task DevePreencherMesa_QuandoNomeDaBaseTiverTermosIntermediarios()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new()
            {
                Nome = "Rogerio Souza Navarro Da Fonseca",
                PresencaConfirmada = true,
                Acompanhantes = new List<Acompanhante>()
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterListaFinalConfirmadosAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Single(response.Confirmados);
        Assert.Equal("Mesa 17", response.Confirmados[0].Mesa);
    }

    [Fact(DisplayName = "Deve retornar lista vazia quando não houver confirmados")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarListaVazia_QuandoNaoHaConfirmados()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());

        // Act
        var response = await Service.ObterListaFinalConfirmadosAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Empty(response.Confirmados);
        Assert.Equal("Nenhum confirmado encontrado.", response.Mensagem);
    }
}
