using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;

namespace Eventos.Tests.Services;

[Trait("Classe", "RelatorioService")]
[Trait("Serviço", "ObterRelatorio")]
public class ObterRelatorioTests : RelatorioServiceTestBase
{
    #region Sucesso

    [Fact(DisplayName = "Deve retornar relatório quando há convidados confirmados com acompanhantes")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarRelatorio_QuandoHaConvidadosConfirmadosComAcompanhantes()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "João Silva",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 2,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "Ana Silva" },
                    new Acompanhante { Nome = "Pedro Silva" }
                }
            },
            new Convidado
            {
                Nome = "Maria Souza",
                PresencaConfirmada = true,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        Assert.Equal(2, response.Convidados.Count);
        Assert.Equal(4, response.TotalPessoas); // João + Ana + Pedro + Maria
        await Repo.Received(1).ObterConvidadosConfirmadosAsync();
    }

    [Fact(DisplayName = "Deve retornar lista vazia e total zero quando não há convidados confirmados")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarListaVaziaETotalZero_QuandoNaoHaConvidadosConfirmados()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        Assert.Empty(response.Convidados);
        Assert.Equal(0, response.TotalPessoas);
    }

    [Fact(DisplayName = "Deve retornar acompanhantes vazios quando convidado vai sozinho")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarAcompanhantesVazios_QuandoConvidadoVaiSozinho()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "Carlos Lima",
                PresencaConfirmada = true,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Single(response.Convidados);
        Assert.Empty(response.Convidados[0].Acompanhantes);
        Assert.Equal(1, response.TotalPessoas);
    }

    [Fact(DisplayName = "Deve contabilizar todas as pessoas mesmo com nomes iguais entre convidado e acompanhante")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveContabilizarTodasPessoas_QuandoConvidadoEAcompanhanteTemMesmoNome()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "João Silva",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 1,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "João Silva" }
                }
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(2, response.TotalPessoas); // convidado + acompanhante, sem deduplicação
    }

    [Fact(DisplayName = "Deve contabilizar todas as pessoas independente de diferença de caixa nos nomes")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveContabilizarTodasPessoas_QuandoNomesIguaisComCaixasDiferentes()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "João Silva",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 1,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "JOÃO SILVA" }
                }
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(2, response.TotalPessoas); // convidado + acompanhante, sem deduplicação
    }

    [Fact(DisplayName = "Deve mapear nomes de acompanhante corretamente")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveMappearNomesAcompanhantesCorretamente_QuandoConvidadoAcompanhado()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "Fernanda Rocha",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 2,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "Lucas Rocha" },
                    new Acompanhante { Nome = "Beatriz Rocha" }
                }
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        var item = Assert.Single(response.Convidados);
        Assert.Equal("Fernanda Rocha", item.Nome);
        Assert.Equal(2, item.Acompanhantes.Count);
        Assert.Contains("Lucas Rocha", item.Acompanhantes);
        Assert.Contains("Beatriz Rocha", item.Acompanhantes);
    }

    [Fact(DisplayName = "Deve contabilizar total correto com múltiplos convidados e acompanhantes")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveContabilizarTotalCorreto_QuandoMultiplosConvidadosComAcompanhantes()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "Convidado Um",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 3,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "Acomp A" },
                    new Acompanhante { Nome = "Acomp B" },
                    new Acompanhante { Nome = "Acomp C" }
                }
            },
            new Convidado
            {
                Nome = "Convidado Dois",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 2,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "Acomp D" },
                    new Acompanhante { Nome = "Acomp E" }
                }
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(7, response.TotalPessoas); // 2 convidados + 5 acompanhantes
    }

    #endregion

    #region Erro Interno

    [Fact(DisplayName = "Deve retornar 500 quando repositório lança exceção")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync()
            .Returns(Task.FromException<List<Convidado>>(new Exception("Erro na base de dados")));

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        Assert.Empty(response.Convidados);
        Assert.Equal(0, response.TotalPessoas);
    }

    #endregion
}

[Trait("Classe", "RelatorioService")]
[Trait("Serviço", "ExportarRelatorio")]
public class ExportarRelatorioTests : RelatorioServiceTestBase
{
    private readonly IRelatorioExporter _exporter;

    public ExportarRelatorioTests()
    {
        _exporter = Substitute.For<IRelatorioExporter>();
        _exporter.ContentType.Returns("application/pdf");
        _exporter.NomeArquivo.Returns("relatorio.pdf");
    }

    #region Sucesso

    [Fact(DisplayName = "Deve retornar bytes, contentType e nomeArquivo corretamente")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarBytesContentTypeENomeArquivo_QuandoExportacaoBemSucedida()
    {
        // Arrange
        var bytesEsperados = new byte[] { 1, 2, 3 };
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());
        _exporter.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(bytesEsperados);

        // Act
        var (bytes, contentType, nomeArquivo) = await Service.ExportarAsync(_exporter);

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
        _exporter.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(Array.Empty<byte>());

        // Act
        await Service.ExportarAsync(_exporter);

        // Assert
        await Repo.Received(1).ObterConvidadosConfirmadosAsync();
        await _exporter.Received(1).ExportarAsync(Arg.Any<RelatorioEventoResponse>());
    }

    [Fact(DisplayName = "Deve passar o relatório gerado para o exporter")]
    [Trait("Categoria", "Sucesso")]
    public async Task DevePassarRelatorioGeradoParaExporter_QuandoHaConvidados()
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
        _exporter.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(new byte[] { 42 });

        // Act
        await Service.ExportarAsync(_exporter);

        // Assert
        await _exporter.Received(1).ExportarAsync(
            Arg.Is<RelatorioEventoResponse>(r =>
                r.CodigoStatus == 200 &&
                r.Convidados.Count == 1 &&
                r.TotalPessoas == 1));
    }

    [Fact(DisplayName = "Deve usar contentType e nomeArquivo do exporter concreto")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveUsarContentTypeENomeArquivoDoExporterConcreto_QuandoDiferentes()
    {
        // Arrange
        var exporterExcel = Substitute.For<IRelatorioExporter>();
        exporterExcel.ContentType.Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        exporterExcel.NomeArquivo.Returns("relatorio.xlsx");
        exporterExcel.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(new byte[] { 1 });
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());

        // Act
        var (_, contentType, nomeArquivo) = await Service.ExportarAsync(exporterExcel);

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
        var (bytes, _, _) = await Service.ExportarAsync(_exporter);

        // Assert — ObterRelatorioAsync trata a exceção e retorna 500, ExportarAsync a usa
        await _exporter.Received(1).ExportarAsync(
            Arg.Is<RelatorioEventoResponse>(r => r.CodigoStatus == 500));
        _ = bytes; // garantir que bytes foi retornado sem exceção não tratada
    }

    [Fact(DisplayName = "Deve propagar exceção quando exporter lança exceção")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DevePropagar_QuandoExporterLancaExcecao()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());
        _exporter.ExportarAsync(Arg.Any<RelatorioEventoResponse>())
            .Returns(Task.FromException<byte[]>(new InvalidOperationException("Falha ao gerar arquivo")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service.ExportarAsync(_exporter));
    }

    #endregion
}
