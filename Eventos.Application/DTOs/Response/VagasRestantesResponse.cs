namespace Eventos.Application.DTOs.Response;

/// <summary>
/// Resposta com a quantidade de vagas restantes no evento
/// </summary>
public class VagasRestantesResponse : BaseResponse
{
    /// <summary>
    /// Quantidade de vagas restantes (limite de 100 menos pessoas confirmadas)
    /// </summary>
    /// <example>37</example>
    public int VagasRestantes { get; set; }

    /// <summary>
    /// Total de pessoas já confirmadas (convidados + acompanhantes)
    /// </summary>
    /// <example>63</example>
    public int PessoasConfirmadas { get; set; }

    public VagasRestantesResponse(int codigoStatus, string mensagem, int vagasRestantes, int pessoasConfirmadas)
        : base(codigoStatus, mensagem)
    {
        VagasRestantes = vagasRestantes;
        PessoasConfirmadas = pessoasConfirmadas;
    }
}
