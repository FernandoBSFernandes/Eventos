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

            _logger.LogInformation(
                "[AdicionarConvidado] Requisição recebida | Nome: {Nome} | VaiAoEvento: {VaiAoEvento} | QuantidadeAcompanhantes (campo): {QuantidadeAcompanhantes} | NomesAcompanhantes.Count: {QuantidadeNomes} | Nomes: [{Nomes}]",
                request?.Nome,
                request?.PresencaConfirmada == true ? "Sim" : "Não",
                request?.QuantidadeAcompanhantes,
                quantidadeNomes,
                string.Join(", ", request?.NomesAcompanhantes ?? []));

            // Validar dados do convidado
            ValidarConvidado(request);

            // Verificar limite de 100 pessoas
            var totalAtual = await _repo.ObterTotalPessoasAsync();
            var novasPessoas = 1 + request.QuantidadeAcompanhantes;

            if (totalAtual + novasPessoas > 100)
            {
                _logger.LogWarning(
                    "[AdicionarConvidado] Limite de 100 pessoas excedido | Total atual: {TotalAtual} | Novas pessoas: {NovasPessoas}",
                    totalAtual, novasPessoas);

                return new BaseResponse(401, "A quantidade máxima de pessoas a serem cadastrados extrapolou o limite de 100 convidados.");
            }

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

    public async Task<BaseResponse> ZerarTabelasAsync()
    {
        try
        {
            _logger.LogInformation("[ZerarTabelas] Requisição para zerar as tabelas recebida.");

            await _repo.ZerarTabelasAsync();

            _logger.LogInformation("[ZerarTabelas] Tabelas zeradas com sucesso.");

            return new BaseResponse(200, "Tabelas zeradas com sucesso.");
        }
        catch (Exception ex)
        {
            return new BaseResponse(500, $"Ocorreu um erro ao zerar as tabelas: {ex.Message}");
        }
    }

    public async Task<RelatorioEventoResponse> ObterRelatorioAsync()
    {
        try
        {
            _logger.LogInformation("[ObterRelatorio] Requisição de relatório recebida.");

            var convidados = await _repo.ObterConvidadosConfirmadosAsync();

            var itens = convidados.Select(c => new ConvidadoRelatorioItem(
                c.Nome,
                c.Acompanhantes.Select(a => a.Nome).ToList()
            )).ToList();

            // Total = convidados confirmados + todos os seus acompanhantes, sem duplicatas de nome
            var todosOsNomes = convidados
                .Select(c => c.Nome)
                .Concat(convidados.SelectMany(c => c.Acompanhantes.Select(a => a.Nome)))
                .Select(n => n.Trim().ToLower())
                .Distinct()
                .Count();

            _logger.LogInformation("[ObterRelatorio] Relatório gerado | Convidados confirmados: {TotalConvidados} | Total de pessoas: {TotalPessoas}",
                itens.Count, todosOsNomes);

            return new RelatorioEventoResponse(200, "Relatório gerado com sucesso.", itens, todosOsNomes);
        }
        catch (Exception ex)
        {
            return new RelatorioEventoResponse(500, $"Ocorreu um erro ao gerar o relatório: {ex.Message}", [], 0);
        }
    }

    public async Task<List<ConvidadoItem>> ListarConvidadosAsync()
    {
        _logger.LogInformation("[ListarConvidados] Requisição para listar todos os convidados recebida.");

        var convidados = await _repo.ObterTodosConvidadosAsync();

        var itens = convidados.Select(c => new ConvidadoItem(
            c.Nome,
            c.PresencaConfirmada,
            c.Participacao,
            c.QuantidadeAcompanhantes,
            c.Acompanhantes.Select(a => a.Nome).ToList()
        )).ToList();

        _logger.LogInformation("[ListarConvidados] Convidados listados com sucesso | Total: {Total}", itens.Count);

        return itens;
    }

    public async Task<BaseResponse> RemoverDuplicatasAsync()
    {
        try
        {
            _logger.LogInformation("[RemoverDuplicatas] Requisição para remover duplicatas recebida.");

            var (convidadosRemovidos, acompanhantesRemovidos) = await _repo.RemoverDuplicatasAsync();

            _logger.LogInformation(
                "[RemoverDuplicatas] Duplicatas removidas | Convidados: {Convidados} | Acompanhantes: {Acompanhantes}",
                convidadosRemovidos, acompanhantesRemovidos);

            return new BaseResponse(200,
                $"Duplicatas removidas com sucesso. Convidados removidos: {convidadosRemovidos}. Acompanhantes removidos: {acompanhantesRemovidos}.");
        }
        catch (Exception ex)
        {
            return new BaseResponse(500, $"Ocorreu um erro ao remover duplicatas: {ex.Message}");
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
