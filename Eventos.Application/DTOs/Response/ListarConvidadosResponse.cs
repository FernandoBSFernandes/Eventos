namespace Eventos.Application.DTOs.Response;

public class ConvidadoItem
{
    public string Nome { get; set; }
    public bool PresencaConfirmada { get; set; }
    public string Participacao { get; set; }
    public int QuantidadeAcompanhantes { get; set; }
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
