using System.Net.Mail;

namespace Eventos.Application.Interfaces;

public interface ISmtpClientWrapper
{
    Task EnviarAsync(MailMessage message);
}
