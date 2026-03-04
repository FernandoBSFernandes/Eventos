using Xunit;
using Eventos.Domain.Entities;
using NSubstitute;

namespace Eventos.Tests.Services;

public class ObterRelatorioTests : EventoServiceTestBase
{
    #region Sucesso

    [Fact]
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
        Assert.Equal("Relatório gerado com sucesso.", response.Mensagem);
        Assert.Equal(2, response.Convidados.Count);
        Assert.Equal(4, response.TotalPessoas); // João + Ana + Pedro + Maria
        await Repo.Received(1).ObterConvidadosConfirmadosAsync();
    }

    [Fact]
    public async Task DeveRetornarListaVaziaETotalZero_QuandoNaoHaConvidadosConfirmados()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Relatório gerado com sucesso.", response.Mensagem);
        Assert.Empty(response.Convidados);
        Assert.Equal(0, response.TotalPessoas);
    }

    [Fact]
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

    [Fact]
    public async Task DeveEliminarDuplicatasNoTotal_QuandoConvidadoEAcompanhanteTemMesmoNome()
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
        Assert.Equal(1, response.TotalPessoas);
    }

    [Fact]
    public async Task DeveIgnorarDiferencaDeCaixa_QuandoNomesIguaisComCaixasDiferentes()
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
        Assert.Equal(1, response.TotalPessoas);
    }

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.ObterConvidadosConfirmadosAsync()
            .Returns(Task.FromException<List<Convidado>>(new Exception("Erro na base de dados")));

        // Act
        var response = await Service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Contains("Ocorreu um erro ao gerar o relatório: Erro na base de dados", response.Mensagem);
        Assert.Empty(response.Convidados);
        Assert.Equal(0, response.TotalPessoas);
    }

    #endregion
}
