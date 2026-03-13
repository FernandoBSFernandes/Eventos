namespace Eventos.Application.Configuration;

/// <summary>
/// Configurações do evento
/// </summary>
public class EventoConfiguration
{
    public const string SectionName = "Evento";

    /// <summary>
    /// Limite máximo de pessoas permitidas no evento (convidados confirmados + acompanhantes)
    /// </summary>
    public int LimiteMaximoPessoas { get; set; } = 100;
}
