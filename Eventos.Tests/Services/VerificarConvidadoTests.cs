namespace Eventos.Tests.Services;

[Trait("Serviço", "VerificarConvidado")]
public class VerificarConvidadoTests : ConvidadoServiceTestBase
{
    #region Sucesso

    [Fact(DisplayName = "Deve retornar existe=true quando convidado está cadastrado")]
    [Trait("Categoria", "Sucesso")]
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
        await Repo.DidNotReceive().AcompanhanteExisteAsync(Arg.Any<string>());
    }

    [Fact(DisplayName = "Deve retornar existe=false quando convidado não está cadastrado nem como acompanhante")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarExisteFalse_QuandoConvidadoNaoCadastrado()
    {
        // Arrange
        Repo.ConvidadoExisteAsync("Maria Souza").Returns(false);
        Repo.AcompanhanteExisteAsync("Maria Souza").Returns(false);

        // Act
        var response = await Service.VerificarConvidadoExisteAsync("Maria Souza");

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Consulta realizada com sucesso.", response.Mensagem);
        Assert.False(response.Existe);
        await Repo.Received(1).ConvidadoExisteAsync("Maria Souza");
        await Repo.Received(1).AcompanhanteExisteAsync("Maria Souza");
    }

    [Fact(DisplayName = "Deve retornar existe=true quando nome é encontrado na tabela de acompanhantes")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornarExisteTrue_QuandoNomeEncontradoComoAcompanhante()
    {
        // Arrange
        Repo.ConvidadoExisteAsync("Ana Silva").Returns(false);
        Repo.AcompanhanteExisteAsync("Ana Silva").Returns(true);

        // Act
        var response = await Service.VerificarConvidadoExisteAsync("Ana Silva");

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.True(response.Existe);
        await Repo.Received(1).ConvidadoExisteAsync("Ana Silva");
        await Repo.Received(1).AcompanhanteExisteAsync("Ana Silva");
    }

    [Fact(DisplayName = "Deve não consultar acompanhantes quando convidado já foi encontrado")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveNaoConsultarAcompanhantes_QuandoConvidadoJaEncontrado()
    {
        // Arrange
        Repo.ConvidadoExisteAsync("Carlos Lima").Returns(true);

        // Act
        await Service.VerificarConvidadoExisteAsync("Carlos Lima");

        // Assert
        await Repo.Received(1).ConvidadoExisteAsync("Carlos Lima");
        await Repo.DidNotReceive().AcompanhanteExisteAsync(Arg.Any<string>());
    }

    [Fact(DisplayName = "Deve passar o nome exato ao repositório")]
    [Trait("Categoria", "Sucesso")]
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

    [Fact(DisplayName = "Deve retornar 400 quando nome é nulo")]
    [Trait("Categoria", "Validação")]
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

    [Fact(DisplayName = "Deve retornar 400 quando nome é vazio")]
    [Trait("Categoria", "Validação")]
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

    [Fact(DisplayName = "Deve retornar 400 quando nome é espaço em branco")]
    [Trait("Categoria", "Validação")]
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

    [Fact(DisplayName = "Deve retornar 500 quando repositório lança exceção")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.ConvidadoExisteAsync(Arg.Any<string>())
            .Returns(Task.FromException<bool>(new Exception("Erro na base de dados")));

        // Act
        var response = await Service.VerificarConvidadoExisteAsync("João Silva");

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Equal("Ocorreu um erro interno. Tente novamente mais tarde.", response.Mensagem);
        Assert.False(response.Existe);
    }

    #endregion
}
