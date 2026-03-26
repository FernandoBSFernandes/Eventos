using Eventos.Application.DTOs.Response;
using Eventos.Application.Enums;
using Eventos.Application.Enums;

namespace Eventos.Application.Interfaces;

public interface IRelatorioService
{
    Task<RelatorioEventoResponse> ObterRelatorioAsync();
    Task<(byte[] bytes, string contentType, string nomeArquivo)> ExportarAsync(FormatoRelatorio formato);
}
