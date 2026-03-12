using Eventos.Application.DTOs.Response;

namespace Eventos.Application.Interfaces;

public interface IAdministracaoService
{
    Task<BaseResponse> ZerarTabelasAsync();
    Task<BaseResponse> RemoverDuplicatasAsync();
}
