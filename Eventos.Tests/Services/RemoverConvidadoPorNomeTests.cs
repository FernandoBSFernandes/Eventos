using Xunit;
using Eventos.Domain.Entities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Eventos.Tests.Services;

public class RemoverConvidadoPorNomeTests : EventoServiceTestBase
{
    #region Sucesso

    [Fact]
    public async Task DeveRetornar200_QuandoConvidadoUnicoEncontradoERemovido()
    {
        // Arrange
        var convidado = new Convidado { Nome = "João Silva", Acompanhantes = new List<Acompanhante>() };
        Repo.BuscarConvidadosPorNomeAsync("João Silva").Returns(new List<Convidado> { convidado });

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("João Silva");

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Convidado removido com sucesso.", response.Mensagem);
        await Repo.Received(1).RemoverConvidadoAsync(convidado);
    }

    [Fact]
    public async Task DeveRetornar200_QuandoBuscaParcialRetornaApenasUmConvidado()
    {
        // Arrange
        var convidado = new Convidado { Nome = "Maria Santos", Acompanhantes = new List<Acompanhante>() };
        Repo.BuscarConvidadosPorNomeAsync("Maria").Returns(new List<Convidado> { convidado });

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("Maria");

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Convidado removido com sucesso.", response.Mensagem);
        await Repo.Received(1).RemoverConvidadoAsync(convidado);
    }

    #endregion

    #region Convidado Não Encontrado

    [Fact]
    public async Task DeveRetornar404_QuandoNenhumConvidadoEncontrado()
    {
        // Arrange
        Repo.BuscarConvidadosPorNomeAsync("Inexistente").Returns(new List<Convidado>());

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("Inexistente");

        // Assert
        Assert.Equal(404, response.CodigoStatus);
        Assert.Equal("O convidado não foi encontrado. Ele ainda não foi convidado ou já foi apagado.", response.Mensagem);
        await Repo.DidNotReceive().RemoverConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task DeveRetornar404_QuandoConvidadoJaFoiApagado()
    {
        // Arrange
        Repo.BuscarConvidadosPorNomeAsync("Maria Santos").Returns(new List<Convidado>());

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("Maria Santos");

        // Assert
        Assert.Equal(404, response.CodigoStatus);
        Assert.Equal("O convidado não foi encontrado. Ele ainda não foi convidado ou já foi apagado.", response.Mensagem);
        await Repo.Received(1).BuscarConvidadosPorNomeAsync("Maria Santos");
        await Repo.DidNotReceive().RemoverConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region Múltiplos Convidados Encontrados

    [Fact]
    public async Task DeveRetornar400_QuandoMultiplosConvidadosEncontrados()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new() { Nome = "Maria Santos", Acompanhantes = new List<Acompanhante>() },
            new() { Nome = "Maria Silva", Acompanhantes = new List<Acompanhante>() }
        };
        Repo.BuscarConvidadosPorNomeAsync("Maria").Returns(convidados);

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("Maria");

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Contains("Foram encontrados 2 convidados com nome semelhante", response.Mensagem);
        Assert.Contains("Maria Santos", response.Mensagem);
        Assert.Contains("Maria Silva", response.Mensagem);
        Assert.Contains("Por favor, informe o nome completo para identificar o convidado correto.", response.Mensagem);
        await Repo.DidNotReceive().RemoverConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task DeveRetornar400_QuandoTresConvidadosEncontradosComNomeSemelhante()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new() { Nome = "Carlos Lima", Acompanhantes = new List<Acompanhante>() },
            new() { Nome = "Carlos Souza", Acompanhantes = new List<Acompanhante>() },
            new() { Nome = "Carlos Alves", Acompanhantes = new List<Acompanhante>() }
        };
        Repo.BuscarConvidadosPorNomeAsync("Carlos").Returns(convidados);

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("Carlos");

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Contains("Foram encontrados 3 convidados com nome semelhante", response.Mensagem);
        Assert.Contains("Carlos Lima", response.Mensagem);
        Assert.Contains("Carlos Souza", response.Mensagem);
        Assert.Contains("Carlos Alves", response.Mensagem);
        await Repo.DidNotReceive().RemoverConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task NaoDeveRemover_QuandoMultiplosConvidadosEncontrados()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new() { Nome = "Ana Costa", Acompanhantes = new List<Acompanhante>() },
            new() { Nome = "Ana Ferreira", Acompanhantes = new List<Acompanhante>() }
        };
        Repo.BuscarConvidadosPorNomeAsync("Ana").Returns(convidados);

        // Act
        await Service.RemoverConvidadoPorNomeAsync("Ana");

        // Assert
        await Repo.DidNotReceive().RemoverConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region Validação de Nome

    [Fact]
    public async Task DeveRetornar400_QuandoNomeNulo()
    {
        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync(null);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
        await Repo.DidNotReceive().BuscarConvidadosPorNomeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task DeveRetornar400_QuandoNomeVazio()
    {
        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("");

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
        await Repo.DidNotReceive().BuscarConvidadosPorNomeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task DeveRetornar400_QuandoNomeEspacoEmBranco()
    {
        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("   ");

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
        await Repo.DidNotReceive().BuscarConvidadosPorNomeAsync(Arg.Any<string>());
    }

    #endregion

    #region Erro Interno

    [Fact]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.BuscarConvidadosPorNomeAsync("João Silva")
            .Throws(new Exception("Erro na base de dados"));

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("João Silva");

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Contains("Ocorreu um erro ao remover o convidado: Erro na base de dados", response.Mensagem);
    }

    #endregion
}
