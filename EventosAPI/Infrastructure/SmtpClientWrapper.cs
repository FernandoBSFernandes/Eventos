using System.Net;
using System.Net.Mail;
using Eventos.Application.Interfaces;
using Eventos.Application.Settings;
using Microsoft.Extensions.Options;

namespace EventosAPI.Infrastructure;

public class SmtpClientWrapper : ISmtpClientWrapper
{
    private readonly EmailSettings _settings;

    public SmtpClientWrapper(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task EnviarAsync(MailMessage message)
    {
        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            Credentials = new NetworkCredential(_settings.Remetente, _settings.Senha),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
    }
}
