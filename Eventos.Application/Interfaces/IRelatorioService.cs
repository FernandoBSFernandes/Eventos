using Eventos.Application.DTOs.Response;
using Eventos.Application.Enums;

namespace Eventos.Application.Interfaces;

public interface IRelatorioService
{
    Task<RelatorioEventoResponse> ObterRelatorioAsync();
    Task<ListaFinalConfirmadosResponse> ObterListaFinalConfirmadosAsync();
    Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarAsync(FormatoRelatorio formato);
    Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarListaFinalConfirmadosPdfAsync();
    Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarListaFinalConfirmadosComMesaPdfAsync();
}
