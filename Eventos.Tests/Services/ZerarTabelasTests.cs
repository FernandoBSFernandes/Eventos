namespace Eventos.Tests.Services;

[Trait("Classe", "AdministracaoService")]
[Trait("Serviço", "ZerarTabelas")]
public class ZerarTabelasTests : AdministracaoServiceTestBase
{
    #region Sucesso

    [Fact(DisplayName = "Deve retornar 200 quando repositório zera com sucesso")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornar200_QuandoRepositorioZeraComSucesso()
    {
        // Arrange
        Repo.ZerarTabelasAsync().Returns(Task.CompletedTask);

        // Act
        var response = await Service.ZerarTabelasAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.Received(1).ZerarTabelasAsync();
    }

    [Fact(DisplayName = "Deve acionar repositório exatamente uma vez")]
    [Trait("Categoria", "Sucesso")]
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

    [Fact(DisplayName = "Deve retornar 500 quando repositório lança exceção")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.ZerarTabelasAsync()
            .Returns(Task.FromException(new Exception("Erro na base de dados")));

        // Act
        var response = await Service.ZerarTabelasAsync();

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.Received(1).ZerarTabelasAsync();
    }

    #endregion
}
