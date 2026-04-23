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
        [NormalizarNome("Lucas Fernandes")] = "Mesa 1",
        [NormalizarNome("Giselda Barros")] = "Mesa 1",
        [NormalizarNome("Flávia Fernandes")] = "Mesa 1",
        [NormalizarNome("Gustavo Oliveira")] = "Mesa 1",
        [NormalizarNome("Jane Fernandes")] = "Mesa 1",
        [NormalizarNome("William Danilo")] = "Mesa 1",

        [NormalizarNome("Laércio Fernandes")] = "Mesa 3",
        [NormalizarNome("Andrea Domingues")] = "Mesa 3",
        [NormalizarNome("Lucas Domingues")] = "Mesa 3",
        [NormalizarNome("Gabriela Ribeiro")] = "Mesa 3",
        [NormalizarNome("Luzia Bezerra")] = "Mesa 3",
        [NormalizarNome("Amanda Carolina")] = "Mesa 3",

        [NormalizarNome("Jane Guimarães")] = "Mesa 5",
        [NormalizarNome("Renato Medeiros")] = "Mesa 5",
        [NormalizarNome("Dulce Maria")] = "Mesa 5",
        [NormalizarNome("Daniel Fernandes")] = "Mesa 5",
        [NormalizarNome("Rosilea Maria")] = "Mesa 5",
        [NormalizarNome("João Pedro")] = "Mesa 5",

        [NormalizarNome("Shirley Matias")] = "Mesa 7",
        [NormalizarNome("Nelson Alves")] = "Mesa 7",
        [NormalizarNome("Nelson Matias")] = "Mesa 7",
        [NormalizarNome("Hosana Brevilato")] = "Mesa 7",
        [NormalizarNome("Rayane Barbosa")] = "Mesa 7",
        [NormalizarNome("Maria Rosa")] = "Mesa 7",

        [NormalizarNome("Claudia Rosa")] = "Mesa 9",
        [NormalizarNome("Pe Josinaldo Otaciano")] = "Mesa 9",
        [NormalizarNome("Wilson Santana")] = "Mesa 9",
        [NormalizarNome("Arthur Santana")] = "Mesa 9",
        [NormalizarNome("Julio Minto")] = "Mesa 9",
        [NormalizarNome("Eloana Minto")] = "Mesa 9",

        [NormalizarNome("Rodolfo Alves")] = "Mesa 11",
        [NormalizarNome("Luciane de Souza")] = "Mesa 11",
        [NormalizarNome("Emanuelle de Souza")] = "Mesa 11",
        [NormalizarNome("Bernardo de Souza")] = "Mesa 11",
        [NormalizarNome("Theo de Souza")] = "Mesa 11",
        [NormalizarNome("Robert Silva")] = "Mesa 11",

        [NormalizarNome("Afonso Bicchieri")] = "Mesa 13",
        [NormalizarNome("Laís Xavier")] = "Mesa 13",
        [NormalizarNome("Clesia Eleonora")] = "Mesa 13",
        [NormalizarNome("Lucas Xavier")] = "Mesa 13",
        [NormalizarNome("Clesley Silva")] = "Mesa 13",
        [NormalizarNome("Karen Lucia")] = "Mesa 13",

        [NormalizarNome("Hannah Fonseca")] = "Mesa 15",
        [NormalizarNome("Manuella Ferreira")] = "Mesa 15",
        [NormalizarNome("Flávia Caetano")] = "Mesa 15",
        [NormalizarNome("Vitor França")] = "Mesa 15",

        [NormalizarNome("Natália Dias")] = "Mesa 17",
        [NormalizarNome("Vinicius Condina")] = "Mesa 17",
        [NormalizarNome("Rogerio Navarro")] = "Mesa 17",
        [NormalizarNome("Isis Malater")] = "Mesa 17",

        [NormalizarNome("Jessica Franco")] = "Mesa 19",
        [NormalizarNome("Kleber Alves")] = "Mesa 19",
        [NormalizarNome("Lorenna Franco")] = "Mesa 19",
        [NormalizarNome("Cecília Araujo")] = "Mesa 19",
        [NormalizarNome("Lucas Moreira")] = "Mesa 19"
    };

    private readonly IEventoRepository _repo;
    private readonly ILogger<RelatorioService> _logger;
    private readonly IRelatorioStrategy _relatorioPdfStrategy;
    private readonly IListaFinalConfirmadosPdfStrategy _listaFinalConfirmadosPdfStrategy;

    public RelatorioService(
        IEventoRepository repo,
        ILogger<RelatorioService> logger,
        IRelatorioStrategy relatorioPdfStrategy,
        IListaFinalConfirmadosPdfStrategy listaFinalConfirmadosPdfStrategy)
    {
        _repo = repo;
        _logger = logger;
        _relatorioPdfStrategy = relatorioPdfStrategy;
        _listaFinalConfirmadosPdfStrategy = listaFinalConfirmadosPdfStrategy;
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

    private static string? ObterMesaPorNome(string nome)
    {
        var chave = NormalizarNome(nome);
        return MapaMesaPorNome.GetValueOrDefault(chave);
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
