using Eventos.Application.DTOs.Request;
using Eventos.Application.DTOs.Response;
using Eventos.Domain.Entities;

namespace Eventos.Application.Interfaces;

public interface IEventoService
{
    Task<BaseResponse> AdicionarConvidadoAsync(AdicionarConvidadoRequest request);
}
