using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Eventos.Application.DTOs.Request
{
    /// <summary>
    /// Dados necessários para registrar um novo convidado no evento
    /// </summary>
    public class AdicionarConvidadoRequest
    {
        /// <summary>
        /// Nome completo do convidado
        /// </summary>
        /// <example>João Silva</example>
        [JsonPropertyName("nome")]
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Nome { get; set; }

        /// <summary>
        /// Indica se o convidado confirmou presença no evento
        /// </summary>
        /// <example>true</example>
        [JsonPropertyName("iraAoRodizio")]
        public bool PresencaConfirmada { get; set; }

        /// <summary>
        /// Indica se o convidado virá sozinho ou acompanhado
        /// </summary>
        /// <example>Acompanhado</example>
        [JsonPropertyName("participacao")]
        public Participacao Participacao { get; set; }

        /// <summary>
        /// Quantidade de acompanhantes. Deve ser 0 quando a participação for Sozinho.
        /// </summary>
        /// <example>2</example>
        [Range(0, 5)]
        public int QuantidadeAcompanhantes { get; set; }

        /// <summary>
        /// Nomes dos acompanhantes. A quantidade deve ser igual a QuantidadeAcompanhantes. Cada nome deve ter entre 3 e 50 caracteres.
        /// </summary>
        /// <example>["Ana Costa", "Pedro Costa"]</example>
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

    /// <summary>
    /// Forma de participação do convidado no evento
    /// </summary>
    public enum Participacao
    {
        /// <summary>O convidado virá sem acompanhantes</summary>
        Sozinho,
        /// <summary>O convidado virá com um ou mais acompanhantes</summary>
        Acompanhado
    }
}
