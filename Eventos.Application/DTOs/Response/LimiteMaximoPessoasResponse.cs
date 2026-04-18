namespace Eventos.Application.DTOs.Response;

/// <summary>
/// Resposta com o limite máximo de pessoas permitido no evento.
/// </summary>
public class LimiteMaximoPessoasResponse : BaseResponse
{
    /// <summary>
    /// Limite máximo de pessoas confirmadas no evento.
    /// </summary>
    /// <example>105</example>
    public int LimiteMaximoPessoas { get; set; }

    public LimiteMaximoPessoasResponse(int codigoStatus, int limiteMaximoPessoas)
        : base(codigoStatus, string.Empty)
    {
        LimiteMaximoPessoas = limiteMaximoPessoas;
    }
}
