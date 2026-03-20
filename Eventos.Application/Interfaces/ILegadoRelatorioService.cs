using Eventos.Application.DTOs.Response;

namespace Eventos.Application.Interfaces;

public interface ILegadoRelatorioService
{
    Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarPdfAsync(IRelatorioExporter exporter);
}
