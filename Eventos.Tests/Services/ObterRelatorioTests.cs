namespace Eventos.Tests.Services;

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
        Assert.Equal("Relatório gerado com sucesso.", response.Mensagem);
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
        Assert.Equal("Relatório gerado com sucesso.", response.Mensagem);
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

    [Fact(DisplayName = "Deve eliminar duplicatas no total quando convidado e acompanhante têm mesmo nome")]
    [Trait("Categoria", "Sucesso")]
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

    [Fact(DisplayName = "Deve ignorar diferença de caixa ao comparar nomes duplicados")]
    [Trait("Categoria", "Sucesso")]
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

    [Fact(DisplayName = "Deve mapear nomes de acompanhantes corretamente")]
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
        Assert.Equal("Ocorreu um erro interno. Tente novamente mais tarde.", response.Mensagem);
        Assert.Empty(response.Convidados);
        Assert.Equal(0, response.TotalPessoas);
    }

    #endregion
}
