namespace Eventos.Application.Configuration;

/// <summary>
/// Configurações do evento
/// </summary>
public class EventoConfiguration
{
    public const string SectionName = "Evento";

    /// <summary>
    /// Limite máximo de pessoas permitidas no evento (convidados confirmados + acompanhantes).
    /// Configurado em appsettings.json na seção "Evento:LimiteMaximoPessoas".
    /// </summary>
    public int LimiteMaximoPessoas { get; set; }
}
