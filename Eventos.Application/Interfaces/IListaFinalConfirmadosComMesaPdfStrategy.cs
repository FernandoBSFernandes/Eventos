using Eventos.Application.DTOs.Response;

namespace Eventos.Application.Interfaces;

public interface IListaFinalConfirmadosComMesaPdfStrategy
{
    string ContentType { get; }
    string NomeArquivo { get; }
    Task<byte[]> ExportarAsync(ListaFinalConfirmadosResponse listaFinal);
}
