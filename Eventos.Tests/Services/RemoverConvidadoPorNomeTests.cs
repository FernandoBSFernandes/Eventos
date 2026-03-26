namespace Eventos.Tests.Services;

[Trait("Classe", "ConvidadoService")]
[Trait("Serviço", "RemoverConvidado")]
public class RemoverConvidadoPorNomeTests : ConvidadoServiceTestBase
{
    #region Sucesso

    [Fact(DisplayName = "Deve retornar 200 quando convidado único é encontrado e removido")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornar200_QuandoConvidadoUnicoEncontradoERemovido()
    {
        // Arrange
        var convidado = new Convidado { Nome = "João Silva", Acompanhantes = new List<Acompanhante>() };
        Repo.BuscarConvidadosPorNomeAsync("João Silva").Returns(new List<Convidado> { convidado });

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("João Silva");

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.Received(1).RemoverConvidadoAsync(convidado);
    }

    [Fact(DisplayName = "Deve retornar 200 quando busca parcial retorna apenas um convidado")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRetornar200_QuandoBuscaParcialRetornaApenasUmConvidado()
    {
        // Arrange
        var convidado = new Convidado { Nome = "Maria Santos", Acompanhantes = new List<Acompanhante>() };
        Repo.BuscarConvidadosPorNomeAsync("Maria").Returns(new List<Convidado> { convidado });

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("Maria");

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.Received(1).RemoverConvidadoAsync(convidado);
    }

    #endregion

    #region Convidado Não Encontrado

    [Fact(DisplayName = "Deve retornar 404 quando nenhum convidado é encontrado")]
    [Trait("Categoria", "Não Encontrado")]
    public async Task DeveRetornar404_QuandoNenhumConvidadoEncontrado()
    {
        // Arrange
        Repo.BuscarConvidadosPorNomeAsync("Inexistente").Returns(new List<Convidado>());

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("Inexistente");

        // Assert
        Assert.Equal(404, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.DidNotReceive().RemoverConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region Múltiplos Convidados Encontrados

    [Fact(DisplayName = "Deve retornar 400 quando múltiplos convidados são encontrados")]
    [Trait("Categoria", "Validação")]
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

    [Theory(DisplayName = "Deve retornar 400 quando trÃƒÂªs convidados com nome semelhante são encontrados")]
    [Trait("Categoria", "Validação")]
    [InlineData("Carlos Lima")]
    [InlineData("Carlos Souza")]
    [InlineData("Carlos Alves")]
    public async Task DeveRetornar400_QuandoTresConvidadosEncontradosComNomeSemelhante(string nomeConvidado)
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

    [Fact(DisplayName = "Não deve remover quando múltiplos convidados são encontrados")]
    [Trait("Categoria", "Validação")]
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

    [Theory(DisplayName = "Deve retornar 400 quando nome é nulo, vazio ou espaço em branco")]
    [Trait("Categoria", "Validação")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeveRetornar400_QuandoNomeInvalido(string nome)
    {
        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync(nome);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.DidNotReceive().BuscarConvidadosPorNomeAsync(Arg.Any<string>());
    }

    #endregion

    #region Erro Interno

    [Fact(DisplayName = "Deve retornar 500 quando repositório lança exceção")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        Repo.BuscarConvidadosPorNomeAsync("João Silva")
            .Throws(new Exception("Erro na base de dados"));

        // Act
        var response = await Service.RemoverConvidadoPorNomeAsync("João Silva");

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
    }

    #endregion
}
