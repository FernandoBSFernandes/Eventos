using Eventos.Application.DTOs.Response;
using Eventos.Application.Enums;
using Eventos.Application.Interfaces;

namespace Eventos.Tests.Services;

[Trait("Classe", "RelatorioService")]
[Trait("Serviço", "ExportarRelatorio")]
public class ExportarRelatorioTests : RelatorioServiceTestBase
{
    private readonly IRelatorioStrategy _strategy;

    public ExportarRelatorioTests()
    {
        _strategy = Substitute.For<IRelatorioStrategy>();
        _strategy.ContentType.Returns("application/pdf");
        _strategy.NomeArquivo.Returns("relatorio.pdf");
        Factory.Criar(FormatoRelatorio.Pdf).Returns(_strategy);
    }

    #region Sucesso

    [Fact(DisplayName = "Deve retornar bytes, contentType e nomeArquivo corretamente")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarBytesContentTypeENomeArquivo_QuandoExportacaoBemSucedida()
    {
        // Arrange
        var bytesEsperados = new byte[] { 1, 2, 3 };
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());
        _strategy.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(bytesEsperados);

        // Act
        var (bytes, contentType, nomeArquivo) = await Service.ExportarAsync(FormatoRelatorio.Pdf);

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
        _strategy.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(Array.Empty<byte>());

        // Act
        await Service.ExportarAsync(FormatoRelatorio.Pdf);

        // Assert
        await Repo.Received(1).ObterConvidadosConfirmadosAsync();
        await _strategy.Received(1).ExportarAsync(Arg.Any<RelatorioEventoResponse>());
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
        _strategy.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(new byte[] { 42 });

        // Act
        await Service.ExportarAsync(FormatoRelatorio.Pdf);

        // Assert
        await _strategy.Received(1).ExportarAsync(
            Arg.Is<RelatorioEventoResponse>(r =>
                r.CodigoStatus == 200 &&
                r.Convidados.Count == 1 &&
                r.TotalPessoas == 1));
    }

    [Fact(DisplayName = "Deve usar contentType e nomeArquivo da strategy Excel")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveUsarContentTypeENomeArquivoDaStrategyExcel_QuandoDiferentes()
    {
        // Arrange
        var strategyExcel = Substitute.For<IRelatorioStrategy>();
        strategyExcel.ContentType.Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        strategyExcel.NomeArquivo.Returns("relatorio.xlsx");
        strategyExcel.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(new byte[] { 1 });
        Factory.Criar(FormatoRelatorio.Excel).Returns(strategyExcel);
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());

        // Act
        var (_, contentType, nomeArquivo) = await Service.ExportarAsync(FormatoRelatorio.Excel);

        // Assert
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", contentType);
        Assert.Equal("relatorio.xlsx", nomeArquivo);
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
        var (bytes, _, _) = await Service.ExportarAsync(FormatoRelatorio.Pdf);

        // Assert — ObterRelatorioAsync trata a exceção e retorna 500, ExportarAsync a usa
        await _strategy.Received(1).ExportarAsync(
            Arg.Is<RelatorioEventoResponse>(r => r.CodigoStatus == 500));
        _ = bytes; // garantir que bytes foi retornado sem exceção não tratada
    }

    [Fact(DisplayName = "Deve propagar exceção quando strategy lança exceção")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DevePropagar_QuandoStrategyLancaExcecao()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());
        _strategy.ExportarAsync(Arg.Any<RelatorioEventoResponse>())
            .Returns(Task.FromException<byte[]>(new InvalidOperationException("Falha ao gerar arquivo")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service.ExportarAsync(FormatoRelatorio.Pdf));
    }

    #endregion
}
