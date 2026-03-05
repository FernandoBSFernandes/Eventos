using Xunit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Eventos.Tests.Services;

public class VerificarConvidadoTests : EventoServiceTestBase
{
    #region Sucesso

    [Fact]
    public async Task DeveRetornarExisteTrue_QuandoConvidadoCadastrado()
    {
        // Arrange
        Repo.ConvidadoExisteAsync("João Silva").Returns(true);

        // Act
        var response = await Service.VerificarConvidadoExisteAsync("João Silva");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Consulta realizada com sucesso.", response.Mensagem);
        Assert.True(response.Existe);
        await Repo.Received(1).ConvidadoExisteAsync("João Silva");
    }

    [Fact]
    public async Task DeveRetornarExisteFalse_QuandoConvidadoNaoCadastrado()
    {
        // Arrange
        Repo.ConvidadoExisteAsync("Maria Souza").Returns(false);

        // Act
        var response = await Service.VerificarConvidadoExisteAsync("Maria Souza");

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Consulta realizada com sucesso.", response.Mensagem);
        Assert.False(response.Existe);
        await Repo.Received(1).ConvidadoExisteAsync("Maria Souza");
    }

    [Fact]
    public async Task DevePassarNomeExatoAoRepositorio_QuandoNomeValido()
    {
        // Arrange
        const string nome = "Carlos Alberto Souza";
        Repo.ConvidadoExisteAsync(nome).Returns(true);

        // Act
        await Service.VerificarConvidadoExisteAsync(nome);

        // Assert
        await Repo.Received(1).ConvidadoExisteAsync(nome);
        await Repo.DidNotReceive().ConvidadoExisteAsync(Arg.Is<string>(n => n != nome));
    }

    #endregion

    #region Validação de Nome

    [Fact]
    public async Task DeveRetornar400_QuandoNomeNulo()
    {
        // Act
        var response = await Service.VerificarConvidadoExisteAsync(null);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
        Assert.False(response.Existe);
        await Repo.DidNotReceive().ConvidadoExisteAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task DeveRetornar400_QuandoNomeVazio()
    {
        // Act
        var response = await Service.VerificarConvidadoExisteAsync("");

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
        Assert.False(response.Existe);
        await Repo.DidNotReceive().ConvidadoExisteAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task DeveRetornar400_QuandoNomeEspacoEmBranco()
    {
        // Act
        var response = await Service.VerificarConvidadoExisteAsync("   ");

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
        Assert.False(response.Existe);
        await Repo.DidNotReceive().ConvidadoExisteAsync(Arg.Any<string>());
    }

    #endregion

    #region Erro Interno

    [Fact]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.ConvidadoExisteAsync(Arg.Any<string>())
            .Returns(Task.FromException<bool>(new Exception("Erro na base de dados")));

        // Act
        var response = await Service.VerificarConvidadoExisteAsync("João Silva");

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Contains("Ocorreu um erro ao verificar o convidado: Erro na base de dados", response.Mensagem);
        Assert.False(response.Existe);
    }

    #endregion
}
