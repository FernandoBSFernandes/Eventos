using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Response;

namespace Eventos.Tests.Services;

[Trait("Serviço", "LegadoRelatorio")]
public class LegadoRelatorioServiceTests : LegadoRelatorioServiceTestBase
{
    private static IRelatorioExporter CriarExporterFake()
    {
        var exporter = Substitute.For<IRelatorioExporter>();
        exporter.ContentType.Returns("application/pdf");
        exporter.NomeArquivo.Returns("relatorio.pdf");
        exporter.ExportarAsync(Arg.Any<RelatorioEventoResponse>()).Returns(new byte[] { 1, 2, 3 });
        return exporter;
    }

    #region Sucesso

    [Fact(DisplayName = "Deve retornar PDF quando há convidados confirmados com acompanhantes")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarPdf_QuandoHaConvidadosConfirmadosComAcompanhantes()
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
        var exporter = CriarExporterFake();

        // Act
        var (bytes, contentType, nomeArquivo) = await Service.ExportarPdfAsync(exporter);

        // Assert
        Assert.NotEmpty(bytes);
        Assert.Equal("application/pdf", contentType);
        Assert.Equal("relatorio.pdf", nomeArquivo);
        await Repo.Received(1).ObterConvidadosConfirmadosAsync();
        await exporter.Received(1).ExportarAsync(Arg.Is<RelatorioEventoResponse>(r =>
            r.Convidados.Count == 2 && r.TotalPessoas == 4));
    }

    [Fact(DisplayName = "Deve retornar PDF com lista vazia quando não há convidados confirmados")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarPdf_QuandoNaoHaConvidadosConfirmados()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());
        var exporter = CriarExporterFake();

        // Act
        var (bytes, contentType, _) = await Service.ExportarPdfAsync(exporter);

        // Assert
        Assert.NotEmpty(bytes);
        Assert.Equal("application/pdf", contentType);
        await exporter.Received(1).ExportarAsync(Arg.Is<RelatorioEventoResponse>(r =>
            r.Convidados.Count == 0 && r.TotalPessoas == 0));
    }

    [Fact(DisplayName = "Deve contabilizar corretamente o total de pessoas incluindo acompanhantes")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveContabilizarTotalPessoas_IncluindoAcompanhantes()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "Carlos Lima",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 3,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "A" },
                    new Acompanhante { Nome = "B" },
                    new Acompanhante { Nome = "C" }
                }
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);
        var exporter = CriarExporterFake();

        // Act
        var _ = await Service.ExportarPdfAsync(exporter);

        // Assert
        await exporter.Received(1).ExportarAsync(Arg.Is<RelatorioEventoResponse>(r =>
            r.TotalPessoas == 4));
    }

    [Fact(DisplayName = "Deve mapear nomes dos acompanhantes corretamente")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveMappearNomesAcompanhantes_QuandoConvidadoAcompanhado()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "Fernanda Rocha",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 1,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "Lucas Rocha" }
                }
            }
        };

        Repo.ObterConvidadosConfirmadosAsync().Returns(convidados);
        var exporter = CriarExporterFake();

        // Act
        var _ = await Service.ExportarPdfAsync(exporter);

        // Assert
        await exporter.Received(1).ExportarAsync(Arg.Is<RelatorioEventoResponse>(r =>
            r.Convidados[0].Acompanhantes.Contains("Lucas Rocha")));
    }

    #endregion

    #region Erro Interno

    [Fact(DisplayName = "Deve propagar exceção quando repositório lança erro")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DevePropagarExcecao_QuandoRepositorioLancaErro()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync()
            .ThrowsAsync(new Exception("Erro na base de origem"));
        var exporter = CriarExporterFake();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => Service.ExportarPdfAsync(exporter));
        await exporter.DidNotReceive().ExportarAsync(Arg.Any<RelatorioEventoResponse>());
    }

    #endregion
}
