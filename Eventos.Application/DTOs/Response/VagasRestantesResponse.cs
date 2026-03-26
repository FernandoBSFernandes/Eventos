namespace Eventos.Application.DTOs.Response;

/// <summary>
/// Resposta com a quantidade de vagas restantes no evento
/// </summary>
public class VagasRestantesResponse : BaseResponse
{
    /// <summary>
    /// Quantidade de vagas restantes no evento
    /// </summary>
    /// <example>37</example>
    public int VagasRestantes { get; set; }

    public VagasRestantesResponse(int codigoStatus, int vagasRestantes)
        : base(codigoStatus, string.Empty)
    {
        VagasRestantes = vagasRestantes;
    }
}
