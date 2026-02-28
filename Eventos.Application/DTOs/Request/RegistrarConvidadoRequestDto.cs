using System.Text.Json.Serialization;

namespace Eventos.Application.DTOs.Request
{
    public class RegistrarConvidadoRequestDto
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("iraAoRodizio")]
        public bool PresencaConfirmada { get; set; }

        [JsonPropertyName("participacao")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Participacao Participacao { get; set; }

        public int QuantidadeAcompanhantes { get; set; }

        public List<string> NomesAcompanhantes { get; set; }

        public RegistrarConvidadoRequestDto(string nome, bool presencaConfirmada, Participacao participacao, int quantidadeAcompanhantes, List<string> nomesAcompanhantes)
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
