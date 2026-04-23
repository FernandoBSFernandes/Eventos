using Eventos.Application.DTOs.Response;

namespace Eventos.Application.Interfaces;

public interface IRelatorioService
{
    Task<RelatorioEventoResponse> ObterRelatorioAsync();
    Task<ListaFinalConfirmadosResponse> ObterListaFinalConfirmadosAsync();
    Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarPdfAsync();
    Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarListaFinalConfirmadosPdfAsync();
}
