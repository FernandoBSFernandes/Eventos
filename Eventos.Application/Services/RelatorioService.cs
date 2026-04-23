using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace Eventos.Application.Services;

public class RelatorioService : IRelatorioService
{
    private static readonly Dictionary<string, string> MapaMesaPorNome = new()
    {
        ["Lucas Fernandes"] = "Mesa 1",
        ["Giselda Barros"] = "Mesa 1",
        ["Flávia Fernandes"] = "Mesa 1",
        ["Gustavo Oliveira"] = "Mesa 1",
        ["Jane Fernandes"] = "Mesa 1",
        ["William Danilo"] = "Mesa 1",

        ["Laércio Fernandes"] = "Mesa 3",
        ["Andrea Domingues"] = "Mesa 3",
        ["Lucas Domingues"] = "Mesa 3",
        ["Gabriela Ribeiro"] = "Mesa 3",
        ["Luzia Bezerra"] = "Mesa 3",
        ["Amanda Carolina"] = "Mesa 3",

        ["Jane Guimarães"] = "Mesa 5",
        ["Renato Medeiros"] = "Mesa 5",
        ["Dulce Maria"] = "Mesa 5",
        ["Daniel Fernandes"] = "Mesa 5",
        ["Rosilea Maria"] = "Mesa 5",
        ["João Pedro"] = "Mesa 5",

        ["Shirley Matias"] = "Mesa 7",
        ["Nelson Alves"] = "Mesa 7",
        ["Nelson Matias"] = "Mesa 7",
        ["Hosana Brevilato"] = "Mesa 7",
        ["Rayane Barbosa"] = "Mesa 7",
        ["Maria Rosa"] = "Mesa 7",

        ["Claudia Rosa"] = "Mesa 9",
        ["Pe Josinaldo Otaciano"] = "Mesa 9",
        ["Wilson Santana"] = "Mesa 9",
        ["Arthur Santana"] = "Mesa 9",
        ["Julio Minto"] = "Mesa 9",
        ["Eloana Minto"] = "Mesa 9",

        ["Rodolfo Alves"] = "Mesa 11",
        ["Luciane de Souza"] = "Mesa 11",
        ["Emanuelle de Souza"] = "Mesa 11",
        ["Bernardo de Souza"] = "Mesa 11",
        ["Theo de Souza"] = "Mesa 11",
        ["Robert Silva"] = "Mesa 11",

        ["Afonso Bicchieri"] = "Mesa 13",
        ["Laís Xavier"] = "Mesa 13",
        ["Clesia Eleonora"] = "Mesa 13",
        ["Lucas Xavier"] = "Mesa 13",
        ["Katia Verônica"] = "Mesa 13",
        ["Gabriel Ferreira"] = "Mesa 13",

        ["Hannah Fonseca"] = "Mesa 15",
        ["Manuella Ferreira"] = "Mesa 15",
        ["Flávia Caetano"] = "Mesa 15",
        ["Vitor França"] = "Mesa 15",
        ["Clesley Silva"] = "Mesa 15",
        ["Karen Lucia"] = "Mesa 15",

        ["Natália Dias"] = "Mesa 17",
        ["Vinicius Condina"] = "Mesa 17",
        ["Rogerio Navarro"] = "Mesa 17",
        ["Isis Malater"] = "Mesa 17",

        ["Jessica Franco"] = "Mesa 19",
        ["Kleber Alves"] = "Mesa 19",
        ["Lorenna Franco"] = "Mesa 19",
        ["Cecília Araujo"] = "Mesa 19",
        ["Lucas Moreira"] = "Mesa 19"
    };

    private readonly IEventoRepository _repo;
    private readonly ILogger<RelatorioService> _logger;
    private readonly IRelatorioStrategy _relatorioPdfStrategy;
    private readonly IListaFinalConfirmadosPdfStrategy _listaFinalConfirmadosPdfStrategy;
    private readonly IRelacaoPessoaMesaPdfStrategy _relacaoPessoaMesaPdfStrategy;

    public RelatorioService(
        IEventoRepository repo,
        ILogger<RelatorioService> logger,
        IRelatorioStrategy relatorioPdfStrategy,
        IListaFinalConfirmadosPdfStrategy listaFinalConfirmadosPdfStrategy,
        IRelacaoPessoaMesaPdfStrategy relacaoPessoaMesaPdfStrategy)
    {
        _repo = repo;
        _logger = logger;
        _relatorioPdfStrategy = relatorioPdfStrategy;
        _listaFinalConfirmadosPdfStrategy = listaFinalConfirmadosPdfStrategy;
        _relacaoPessoaMesaPdfStrategy = relacaoPessoaMesaPdfStrategy;
    }

    public async Task<RelatorioEventoResponse> ObterRelatorioAsync()
    {
        try
        {
            _logger.LogInformation("[ObterRelatorio] Requisição de relatório recebida.");

            var convidados = await _repo.ObterConvidadosConfirmadosAsync();

            var itens = new List<ConvidadoRelatorioItem>(convidados.Count);

            foreach (var c in convidados)
            {
                itens.Add(new ConvidadoRelatorioItem(
                    c.Nome,
                    c.Acompanhantes.Select(a => a.Nome).ToList()
                ));
            }

            var totalPessoas = await _repo.ObterTotalPessoasAsync();

            _logger.LogInformation("[ObterRelatorio] Relatório gerado | Convidados confirmados: {TotalConvidados} | Total de pessoas: {TotalPessoas}",
                itens.Count, totalPessoas);

            return new RelatorioEventoResponse(200, "Relatório gerado com sucesso.", itens, totalPessoas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ObterRelatorio] Erro inesperado ao gerar o relatório.");
            return new RelatorioEventoResponse(500, "Ocorreu um erro interno. Tente novamente mais tarde.", [], 0);
        }
    }

    public async Task<ListaFinalConfirmadosResponse> ObterListaFinalConfirmadosAsync()
    {
        try
        {
            _logger.LogInformation("[ObterListaFinalConfirmados] Requisição da lista final recebida.");

            var convidados = await _repo.ObterConvidadosConfirmadosAsync();

            var confirmados = convidados
                .SelectMany(c => new[] { c.Nome }.Concat(c.Acompanhantes.Select(a => a.Nome)))
                .OrderBy(nome => nome)
                .Select((nome, indice) => new ListaFinalConfirmadoItem(indice + 1, nome, ObterMesaPorNome(nome), null))
                .ToList();

            var mensagem = confirmados.Count == 0
                ? "Nenhum confirmado encontrado."
                : string.Empty;

            _logger.LogInformation("[ObterListaFinalConfirmados] Lista final gerada | Total de pessoas: {TotalPessoas}", confirmados.Count);

            return new ListaFinalConfirmadosResponse(200, mensagem, confirmados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ObterListaFinalConfirmados] Erro inesperado ao gerar lista final.");
            return new ListaFinalConfirmadosResponse(500, "Ocorreu um erro interno. Tente novamente mais tarde.", []);
        }
    }

    public async Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarPdfAsync()
    {
        var relatorio = await ObterRelatorioAsync();
        var bytes = await _relatorioPdfStrategy.ExportarAsync(relatorio);
        return (bytes, _relatorioPdfStrategy.ContentType, _relatorioPdfStrategy.NomeArquivo);
    }

    public async Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarListaFinalConfirmadosPdfAsync()
    {
        var response = await ObterListaFinalConfirmadosAsync();
        var bytes = await _listaFinalConfirmadosPdfStrategy.ExportarAsync(response);
        return (bytes, _listaFinalConfirmadosPdfStrategy.ContentType, _listaFinalConfirmadosPdfStrategy.NomeArquivo);
    }

    public async Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarRelacaoPessoaMesaPdfAsync()
    {
        var response = await ObterListaFinalConfirmadosAsync();
        var bytes = await _relacaoPessoaMesaPdfStrategy.ExportarAsync(response);
        return (bytes, _relacaoPessoaMesaPdfStrategy.ContentType, _relacaoPessoaMesaPdfStrategy.NomeArquivo);
    }

    private static string? ObterMesaPorNome(string nome)
    {
        var nomeBancoNormalizado = NormalizarNome(nome);

        foreach (var item in MapaMesaPorNome)
        {
            if (nomeBancoNormalizado.Equals(NormalizarNome(item.Key), StringComparison.Ordinal))
                return item.Value;
        }

        foreach (var item in MapaMesaPorNome)
        {
            if (nomeBancoNormalizado.Contains(NormalizarNome(item.Key), StringComparison.Ordinal))
                return item.Value;
        }

        foreach (var item in MapaMesaPorNome)
        {
            if (CorrespondePorTermos(nomeBancoNormalizado, item.Key))
                return item.Value;
        }

        return null;
    }

    private static bool CorrespondePorTermos(string nomeBancoNormalizado, string nomeMapeado)
    {
        var termosMapeados = NormalizarNome(nomeMapeado)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (termosMapeados.Length == 0)
            return false;

        return termosMapeados.All(termo => nomeBancoNormalizado.Contains(termo, StringComparison.Ordinal));
    }

    private static string NormalizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return string.Empty;

        var textoNormalizado = nome.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(textoNormalizado.Length);

        foreach (var caractere in textoNormalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(caractere);
            if (categoria != UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLowerInvariant(caractere));
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
