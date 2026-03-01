using Eventos.Application.DTOs.Request;
using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using Eventos.Domain.Entities;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Eventos.Application.Services;

public class EventoService : IEventoService
{
    private readonly IEventoRepository _repo;
    private readonly ILogger<EventoService> _logger;

    public EventoService(IEventoRepository repo, ILogger<EventoService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<BaseResponse> AdicionarConvidadoAsync(AdicionarConvidadoRequest request)
    {
        try
        {
            var quantidadeNomes = request?.NomesAcompanhantes?.Count ?? 0;
            var tamanhoLista = request?.NomesAcompanhantes?.Count ?? 0;

            _logger.LogInformation(
                "[AdicionarConvidado] Requisição recebida | Nome: {Nome} | QuantidadeAcompanhantes (campo): {QuantidadeAcompanhantes} | NomesAcompanhantes.Count: {QuantidadeNomes} | Tamanho da lista: {TamanhoLista} | Nomes: [{Nomes}]",
                request?.Nome,
                request?.QuantidadeAcompanhantes,
                quantidadeNomes,
                tamanhoLista,
                string.Join(", ", request?.NomesAcompanhantes ?? []));

            // Validar dados do convidado
            ValidarConvidado(request);

            // Preparar o objeto para persistência
            var convidado = new Convidado
            {
                Nome = request.Nome,
                PresencaConfirmada = request.PresencaConfirmada,
                Participacao = request.Participacao.ToString(),
                QuantidadeAcompanhantes = request.QuantidadeAcompanhantes,
                Acompanhantes = request.Participacao.ToString() == "Sozinho" 
                    ? new List<Acompanhante>()
                    : request.NomesAcompanhantes?
                        .Select(nome => new Acompanhante { Nome = nome })
                        .ToList() ?? new List<Acompanhante>()
            };

            // Persistir na base de dados
            await _repo.AdicionarConvidadoAsync(convidado);

            // Retornar sucesso com código 201
            return new BaseResponse(201, "Convidado foi registrado com sucesso");
        }
        catch (ArgumentException ex)
        {
            // Erro de validação - retornar código 400
            return new BaseResponse(400, ex.Message);
        }
        catch (Exception ex)
        {
            // Outro tipo de erro - retornar código 500
            return new BaseResponse(500, $"Ocorreu um erro ao adicionar o convidado: {ex.Message}");
        }
    }

    public async Task<VerificarConvidadoResponse> VerificarConvidadoExisteAsync(string nome)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nome))
                return new VerificarConvidadoResponse(400, "O nome do convidado é obrigatório.", false);

            var existe = await _repo.ConvidadoExisteAsync(nome);

            return new VerificarConvidadoResponse(200, "Consulta realizada com sucesso.", existe);
        }
        catch (Exception ex)
        {
            return new VerificarConvidadoResponse(500, $"Ocorreu um erro ao verificar o convidado: {ex.Message}", false);
        }
    }

    private static void ValidarConvidado(AdicionarConvidadoRequest request)
    {
        if (request == null)
            throw new ArgumentException("Dados do convidado são obrigatórios.");

        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new ArgumentException("O nome do convidado é obrigatório.");

        if (request.Nome.Length < 3 || request.Nome.Length > 50)
            throw new ArgumentException("O nome deve ter entre 3 e 50 caracteres.");

        if (request.QuantidadeAcompanhantes < 0 || request.QuantidadeAcompanhantes > 5)
            throw new ArgumentException("A quantidade de acompanhantes não pode ser negativa ou superior a 5.");

        if (request.Participacao.ToString() == "Sozinho" && request.QuantidadeAcompanhantes > 0)
            throw new ArgumentException("Convidado que vai sozinho não pode ter acompanhantes.");

        var quantidadeNomesAcompanhantes = request.NomesAcompanhantes?.Count ?? 0;

        if (request.QuantidadeAcompanhantes != quantidadeNomesAcompanhantes)
            throw new ArgumentException("A quantidade de acompanhantes deve ser igual a quantidade de nomes informados.");

        if (request.NomesAcompanhantes != null && request.NomesAcompanhantes.Any(nome => string.IsNullOrWhiteSpace(nome)))
            throw new ArgumentException("Os nomes dos acompanhantes não podem estar vazios.");

        if (request.NomesAcompanhantes != null && request.NomesAcompanhantes.Any(nome => nome.Length < 3 || nome.Length > 50))
            throw new ArgumentException("O nome de cada acompanhante deve ter entre 3 e 50 caracteres.");
    }
}
