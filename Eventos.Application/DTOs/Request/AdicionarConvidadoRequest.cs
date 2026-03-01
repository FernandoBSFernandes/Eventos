using System.Text.Json.Serialization;

namespace Eventos.Application.DTOs.Request
{
    public class AdicionarConvidadoRequest
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("iraAoRodizio")]
        public bool PresencaConfirmada { get; set; }

        [JsonPropertyName("participacao")]
        public Participacao Participacao { get; set; }

        public int QuantidadeAcompanhantes { get; set; }

        public List<string> NomesAcompanhantes { get; set; }

        public AdicionarConvidadoRequest(string nome, bool presencaConfirmada, Participacao participacao, int quantidadeAcompanhantes, List<string> nomesAcompanhantes)
        {
            Nome = nome;
            PresencaConfirmada = presencaConfirmada;
            Participacao = participacao;
            QuantidadeAcompanhantes = quantidadeAcompanhantes;
            NomesAcompanhantes = nomesAcompanhantes;
        }
    }

    public enum Participacao
    {
        Sozinho,
        Acompanhado
    }

}
