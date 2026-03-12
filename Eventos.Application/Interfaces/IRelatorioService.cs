using Eventos.Application.DTOs.Response;

namespace Eventos.Application.Interfaces;

public interface IRelatorioService
{
    Task<RelatorioEventoResponse> ObterRelatorioAsync();
    Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarAsync(IRelatorioExporter exporter);
}
