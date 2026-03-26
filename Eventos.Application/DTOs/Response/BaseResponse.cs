using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Eventos.Application.DTOs.Response
{
    /// <summary>
    /// Resposta padrão da API com código de status e mensagem descritiva
    /// </summary>
    public class BaseResponse
    {
        /// <summary>
        /// Código de status HTTP da operação
        /// </summary>
        /// <example>201</example>
        public int CodigoStatus { get; set; }

        /// <summary>
        /// Mensagem descritiva sobre o resultado da operação
        /// </summary>
        /// <example>Convidado foi registrado com sucesso</example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Mensagem { get; set; }

        public BaseResponse(int codigoStatus, string mensagem)
        {
            CodigoStatus = codigoStatus;
            Mensagem = mensagem;
        }
    }
}
