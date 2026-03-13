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

            var itens = new List<ConvidadoRelatorioItem>(convidados.Count);
            var totalPessoas = 0;

            foreach (var c in convidados)
            {
                itens.Add(new ConvidadoRelatorioItem(
                    c.Nome,
                    c.Acompanhantes.Select(a => a.Nome).ToList()
                ));

                totalPessoas += 1 + c.Acompanhantes.Count;
            }

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

    public async Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarAsync(IRelatorioExporter exporter)
    {
        var relatorio = await ObterRelatorioAsync();
        var bytes = await exporter.ExportarAsync(relatorio);
        return (bytes, exporter.ContentType, exporter.NomeArquivo);
    }
}
