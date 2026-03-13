namespace Eventos.Tests.Services;

[Trait("Serviço", "RemoverDuplicatas")]
public class RemoverDuplicatasTests : AdministracaoServiceTestBase
{
    #region Sucesso

    [Fact(DisplayName = "Deve retornar 200 quando não há duplicatas")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornar200_QuandoNaoHaDuplicatas()
    {
        // Arrange
        Repo.RemoverDuplicatasAsync().Returns((0, 0));

        // Act
        var response = await Service.RemoverDuplicatasAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Contains("Convidados removidos: 0", response.Mensagem);
        Assert.Contains("Acompanhantes removidos: 0", response.Mensagem);
        await Repo.Received(1).RemoverDuplicatasAsync();
    }

    [Fact(DisplayName = "Deve retornar 200 quando há duplicatas de convidados")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornar200_QuandoHaDuplicatasDeConvidados()
    {
        // Arrange
        Repo.RemoverDuplicatasAsync().Returns((3, 0));

        // Act
        var response = await Service.RemoverDuplicatasAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Contains("Convidados removidos: 3", response.Mensagem);
        Assert.Contains("Acompanhantes removidos: 0", response.Mensagem);
        await Repo.Received(1).RemoverDuplicatasAsync();
    }

    [Fact(DisplayName = "Deve retornar 200 quando há duplicatas de acompanhantes")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornar200_QuandoHaDuplicatasDeAcompanhantes()
    {
        // Arrange
        Repo.RemoverDuplicatasAsync().Returns((0, 5));

        // Act
        var response = await Service.RemoverDuplicatasAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Contains("Convidados removidos: 0", response.Mensagem);
        Assert.Contains("Acompanhantes removidos: 5", response.Mensagem);
        await Repo.Received(1).RemoverDuplicatasAsync();
    }

    [Fact(DisplayName = "Deve retornar 200 quando há duplicatas de convidados e acompanhantes")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornar200_QuandoHaDuplicatasDeConvidadosEAcompanhantes()
    {
        // Arrange
        Repo.RemoverDuplicatasAsync().Returns((2, 4));

        // Act
        var response = await Service.RemoverDuplicatasAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Contains("Convidados removidos: 2", response.Mensagem);
        Assert.Contains("Acompanhantes removidos: 4", response.Mensagem);
        await Repo.Received(1).RemoverDuplicatasAsync();
    }

    [Fact(DisplayName = "Deve acionar repositório exatamente uma vez")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveAcionarRepositorioUmaVez_QuandoChamado()
    {
        // Arrange
        Repo.RemoverDuplicatasAsync().Returns((0, 0));

        // Act
        await Service.RemoverDuplicatasAsync();

        // Assert
        await Repo.Received(1).RemoverDuplicatasAsync();
    }

    [Fact(DisplayName = "Mensagem deve conter 'sucesso' quando operação conclui")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveMensagemConterSucesso_QuandoOperacaoConclui()
    {
        // Arrange
        Repo.RemoverDuplicatasAsync().Returns((1, 2));

        // Act
        var response = await Service.RemoverDuplicatasAsync();

        // Assert
        Assert.NotNull(response.Mensagem);
        Assert.Contains("sucesso", response.Mensagem, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Erro Interno

    [Fact(DisplayName = "Deve retornar 500 quando repositório lança exceção")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.RemoverDuplicatasAsync()
            .Returns(Task.FromException<(int, int)>(new Exception("Erro na base de dados")));

        // Act
        var response = await Service.RemoverDuplicatasAsync();

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Equal("Ocorreu um erro interno. Tente novamente mais tarde.", response.Mensagem);
        await Repo.Received(1).RemoverDuplicatasAsync();
    }

    [Fact(DisplayName = "Não deve lançar exceção quando repositório falha")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DeveNaoLancarExcecao_QuandoRepositorioFalha()
    {
        // Arrange
        Repo.RemoverDuplicatasAsync()
            .Returns(Task.FromException<(int, int)>(new Exception("Falha inesperada")));

        // Act
        var exception = await Record.ExceptionAsync(() => Service.RemoverDuplicatasAsync());

        // Assert
        Assert.Null(exception);
    }

    #endregion
}
