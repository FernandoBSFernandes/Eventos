namespace Eventos.Application.DTOs.Response;

/// <summary>
/// Resposta da lista final de confirmados para controle de pagamento.
/// </summary>
public class ListaFinalConfirmadosResponse : BaseResponse
{
    /// <summary>
    /// Lista de pessoas confirmadas em ordem alfabética.
    /// </summary>
    public List<ListaFinalConfirmadoItem> Confirmados { get; set; }

    public ListaFinalConfirmadosResponse(int codigoStatus, string mensagem, List<ListaFinalConfirmadoItem> confirmados)
        : base(codigoStatus, mensagem)
    {
        Confirmados = confirmados;
    }
}

/// <summary>
/// Item da lista final de confirmados.
/// </summary>
public class ListaFinalConfirmadoItem
{
    /// <summary>
    /// Numeração sequencial para exibição na primeira coluna.
    /// </summary>
    /// <example>1</example>
    public int Numero { get; set; }

    /// <summary>
    /// Nome da pessoa confirmada.
    /// </summary>
    /// <example>Maria Souza</example>
    public string Nome { get; set; }

    /// <summary>
    /// Campo para controle visual de pagamento (checkbox manual).
    /// </summary>
    /// <example>null</example>
    public bool? Pago { get; set; }

    public ListaFinalConfirmadoItem(int numero, string nome, bool? pago)
    {
        Numero = numero;
        Nome = nome;
        Pago = pago;
    }
}
