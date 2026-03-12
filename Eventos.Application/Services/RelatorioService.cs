using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Eventos.Application.Services;

public class RelatorioService : IRelatorioService
{
    private readonly IEventoRepository _repo;
    private readonly ILogger<RelatorioService> _logger;

    public RelatorioService(IEventoRepository repo, ILogger<RelatorioService> logger)
    {
        _repo = repo;
        _logger = logger;
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

    public async Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarAsync(IRelatorioExporter exporter)
    {
        var relatorio = await ObterRelatorioAsync();
        var bytes = await exporter.ExportarAsync(relatorio);
        return (bytes, exporter.ContentType, exporter.NomeArquivo);
    }
}
