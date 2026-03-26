using Eventos.Application.DTOs.Response;

namespace Eventos.Application.Interfaces;

public interface IRelatorioStrategy
{
    string ContentType { get; }
    string NomeArquivo { get; }
    Task<byte[]> ExportarAsync(RelatorioEventoResponse relatorio);
}
