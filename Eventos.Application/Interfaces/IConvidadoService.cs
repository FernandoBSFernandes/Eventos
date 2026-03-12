using Eventos.Application.DTOs.Request;
using Eventos.Application.DTOs.Response;

namespace Eventos.Application.Interfaces;

public interface IConvidadoService
{
    Task<BaseResponse> AdicionarConvidadoAsync(AdicionarConvidadoRequest request);
    Task<VerificarConvidadoResponse> VerificarConvidadoExisteAsync(string nome);
    Task<List<ConvidadoItem>> ListarConvidadosAsync();
    Task<BaseResponse> RemoverConvidadoPorNomeAsync(string nome);
    Task<VagasRestantesResponse> ObterVagasRestantesAsync();
}
