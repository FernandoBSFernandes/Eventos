namespace Eventos.Application.DTOs.Response;

/// <summary>
/// Item da listagem de convidados cadastrados no evento
/// </summary>
public class ConvidadoItem
{
    /// <summary>
    /// Nome completo do convidado
    /// </summary>
    /// <example>Maria Santos</example>
    public string Nome { get; set; }

    /// <summary>
    /// Indica se o convidado confirmou presença no evento
    /// </summary>
    /// <example>true</example>
    public bool PresencaConfirmada { get; set; }

    /// <summary>
    /// Forma de participação do convidado: Sozinho ou Acompanhado
    /// </summary>
    /// <example>Acompanhado</example>
    public string Participacao { get; set; }

    /// <summary>
    /// Quantidade de acompanhantes registrados
    /// </summary>
    /// <example>2</example>
    public int QuantidadeAcompanhantes { get; set; }

    /// <summary>
    /// Nomes dos acompanhantes do convidado
    /// </summary>
    /// <example>["Ana Costa", "Pedro Costa"]</example>
    public List<string> NomesAcompanhantes { get; set; }

    public ConvidadoItem(string nome, bool presencaConfirmada, string participacao, int quantidadeAcompanhantes, List<string> nomesAcompanhantes)
    {
        Nome = nome;
        PresencaConfirmada = presencaConfirmada;
        Participacao = participacao;
        QuantidadeAcompanhantes = quantidadeAcompanhantes;
        NomesAcompanhantes = nomesAcompanhantes;
    }
}

/// <summary>
/// Resposta da listagem de convidados
/// </summary>
public class ListarConvidadosResponse : BaseResponse
{
    /// <summary>
    /// Lista de convidados cadastrados
    /// </summary>
    public List<ConvidadoItem> Convidados { get; set; }

    public ListarConvidadosResponse(int codigoStatus, string mensagem, List<ConvidadoItem> convidados)
        : base(codigoStatus, mensagem)
    {
        Convidados = convidados;
    }
}
