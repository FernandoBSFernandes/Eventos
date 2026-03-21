using Eventos.Application.DTOs.Request;

namespace Eventos.Tests.Services;

[Trait("Classe", "ConvidadoService")]
[Trait("Serviço", "AdicionarConvidado")]
public class AdicionarConvidadoTests : ConvidadoServiceTestBase
{
    #region Sucesso

    [Fact(DisplayName = "Deve registrar convidado sozinho com dados válidos")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRegistrarComSucesso_QuandoConvidadoSozinhoComDadosValidos()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(201, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.Received(1).AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve registrar convidado acompanhado com nomes válidos")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRegistrarComSucesso_QuandoConvidadoAcompanhadoComNomesValidos()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Maria Santos",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 2,
            nomesAcompanhantes: new List<string> { "Ana Costa", "Pedro Costa" }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(201, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.Received(1).AdicionarConvidadoAsync(Arg.Is<Convidado>(c =>
            c.Nome == "Maria Santos" &&
            c.QuantidadeAcompanhantes == 2 &&
            c.Acompanhantes.Count == 2
        ));
    }

    [Fact(DisplayName = "Deve registrar convidado sem presença confirmada")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRegistrarComSucesso_QuandoConvidadoNaoConfirmadoPresenca()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Carlos Lima",
            presencaConfirmada: false,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(201, response.CodigoStatus);
        await Repo.Received(1).AdicionarConvidadoAsync(Arg.Is<Convidado>(c =>
            !c.PresencaConfirmada
        ));
    }

    [Fact(DisplayName = "Deve registrar convidado com nome de 3 caracteres (mínimo)")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRegistrarComSucesso_QuandoNomeTem3Caracteres()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Ana",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(201, response.CodigoStatus);
    }

    [Fact(DisplayName = "Deve registrar convidado com nome de 50 caracteres (máximo)")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRegistrarComSucesso_QuandoNomeTem50Caracteres()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: new string('a', 50),
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(201, response.CodigoStatus);
    }

    [Fact(DisplayName = "Deve registrar convidado acompanhado com 5 acompanhantes (máximo)")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveRegistrarComSucesso_QuandoAcompanhadoComMaximo5Acompanhantes()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Fernanda Rocha",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 5,
            nomesAcompanhantes: new List<string>
            {
                "Nome Um", "Nome Dois", "Nome Tres", "Nome Qua", "Nome Cin"
            }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(201, response.CodigoStatus);
        await Repo.Received(1).AdicionarConvidadoAsync(Arg.Is<Convidado>(c =>
            c.Acompanhantes.Count == 5
        ));
    }

    [Fact(DisplayName = "Deve garantir lista de acompanhantes vazia quando convidado vai sozinho")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveGarantirAcompanhantesVazios_QuandoConvidadoVaiSozinho()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Pedro Alves",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        await Service.AdicionarConvidadoAsync(request);

        // Assert
        await Repo.Received(1).AdicionarConvidadoAsync(Arg.Is<Convidado>(c =>
            c.Acompanhantes.Count == 0
        ));
    }

    #endregion

    #region Validação de Request Nulo

    [Fact(DisplayName = "Deve retornar 400 quando request é nulo")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoRequestNulo()
    {
        // Act
        var response = await Service.AdicionarConvidadoAsync(null);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("Dados do convidado são obrigatórios.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
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
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: nome,
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 400 quando nome tem menos de 3 caracteres")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoNomeMenorQue3Caracteres()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Jo",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome deve ter entre 3 e 50 caracteres.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 400 quando nome tem mais de 50 caracteres")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoNomeMaiorQue50Caracteres()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: new string('a', 51),
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome deve ter entre 3 e 50 caracteres.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region Validação de Acompanhantes

    [Fact(DisplayName = "Deve retornar 400 quando quantidade de acompanhantes é negativa")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoQuantidadeAcompanhantesNegativa()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: -1,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("A quantidade de acompanhantes não pode ser negativa ou superior a 5.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 400 quando quantidade de acompanhantes é maior que 5")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoQuantidadeAcompanhantesMaiorQue5()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 6,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("A quantidade de acompanhantes não pode ser negativa ou superior a 5.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 400 quando participação é Sozinho mas há acompanhantes")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoSozinhoComAcompanhantes()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 1,
            nomesAcompanhantes: new List<string> { "Ana Silva" }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("Convidado que vai sozinho não pode ter acompanhantes.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 400 quando quantidade declarada não corresponde aos nomes informados")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoQuantidadeNaoCorrespondeAosNomes()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 3,
            nomesAcompanhantes: new List<string> { "Ana Silva", "Pedro Silva" }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("A quantidade de acompanhantes deve ser igual a quantidade de nomes informados.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 400 quando nome de acompanhante é vazio")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoNomeAcompanhanteVazio()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 2,
            nomesAcompanhantes: new List<string> { "Ana Silva", "" }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("Os nomes dos acompanhantes não podem estar vazios.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 400 quando nome de acompanhante é espaço em branco")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoNomeAcompanhanteEspacoEmBranco()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 2,
            nomesAcompanhantes: new List<string> { "Ana Silva", "   " }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("Os nomes dos acompanhantes não podem estar vazios.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 400 quando nome de acompanhante tem menos de 3 caracteres")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoNomeAcompanhanteMenorQue3Caracteres()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 1,
            nomesAcompanhantes: new List<string> { "An" }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome de cada acompanhante deve ter entre 3 e 50 caracteres.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 400 quando nome de acompanhante tem mais de 50 caracteres")]
    [Trait("Categoria", "Validação")]
    public async Task DeveRetornar400_QuandoNomeAcompanhanteMaiorQue50Caracteres()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 1,
            nomesAcompanhantes: new List<string> { new string('a', 51) }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome de cada acompanhante deve ter entre 3 e 50 caracteres.", response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region Limite de 100 Pessoas

    [Fact(DisplayName = "Deve registrar quando total atual + novas pessoas é exatamente 100")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRegistrarComSucesso_QuandoTotalAtualMaisNovasPessoasIgualA100()
    {
        // Arrange - 99 pessoas já cadastradas + 1 nova (sozinho) = 100
        Repo.ObterTotalPessoasAsync().Returns(99);

        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(201, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.Received(1).AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve registrar convidado com acompanhantes quando não excede o limite")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRegistrarComSucesso_QuandoConvidadoComAcompanhantesNaoExcedeLimite()
    {
        // Arrange - 94 pessoas já cadastradas + 1 convidado + 5 acompanhantes = 100
        Repo.ObterTotalPessoasAsync().Returns(94);

        var request = new AdicionarConvidadoRequest(
            nome: "Maria Santos",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 5,
            nomesAcompanhantes: new List<string>
            {
                "Nome Um", "Nome Dois", "Nome Tres", "Nome Qua", "Nome Cin"
            }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(201, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.Received(1).AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 401 quando convidado sozinho excede o limite")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRetornar401_QuandoConvidadoSozinhoExcedeLimite()
    {
        // Arrange - 100 pessoas já cadastradas + 1 nova = 101
        Repo.ObterTotalPessoasAsync().Returns(100);

        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(401, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 401 quando convidado com acompanhantes excede o limite")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRetornar401_QuandoConvidadoComAcompanhantesExcedeLimite()
    {
        // Arrange - 98 pessoas já cadastradas + 1 convidado + 2 acompanhantes = 101
        Repo.ObterTotalPessoasAsync().Returns(98);

        var request = new AdicionarConvidadoRequest(
            nome: "Maria Santos",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 2,
            nomesAcompanhantes: new List<string> { "Ana Costa", "Pedro Costa" }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(401, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 401 quando acompanhantes ultrapassam o limite exato")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRetornar401_QuandoAcompanhantesUltrapassamLimiteExato()
    {
        // Arrange - 95 pessoas já cadastradas + 1 convidado + 5 acompanhantes = 101
        Repo.ObterTotalPessoasAsync().Returns(95);

        var request = new AdicionarConvidadoRequest(
            nome: "Fernanda Rocha",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 5,
            nomesAcompanhantes: new List<string>
            {
                "Nome Um", "Nome Dois", "Nome Tres", "Nome Qua", "Nome Cin"
            }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(401, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve registrar convidado sozinho quando base está vazia")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRegistrarComSucesso_QuandoBaseVaziaEConvidadoSozinho()
    {
        // Arrange - base vazia + 1 pessoa = 1
        Repo.ObterTotalPessoasAsync().Returns(0);

        var request = new AdicionarConvidadoRequest(
            nome: "Carlos Lima",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(201, response.CodigoStatus);
        await Repo.Received(1).AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve registrar convidado com acompanhantes quando base está vazia")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRegistrarComSucesso_QuandoBaseVaziaEConvidadoComAcompanhantes()
    {
        // Arrange - base vazia + 1 convidado + 5 acompanhantes = 6
        Repo.ObterTotalPessoasAsync().Returns(0);

        var request = new AdicionarConvidadoRequest(
            nome: "Fernanda Rocha",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 5,
            nomesAcompanhantes: new List<string>
            {
                "Nome Um", "Nome Dois", "Nome Tres", "Nome Qua", "Nome Cin"
            }
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(201, response.CodigoStatus);
        await Repo.Received(1).AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Não deve persistir quando limite é excedido")]
    [Trait("Categoria", "Limite")]
    public async Task NaoDevePersistir_QuandoLimiteExcedido()
    {
        // Arrange - 100 já cadastradas, qualquer adição excede
        Repo.ObterTotalPessoasAsync().Returns(100);

        var request = new AdicionarConvidadoRequest(
            nome: "Novo Convidado",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(401, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact(DisplayName = "Deve retornar 401 quando limite excedido mesmo com presença não confirmada")]
    [Trait("Categoria", "Limite")]
    public async Task DeveRetornar401_QuandoLimiteExcedidoMesmoComPresencaNaoConfirmada()
    {
        // Arrange - presença não confirmada ainda conta para o limite
        Repo.ObterTotalPessoasAsync().Returns(100);

        var request = new AdicionarConvidadoRequest(
            nome: "Carlos Lima",
            presencaConfirmada: false,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(401, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region Erro Interno

    [Fact(DisplayName = "Deve retornar 500 quando repositório lança exceção")]
    [Trait("Categoria", "Erro Interno")]
    public async Task DeveRetornar500_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        Repo.AdicionarConvidadoAsync(Arg.Any<Convidado>())
            .Returns(Task.FromException(new Exception("Erro na base de dados")));

        // Act
        var response = await Service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.NotEmpty(response.Mensagem);
        await Repo.Received(1).AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion
}
