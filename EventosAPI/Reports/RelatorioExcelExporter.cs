using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;

namespace EventosAPI.Reports;

public class RelatorioExcelExporter : IRelatorioExporter
{
    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string NomeArquivo => "Relação de Participantes do Rodizio.xlsx";

    public Task<byte[]> ExportarAsync(RelatorioEventoResponse relatorio)
        => RelatorioExcelGenerator.GerarAsync(relatorio);
}
