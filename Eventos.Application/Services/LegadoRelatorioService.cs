using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Eventos.Application.Services;

[ExcludeFromCodeCoverage]
public class LegadoRelatorioService : ILegadoRelatorioService
{
    private readonly IOrigemRepository _repo;
    private readonly ILogger<LegadoRelatorioService> _logger;

    public LegadoRelatorioService(IOrigemRepository repo, ILogger<LegadoRelatorioService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarPdfAsync(IRelatorioExporter exporter)
    {
        _logger.LogInformation("[LegadoRelatorio] Requisição de relatório da base de origem recebida.");

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

        _logger.LogInformation("[LegadoRelatorio] Relatório gerado | Convidados confirmados: {TotalConvidados} | Total de pessoas: {TotalPessoas}",
            itens.Count, totalPessoas);

        var relatorio = new RelatorioEventoResponse(200, "Relatório gerado com sucesso.", itens, totalPessoas);
        var bytes = await exporter.ExportarAsync(relatorio);

        return (bytes, exporter.ContentType, exporter.NomeArquivo);
    }
}
