using Xunit;
using NSubstitute;
using Eventos.Application.Services;
using Eventos.Application.DTOs.Request;
using Eventos.Domain.Repositories;
using Eventos.Domain.Entities;

namespace Eventos.Tests.Services;

public class EventoServiceTests
{
    private readonly IEventoRepository _repo;
    private readonly EventoService _service;

    public EventoServiceTests()
    {
        _repo = Substitute.For<IEventoRepository>();
        _service = new EventoService(_repo);
    }

    #region AdicionarConvidado - Sucesso

    [Fact]
    public async Task AdicionarConvidado_DeveRegistrarComSucesso_QuandoDadosValidos()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Jo�o Silva",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(201, response.CodigoStatus);
        Assert.Equal("Convidado foi registrado com sucesso", response.Mensagem);
        await _repo.Received(1).AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRegistrarComAcompanhantes_QuandoAcompanhadoComNomesValidos()
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
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(201, response.CodigoStatus);
        Assert.Equal("Convidado foi registrado com sucesso", response.Mensagem);
        
        await _repo.Received(1).AdicionarConvidadoAsync(Arg.Is<Convidado>(c =>
            c.Nome == "Maria Santos" &&
            c.QuantidadeAcompanhantes == 2 &&
            c.Acompanhantes.Count == 2
        ));
    }

    #endregion

    #region AdicionarConvidado - Valida��o de Nome

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeNulo()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: null,
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado � obrigat�rio", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeVazio()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado � obrigat�rio", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeMenorQue3Caracteres()
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
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome deve ter entre 3 e 50 caracteres", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeMaiorQue50Caracteres()
    {
        // Arrange
        var nome = new string('a', 51);
        var request = new AdicionarConvidadoRequest(
            nome: nome,
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome deve ter entre 3 e 50 caracteres", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region AdicionarConvidado - Valida��o de Acompanhantes

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeAcompanhantesNegativa()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Jo�o Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: -1,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("A quantidade de acompanhantes n�o pode ser negativa ou superior a 5", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeAcompanhantesMaiorQue5()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Jo�o Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 6,
            nomesAcompanhantes: new List<string>()
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("A quantidade de acompanhantes n�o pode ser negativa ou superior a 5", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoSozinhoComAcompanhantes()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Jo�o Silva",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 1,
            nomesAcompanhantes: new List<string> { "Ana Silva" }
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("Convidado que vai sozinho n�o pode ter acompanhantes", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeNaoCorrespondePdpNomes()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Jo�o Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 3,
            nomesAcompanhantes: new List<string> { "Ana Silva", "Pedro Silva" }
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("A quantidade de acompanhantes deve ser igual � quantidade de nomes informados", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteVazio()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Jo�o Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 2,
            nomesAcompanhantes: new List<string> { "Ana Silva", "" }
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("Os nomes dos acompanhantes n�o podem estar vazios", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteMenorQue3Caracteres()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Jo�o Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 1,
            nomesAcompanhantes: new List<string> { "An" }
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome de cada acompanhante deve ter entre 3 e 50 caracteres", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteMaiorQue50Caracteres()
    {
        // Arrange
        var nomeGrande = new string('a', 51);
        var request = new AdicionarConvidadoRequest(
            nome: "Jo�o Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 1,
            nomesAcompanhantes: new List<string> { nomeGrande }
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome de cada acompanhante deve ter entre 3 e 50 caracteres", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region AdicionarConvidado - Request Nulo

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoRequestNulo()
    {
        // Act
        var response = await _service.AdicionarConvidadoAsync(null);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("Dados do convidado s�o obrigat�rios", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region AdicionarConvidado - Erro Interno

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarServerError_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "Jo�o Silva",
            presencaConfirmada: true,
            participacao: Participacao.Sozinho,
            quantidadeAcompanhantes: 0,
            nomesAcompanhantes: new List<string>()
        );

        _repo.AdicionarConvidadoAsync(Arg.Any<Convidado>())
            .Returns(Task.FromException(new Exception("Erro na base de dados")));

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Contains("Ocorreu um erro ao adicionar o convidado: Erro na base de dados", response.Mensagem);
    }

    #endregion
}
