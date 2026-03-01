using Eventos.Application.DTOs.Request;
using Eventos.Application.DTOs.Response;

namespace Eventos.Application.Interfaces;

public interface IEventoService
{
    Task<BaseResponse> AdicionarConvidadoAsync(AdicionarConvidadoRequest request);
    Task<VerificarConvidadoResponse> VerificarConvidadoExisteAsync(string nome);
}
