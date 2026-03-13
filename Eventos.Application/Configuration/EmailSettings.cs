namespace Eventos.Application.Configuration;

/// <summary>
/// Configurações de SMTP para envio de e-mail
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string Remetente { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Destinatario { get; set; } = string.Empty;
}
