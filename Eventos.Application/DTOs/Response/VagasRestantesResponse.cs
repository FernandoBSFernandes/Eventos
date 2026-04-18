namespace Eventos.Application.DTOs.Response;

/// <summary>
/// Resposta com a quantidade de vagas restantes no evento
/// </summary>
public class VagasRestantesResponse : BaseResponse
{
    /// <summary>
    /// Quantidade total de pessoas confirmadas no evento.
    /// </summary>
    /// <example>68</example>
    public int PessoasConfirmadas { get; set; }

    /// <summary>
    /// Quantidade de vagas restantes no evento
    /// </summary>
    /// <example>37</example>
    public int VagasRestantes { get; set; }

    public VagasRestantesResponse(int codigoStatus, int pessoasConfirmadas, int vagasRestantes)
        : base(codigoStatus, string.Empty)
    {
        PessoasConfirmadas = pessoasConfirmadas;
        VagasRestantes = vagasRestantes;
    }
}
