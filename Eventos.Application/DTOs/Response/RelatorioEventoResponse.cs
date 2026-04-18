namespace Eventos.Application.DTOs.Response;

/// <summary>
/// Resposta do relatório de convidados confirmados
/// </summary>
public class RelatorioEventoResponse : BaseResponse
{
    /// <summary>
    /// Lista de convidados confirmados e seus respectivos acompanhantes
    /// </summary>
    public List<ConvidadoRelatorioItem> Convidados { get; set; }

    /// <summary>
    /// Total de pessoas confirmadas, contando convidados e acompanhantes
    /// </summary>
    /// <example>8</example>
    public int TotalPessoas { get; set; }

    /// <summary>
    /// Total de pessoas confirmadas (mesmo valor de <see cref="TotalPessoas"/>), exposto para manter o mesmo contrato lógico do endpoint de vagas.
    /// </summary>
    /// <example>8</example>
    public int PessoasConfirmadas => TotalPessoas;

    public RelatorioEventoResponse(int codigoStatus, string mensagem, List<ConvidadoRelatorioItem> convidados, int totalPessoas)
        : base(codigoStatus, mensagem)
    {
        Convidados = convidados;
        TotalPessoas = totalPessoas;
    }
}

/// <summary>
/// Item do relatório representando um convidado confirmado e seus acompanhantes
/// </summary>
public class ConvidadoRelatorioItem
{
    /// <summary>
    /// Nome do convidado confirmado
    /// </summary>
    /// <example>João Silva</example>
    public string Nome { get; set; }

    /// <summary>
    /// Nomes dos acompanhantes do convidado
    /// </summary>
    /// <example>["Ana Silva", "Pedro Silva"]</example>
    public List<string> Acompanhantes { get; set; }

    public ConvidadoRelatorioItem(string nome, List<string> acompanhantes)
    {
        Nome = nome;
        Acompanhantes = acompanhantes;
    }
}
