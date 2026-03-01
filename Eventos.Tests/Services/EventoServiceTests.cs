using Xunit;
using NSubstitute;
using Eventos.Application.Services;
using Eventos.Application.DTOs.Request;
using Eventos.Domain.Repositories;
using Eventos.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Eventos.Tests.Services;

public class EventoServiceTests
{
    private readonly IEventoRepository _repo;
    private readonly EventoService _service;

    public EventoServiceTests()
    {
        _repo = Substitute.For<IEventoRepository>();
        _service = new EventoService(_repo, NullLogger<EventoService>.Instance);
    }

    #region AdicionarConvidado - Sucesso

    [Fact]
    public async Task AdicionarConvidado_DeveRegistrarComSucesso_QuandoDadosValidos()
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

    #region AdicionarConvidado - Validação de Nome

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
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
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
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
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
        Assert.Equal("O nome deve ter entre 3 e 50 caracteres.", response.Mensagem);
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
        Assert.Equal("O nome deve ter entre 3 e 50 caracteres.", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region AdicionarConvidado - Validação de Acompanhantes

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeAcompanhantesNegativa()
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
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("A quantidade de acompanhantes não pode ser negativa ou superior a 5.", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeAcompanhantesMaiorQue5()
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
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("A quantidade de acompanhantes não pode ser negativa ou superior a 5.", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoSozinhoComAcompanhantes()
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
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("Convidado que vai sozinho não pode ter acompanhantes.", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoQuantidadeNaoCorrespondePdpNomes()
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
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("A quantidade de acompanhantes deve ser igual a quantidade de nomes informados.", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteVazio()
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
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("Os nomes dos acompanhantes não podem estar vazios.", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteMenorQue3Caracteres()
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
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome de cada acompanhante deve ter entre 3 e 50 caracteres.", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarBadRequest_QuandoNomeAcompanhanteMaiorQue50Caracteres()
    {
        // Arrange
        var nomeGrande = new string('a', 51);
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
            presencaConfirmada: true,
            participacao: Participacao.Acompanhado,
            quantidadeAcompanhantes: 1,
            nomesAcompanhantes: new List<string> { nomeGrande }
        );

        // Act
        var response = await _service.AdicionarConvidadoAsync(request);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome de cada acompanhante deve ter entre 3 e 50 caracteres.", response.Mensagem);
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
        Assert.Equal("Dados do convidado são obrigatórios.", response.Mensagem);
        await _repo.DidNotReceive().AdicionarConvidadoAsync(Arg.Any<Convidado>());
    }

    #endregion

    #region AdicionarConvidado - Erro Interno

    [Fact]
    public async Task AdicionarConvidado_DeveRetornarServerError_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        var request = new AdicionarConvidadoRequest(
            nome: "João Silva",
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

    #region VerificarConvidadoExiste - Sucesso

    [Fact]
    public async Task VerificarConvidadoExiste_DeveRetornarExistsTrue_QuandoConvidadoCadastrado()
    {
        // Arrange
        _repo.ConvidadoExisteAsync("João Silva").Returns(true);

        // Act
        var response = await _service.VerificarConvidadoExisteAsync("João Silva");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Consulta realizada com sucesso.", response.Mensagem);
        Assert.True(response.Existe);
        await _repo.Received(1).ConvidadoExisteAsync("João Silva");
    }

    [Fact]
    public async Task VerificarConvidadoExiste_DeveRetornarExistsFalse_QuandoConvidadoNaoCadastrado()
    {
        // Arrange
        _repo.ConvidadoExisteAsync("Maria Souza").Returns(false);

        // Act
        var response = await _service.VerificarConvidadoExisteAsync("Maria Souza");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Consulta realizada com sucesso.", response.Mensagem);
        Assert.False(response.Existe);
        await _repo.Received(1).ConvidadoExisteAsync("Maria Souza");
    }

    #endregion

    #region VerificarConvidadoExiste - Validação

    [Fact]
    public async Task VerificarConvidadoExiste_DeveRetornarBadRequest_QuandoNomeNulo()
    {
        // Act
        var response = await _service.VerificarConvidadoExisteAsync(null);

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
        Assert.False(response.Existe);
        await _repo.DidNotReceive().ConvidadoExisteAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task VerificarConvidadoExiste_DeveRetornarBadRequest_QuandoNomeVazio()
    {
        // Act
        var response = await _service.VerificarConvidadoExisteAsync("");

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
        Assert.False(response.Existe);
        await _repo.DidNotReceive().ConvidadoExisteAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task VerificarConvidadoExiste_DeveRetornarBadRequest_QuandoNomeEspacoEmBranco()
    {
        // Act
        var response = await _service.VerificarConvidadoExisteAsync("   ");

        // Assert
        Assert.Equal(400, response.CodigoStatus);
        Assert.Equal("O nome do convidado é obrigatório.", response.Mensagem);
        Assert.False(response.Existe);
        await _repo.DidNotReceive().ConvidadoExisteAsync(Arg.Any<string>());
    }

    #endregion

    #region VerificarConvidadoExiste - Erro Interno

    [Fact]
    public async Task VerificarConvidadoExiste_DeveRetornarServerError_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        _repo.ConvidadoExisteAsync(Arg.Any<string>())
            .Returns(Task.FromException<bool>(new Exception("Erro na base de dados")));

        // Act
        var response = await _service.VerificarConvidadoExisteAsync("João Silva");

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Contains("Ocorreu um erro ao verificar o convidado: Erro na base de dados", response.Mensagem);
        Assert.False(response.Existe);
    }

    #endregion

    #region ObterRelatorio - Sucesso

    [Fact]
    public async Task ObterRelatorio_DeveRetornarRelatorioComConvidadosEAcompanhantes_QuandoHaConfirmados()
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
                PresencaConfirmada = true,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        };

        _repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await _service.ObterRelatorioAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Relatório gerado com sucesso.", response.Mensagem);
        Assert.Equal(2, response.Convidados.Count);
        Assert.Equal(4, response.TotalPessoas); // João + Ana + Pedro + Maria
        await _repo.Received(1).ObterConvidadosConfirmadosAsync();
    }

    [Fact]
    public async Task ObterRelatorio_DeveRetornarTotalSemDuplicatas_QuandoNomesRepetidos()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "João Silva",
                PresencaConfirmada = true,
                Participacao = "Acompanhado",
                QuantidadeAcompanhantes = 1,
                Acompanhantes = new List<Acompanhante>
                {
                    new Acompanhante { Nome = "João Silva" } // mesmo nome do convidado
                }
            }
        };

        _repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await _service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal(1, response.TotalPessoas); // duplicata eliminada
    }

    [Fact]
    public async Task ObterRelatorio_DeveRetornarListaVaziaETotalZero_QuandoNaoHaConfirmados()
    {
        // Arrange
        _repo.ObterConvidadosConfirmadosAsync().Returns(new List<Convidado>());

        // Act
        var response = await _service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Equal("Relatório gerado com sucesso.", response.Mensagem);
        Assert.Empty(response.Convidados);
        Assert.Equal(0, response.TotalPessoas);
    }

    [Fact]
    public async Task ObterRelatorio_DeveRetornarAcompanhantesVazios_QuandoConvidadoVaiSozinho()
    {
        // Arrange
        var convidados = new List<Convidado>
        {
            new Convidado
            {
                Nome = "Carlos Lima",
                PresencaConfirmada = true,
                Participacao = "Sozinho",
                QuantidadeAcompanhantes = 0,
                Acompanhantes = new List<Acompanhante>()
            }
        };

        _repo.ObterConvidadosConfirmadosAsync().Returns(convidados);

        // Act
        var response = await _service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(200, response.CodigoStatus);
        Assert.Single(response.Convidados);
        Assert.Empty(response.Convidados[0].Acompanhantes);
        Assert.Equal(1, response.TotalPessoas);
    }

    #endregion

    #region ObterRelatorio - Erro Interno

    [Fact]
    public async Task ObterRelatorio_DeveRetornarServerError_QuandoRepositorioLancaExcecao()
    {
        // Arrange
        _repo.ObterConvidadosConfirmadosAsync()
            .Returns(Task.FromException<List<Convidado>>(new Exception("Erro na base de dados")));

        // Act
        var response = await _service.ObterRelatorioAsync();

        // Assert
        Assert.Equal(500, response.CodigoStatus);
        Assert.Contains("Ocorreu um erro ao gerar o relatório: Erro na base de dados", response.Mensagem);
        Assert.Empty(response.Convidados);
        Assert.Equal(0, response.TotalPessoas);
    }

    #endregion
}
