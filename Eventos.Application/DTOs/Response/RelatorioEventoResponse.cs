namespace Eventos.Application.DTOs.Response;

public class RelatorioEventoResponse : BaseResponse
{
    public List<ConvidadoRelatorioItem> Convidados { get; set; }
    public int TotalPessoas { get; set; }

    public RelatorioEventoResponse(int codigoStatus, string mensagem, List<ConvidadoRelatorioItem> convidados, int totalPessoas)
        : base(codigoStatus, mensagem)
    {
        Convidados = convidados;
        TotalPessoas = totalPessoas;
    }
}

public class ConvidadoRelatorioItem
{
    public string Nome { get; set; }
    public List<string> Acompanhantes { get; set; }

    public ConvidadoRelatorioItem(string nome, List<string> acompanhantes)
    {
        Nome = nome;
        Acompanhantes = acompanhantes;
    }
}
