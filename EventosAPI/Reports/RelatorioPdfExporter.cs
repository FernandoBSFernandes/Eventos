using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;

namespace EventosAPI.Reports;

public class RelatorioPdfExporter : IRelatorioExporter
{
    public string ContentType => "application/pdf";
    public string NomeArquivo => "Relação de Participantes do Rodizio.pdf";

    public Task<byte[]> ExportarAsync(RelatorioEventoResponse relatorio)
        => RelatorioPdfGenerator.GerarAsync(relatorio);
}
