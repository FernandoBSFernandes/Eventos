using Eventos.Application.Configuration;
using Eventos.Application.DTOs.Request;
using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using Eventos.Domain.Entities;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eventos.Application.Services;

public class ConvidadoService : IConvidadoService
{
    private readonly IEventoRepository _repo;
    private readonly ILogger<ConvidadoService> _logger;
    private readonly int _limiteMaximoPessoas;

    public ConvidadoService(IEventoRepository repo, ILogger<ConvidadoService> logger, IOptions<EventoConfiguration> options)
    {
        _repo = repo;
        _logger = logger;
        _limiteMaximoPessoas = options.Value.LimiteMaximoPessoas;
    }

    public async Task<BaseResponse> AdicionarConvidadoAsync(AdicionarConvidadoRequest request)
    {
        try
        {
            ValidarConvidado(request);

            var quantidadeNomes = request.NomesAcompanhantes?.Count ?? 0;

            _logger.LogInformation(
                "[AdicionarConvidado] Requisição recebida | Nome: {Nome} | PresencaConfirmada: {PresencaConfirmada} | QuantidadeAcompanhantes: {QuantidadeAcompanhantes} | QuantidadeNomes: {QuantidadeNomes} | Nomes: {Nomes}",
                request.Nome,
                request.PresencaConfirmada,
                request.QuantidadeAcompanhantes,
                quantidadeNomes,
                string.Join(", ", request.NomesAcompanhantes ?? []));

            var totalAtual = await _repo.ObterTotalPessoasAsync();
            var novasPessoas = 1 + request.QuantidadeAcompanhantes;

            if (totalAtual + novasPessoas > _limiteMaximoPessoas)
            {
                _logger.LogWarning(
                    "[AdicionarConvidado] Limite de {LimiteMaximoPessoas} pessoas excedido | Total atual: {TotalAtual} | Novas pessoas: {NovasPessoas}",
                    _limiteMaximoPessoas, totalAtual, novasPessoas);

                return new BaseResponse(401, $"A quantidade máxima de pessoas a serem cadastrados extrapolou o limite de {_limiteMaximoPessoas} convidados.");
            }

            var convidado = new Convidado
            {
                Nome = request.Nome,
                PresencaConfirmada = request.PresencaConfirmada,
                Participacao = request.Participacao.ToString(),
                QuantidadeAcompanhantes = request.QuantidadeAcompanhantes,
                Acompanhantes = request.Participacao == Participacao.Sozinho
                    ? new List<Acompanhante>()
                    : request.NomesAcompanhantes?
                        .Select(nome => new Acompanhante { Nome = nome })
                        .ToList() ?? new List<Acompanhante>()
            };

            await _repo.AdicionarConvidadoAsync(convidado);

            return new BaseResponse(201, "Convidado foi registrado com sucesso");
        }
        catch (ArgumentException ex)
        {
            return new BaseResponse(400, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AdicionarConvidado] Erro inesperado ao adicionar convidado.");
            return new BaseResponse(500, "Ocorreu um erro interno. Tente novamente mais tarde.");
        }
    }

    public async Task<VerificarConvidadoResponse> VerificarConvidadoExisteAsync(string nome)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nome))
                return new VerificarConvidadoResponse(400, "O nome do convidado é obrigatório.", false, false);

            var existeComoConvidado = await _repo.ConvidadoExisteAsync(nome);
            var existeComoAcompanhante = !existeComoConvidado && await _repo.AcompanhanteExisteAsync(nome);

            return new VerificarConvidadoResponse(200, string.Empty, existeComoConvidado, existeComoAcompanhante);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VerificarConvidado] Erro inesperado ao verificar convidado.");
            return new VerificarConvidadoResponse(500, "Ocorreu um erro interno. Tente novamente mais tarde.", false, false);
        }
    }

    public async Task<ListarConvidadosResponse> ListarConvidadosAsync()
    {
        try
        {
            _logger.LogInformation("[ListarConvidados] Requisição para listar todos os convidados recebida.");

            var convidados = await _repo.ObterTodosConvidadosAsync();

            if (convidados.Count == 0)
            {
                _logger.LogInformation("[ListarConvidados] Nenhum convidado cadastrado.");
                return new ListarConvidadosResponse(200, "Nenhum convidado cadastrado.", []);
            }

            var itens = convidados.Select(c => new ConvidadoItem(
                c.Nome,
                c.PresencaConfirmada,
                c.Participacao,
                c.QuantidadeAcompanhantes,
                c.Acompanhantes.Select(a => a.Nome).ToList()
            )).ToList();

            _logger.LogInformation("[ListarConvidados] Convidados listados com sucesso | Total: {Total}", itens.Count);

            return new ListarConvidadosResponse(200, string.Empty, itens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ListarConvidados] Erro inesperado ao listar convidados.");
            return new ListarConvidadosResponse(500, "Ocorreu um erro interno. Tente novamente mais tarde.", []);
        }
    }

    public async Task<BaseResponse> RemoverConvidadoPorNomeAsync(string nome)
    {
        try
        {
            _logger.LogInformation("[RemoverConvidado] Requisição para remover convidado recebida | Nome: {Nome}", nome);

            if (string.IsNullOrWhiteSpace(nome))
                return new BaseResponse(400, "O nome do convidado é obrigatório.");

            var convidadosEncontrados = await _repo.BuscarConvidadosPorNomeAsync(nome);

            if (convidadosEncontrados.Count == 0)
            {
                _logger.LogWarning("[RemoverConvidado] Convidado não encontrado | Nome: {Nome}", nome);
                return new BaseResponse(404, "O convidado não foi encontrado. Ele ainda não foi convidado ou já foi apagado.");
            }

            if (convidadosEncontrados.Count > 1)
            {
                var nomesEncontrados = string.Join(", ", convidadosEncontrados.Select(c => c.Nome));
                _logger.LogWarning("[RemoverConvidado] Múltiplos convidados encontrados | Nome: {Nome} | Encontrados: {Encontrados}", nome, nomesEncontrados);
                return new BaseResponse(400, $"Foram encontrados {convidadosEncontrados.Count} convidados com nome semelhante: {nomesEncontrados}. Por favor, informe o nome completo para identificar o convidado correto.");
            }

            var convidado = convidadosEncontrados[0];
            await _repo.RemoverConvidadoAsync(convidado);

            _logger.LogInformation("[RemoverConvidado] Convidado removido com sucesso | Nome: {Nome}", convidado.Nome);
            return new BaseResponse(200, "Convidado removido com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RemoverConvidado] Erro inesperado ao remover convidado.");
            return new BaseResponse(500, "Ocorreu um erro interno. Tente novamente mais tarde.");
        }
    }

    public async Task<VagasRestantesResponse> ObterVagasRestantesAsync()
    {
        try
        {
            var pessoasConfirmadas = await _repo.ObterTotalPessoasAsync();
            var vagasRestantes = _limiteMaximoPessoas - pessoasConfirmadas;

            _logger.LogInformation(
                "[ObterVagasRestantes] Vagas restantes: {VagasRestantes} | Pessoas confirmadas: {PessoasConfirmadas}",
                vagasRestantes, pessoasConfirmadas);

            return new VagasRestantesResponse(200, vagasRestantes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ObterVagasRestantes] Erro inesperado ao obter vagas restantes.");
            return new VagasRestantesResponse(500, 0);
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
