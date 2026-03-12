namespace Eventos.Application.DTOs.Response
{
    /// <summary>
    /// Resposta da consulta de existência de um convidado
    /// </summary>
    public class VerificarConvidadoResponse : BaseResponse
    {
        /// <summary>
        /// Indica se o convidado foi encontrado na base de dados
        /// </summary>
        /// <example>true</example>
        public bool Existe { get; set; }

        public VerificarConvidadoResponse(int codigoStatus, string mensagem, bool existe)
            : base(codigoStatus, mensagem)
        {
            Existe = existe;
        }
    }
}
