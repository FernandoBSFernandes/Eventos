using Xunit;
using Eventos.Domain.Entities;
using NSubstitute;

namespace Eventos.Tests.Services;

public class ListarConvidadosTests : EventoServiceTestBase
{
    #region Sucesso

    [Fact]
    public async Task DeveRetornarTodosConvidados_QuandoHaConvidadosCadastrados()
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
                PresencaConfirmada = false,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        };

        Repo.ObterTodosConvidadosAsync().Returns(convidados);

        // Act
        var resultado = await Service.ListarConvidadosAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        await Repo.Received(1).ObterTodosConvidadosAsync();
    }

    [Fact]
    public async Task DeveRetornarListaVazia_QuandoNaoHaConvidadosCadastrados()
    {
        // Arrange
        Repo.ObterTodosConvidadosAsync().Returns(new List<Convidado>());

        // Act
        var resultado = await Service.ListarConvidadosAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
        await Repo.Received(1).ObterTodosConvidadosAsync();
    }

    [Fact]
    public async Task DeveMappearTodosCampos_QuandoConvidadoAcompanhado()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "Carlos Lima",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 1,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "Lucia Lima" }
                }
            }
        };

        Repo.ObterTodosConvidadosAsync().Returns(convidados);

        // Act
        var resultado = await Service.ListarConvidadosAsync();

        // Assert
        var item = Assert.Single(resultado);
        Assert.Equal("Carlos Lima", item.Nome);
        Assert.True(item.PresencaConfirmada);
        Assert.Equal("Acompanhado", item.Participacao);
        Assert.Equal(1, item.QuantidadeAcompanhantes);
        Assert.Single(item.NomesAcompanhantes);
        Assert.Equal("Lucia Lima", item.NomesAcompanhantes[0]);
    }

    [Fact]
    public async Task DeveMappearTodosCampos_QuandoConvidadoSozinho()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "Pedro Alves",
                PresencaConfirmada = false,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        };

        Repo.ObterTodosConvidadosAsync().Returns(convidados);

        // Act
        var resultado = await Service.ListarConvidadosAsync();

        // Assert
        var item = Assert.Single(resultado);
        Assert.Equal("Pedro Alves", item.Nome);
        Assert.False(item.PresencaConfirmada);
        Assert.Equal("Sozinho", item.Participacao);
        Assert.Equal(0, item.QuantidadeAcompanhantes);
        Assert.Empty(item.NomesAcompanhantes);
    }

    [Fact]
    public async Task DeveListarConvidadosConfirmadosENaoConfirmados_QuandoAmbosExistem()
    {
        // Arrange
        var convidados = new List<Convidado>
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
                Nome = "Nao Confirmado",
                PresencaConfirmada = false,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        };

        Repo.ObterTodosConvidadosAsync().Returns(convidados);

        // Act
        var resultado = await Service.ListarConvidadosAsync();

        // Assert
        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, c => c.PresencaConfirmada == true);
        Assert.Contains(resultado, c => c.PresencaConfirmada == false);
    }

    [Fact]
    public async Task DeveMappearNomesAcompanhantesCorretamente_QuandoMultiplosAcompanhantes()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "Fernanda Rocha",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 3,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "Acomp Um" },
                    new Acompanhante { Nome = "Acomp Dois" },
                    new Acompanhante { Nome = "Acomp Tres" }
                }
            }
        };

        Repo.ObterTodosConvidadosAsync().Returns(convidados);

        // Act
        var resultado = await Service.ListarConvidadosAsync();

        // Assert
        var item = Assert.Single(resultado);
        Assert.Equal(3, item.NomesAcompanhantes.Count);
        Assert.Contains("Acomp Um", item.NomesAcompanhantes);
        Assert.Contains("Acomp Dois", item.NomesAcompanhantes);
        Assert.Contains("Acomp Tres", item.NomesAcompanhantes);
    }

    #endregion

    #region Erro Interno

    [Fact]
    public async Task DeveLancarExcecao_QuandoRepositorioFalha()
    {
        // Arrange
        Repo.ObterTodosConvidadosAsync()
            .Returns(Task.FromException<List<Convidado>>(new Exception("Erro na base de dados")));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => Service.ListarConvidadosAsync());
        await Repo.Received(1).ObterTodosConvidadosAsync();
    }

    #endregion
}
