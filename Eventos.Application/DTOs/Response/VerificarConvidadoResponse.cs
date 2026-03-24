namespace Eventos.Application.DTOs.Response;

/// <summary>
/// Resposta da consulta de existência de um convidado
/// </summary>
public class VerificarConvidadoResponse : BaseResponse
{
    /// <summary>
    /// Indica se o nome foi encontrado como convidado principal
    /// </summary>
    /// <example>true</example>
    public bool ExisteComoConvidado { get; set; }

    /// <summary>
    /// Indica se o nome foi encontrado como acompanhante
    /// </summary>
    /// <example>false</example>
    public bool ExisteComoAcompanhante { get; set; }

    public VerificarConvidadoResponse(int codigoStatus, string mensagem, bool existeComoConvidado, bool existeComoAcompanhante)
        : base(codigoStatus, mensagem)
    {
        ExisteComoConvidado = existeComoConvidado;
        ExisteComoAcompanhante = existeComoAcompanhante;
    }
}
