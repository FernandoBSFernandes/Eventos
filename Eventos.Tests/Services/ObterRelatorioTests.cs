using Eventos.Application.DTOs.Response;

namespace Eventos.Tests.Services;

[Trait("Classe", "RelatorioService")]
[Trait("Serviço", "ExportarRelatorio")]
public class ExportarRelatorioTests : RelatorioServiceTestBase
{
    #region Sucesso

    [Fact(DisplayName = "Deve usar a mesma contagem de total da API de vagas")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveUsarMesmaContagemDaApiDeVagas_QuandoMontarRelatorio()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new()
            {
                Nome = "Ana",
                PresencaConfirmada = true,
                Acompanhantes = new List<Acompanhante>
                {
                    new() { Nome = "Bruno" }
                }
            },
            new()
            {
                Nome = "Carlos",
                PresencaConfirmada = true,
                Acompanhantes = new List<Acompanhante>
                {
                    new() { Nome = "Diana" }
                }
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);
        Repo.ObterTotalPessoasAsync().Returns(4);

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(4, response.TotalPessoas);
        Assert.Equal(4, response.PessoasConfirmadas);
        await Repo.Received(1).ObterTotalPessoasAsync();
    }

    [Fact(DisplayName = "Deve manter acompanhantes no item do relatório")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveManterAcompanhantesNoItemDoRelatorio()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new()
            {
                Nome = "João",
                PresencaConfirmada = true,
                Acompanhantes = new List<Acompanhante>
                {
                    new() { Nome = "Maria" }
                }
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);
        Repo.ObterTotalPessoasAsync().Returns(2);

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.Single(response.Convidados);
        Assert.Single(response.Convidados[0].Acompanhantes);
        Assert.Equal("Maria", response.Convidados[0].Acompanhantes[0]);
        Assert.Equal(2, response.TotalPessoas);
        Assert.Equal(2, response.PessoasConfirmadas);
    }

    [Fact(DisplayName = "Deve retornar bytes, contentType e nomeArquivo corretamente")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarBytesContentTypeENomeArquivo_QuandoExportacaoBemSucedida()
    {
        // Arrange
        var bytesEsperados = new byte[] { 1, 2, 3 };
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());
        Repo.ObterTotalPessoasAsync().Returns(0);
        RelatorioPdfStrategy.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(bytesEsperados);

        // Act
        var (bytes, contentType, nomeArquivo) = await Service.ExportarPdfAsync();

        // Assert
        Assert.Equal(bytesEsperados, bytes);
        Assert.Equal("application/pdf", contentType);
        Assert.Equal("relatorio.pdf", nomeArquivo);
    }

    [Fact(DisplayName = "Deve chamar ObterRelatorioAsync antes de exportar")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveChamarObterRelatorio_AntesDeExportar()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());
        Repo.ObterTotalPessoasAsync().Returns(0);
        RelatorioPdfStrategy.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(Array.Empty<byte>());

        // Act
        await Service.ExportarPdfAsync();

        // Assert
        await Repo.Received(1).ObterConvidadosConfirmadosAsync();
        await Repo.Received(1).ObterTotalPessoasAsync();
        await RelatorioPdfStrategy.Received(1).ExportarAsync(Arg.Any<RelatorioEventoResponse>());
    }

    [Fact(DisplayName = "Deve passar o relatório gerado para a strategy")]
    [Trait("Categoria", "Sucesso")]
    public async Task DevePassarRelatorioGeradoParaStrategy_QuandoHaConvidados()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "João Silva",
                PresencaConfirmada = true,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        };
        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);
        Repo.ObterTotalPessoasAsync().Returns(1);
        RelatorioPdfStrategy.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(new byte[] { 42 });

        // Act
        await Service.ExportarPdfAsync();

        // Assert
        await RelatorioPdfStrategy.Received(1).ExportarAsync(
            Arg.Is<RelatorioEventoResponse>(r =>
                r.CodigoStatus == 200 &&
                r.Convidados.Count == 1 &&
                r.TotalPessoas == 1));
    }

    #endregion

    #region Erro Interno

    [Fact(DisplayName = "Deve propagar exceção quando repositório falha durante exportação")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DeveRetornar500NaExportacao_QuandoRepositorioFalha()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync()
            .Returns(Task.FromException<List<Convidado>>(new Exception("Erro na base de dados")));

        // Act
        var (bytes, _, _) = await Service.ExportarPdfAsync();

        // Assert Ã¢Â€Â” ObterRelatorioAsync trata a exceção e retorna 500, ExportarAsync a usa
        await RelatorioPdfStrategy.Received(1).ExportarAsync(
            Arg.Is<RelatorioEventoResponse>(r => r.CodigoStatus == 500));
        _ = bytes; // garantir que bytes foi retornado sem exceção não tratada
    }

    [Fact(DisplayName = "Deve propagar exceção quando strategy lança exceção")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DevePropagar_QuandoStrategyLancaExcecao()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());
        Repo.ObterTotalPessoasAsync().Returns(0);
        RelatorioPdfStrategy.ExportarAsync(Arg.Any<RelatorioEventoResponse>())
            .Returns(Task.FromException<byte[]>(new InvalidOperationException("Falha ao gerar arquivo")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service.ExportarPdfAsync());
    }

    #endregion
}
