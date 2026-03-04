using Xunit;
using NSubstitute;

namespace Eventos.Tests.Services;

public class ZerarTabelasTests : EventoServiceTestBase
{
    #region Sucesso

    [Fact]
    public async Task DeveRetornar200_QuandoRepositorioZeraComSucesso()
    {
        // Arrange
        Repo.ZerarTabelasAsync().Returns(Task.CompletedTask);

        // Act
        var response = await Service.ZerarTabelasAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Tabelas zeradas com sucesso.", response.Mensagem);
        await Repo.Received(1).ZerarTabelasAsync();
    }

    [Fact]
    public async Task DeveAcionarRepositorioUmaVez_QuandoChamado()
    {
        // Arrange
        Repo.ZerarTabelasAsync().Returns(Task.CompletedTask);

        // Act
        await Service.ZerarTabelasAsync();

        // Assert
        await Repo.Received(1).ZerarTabelasAsync();
    }

    #endregion

    #region Erro Interno

    [Fact]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.ZerarTabelasAsync()
            .Returns(Task.FromException(new Exception("Erro na base de dados")));

        // Act
        var response = await Service.ZerarTabelasAsync();

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Contains("Ocorreu um erro ao zerar as tabelas: Erro na base de dados", response.Mensagem);
        await Repo.Received(1).ZerarTabelasAsync();
    }

    #endregion
}
