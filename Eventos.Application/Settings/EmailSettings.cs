namespace Eventos.Application.Settings;

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string Remetente { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Destinatario { get; set; } = string.Empty;
}
