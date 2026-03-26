using Eventos.Application.Configuration;
using Eventos.Application.Enums;
using Eventos.Application.Enums;
using Eventos.Application.Configuration;
using Eventos.Application.Enums;
using Eventos.Application.Enums;
using Eventos.Application.Enums;
using Eventos.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EventosAPI.Services;

public class RelatorioEmailService : IRelatorioEmailService
{
    private readonly IRelatorioService _relatorioService;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<RelatorioEmailService> _logger;

    public RelatorioEmailService(
        IRelatorioService relatorioService,
        IOptions<EmailSettings> emailSettings,
        ILogger<RelatorioEmailService> logger)
    {
        _relatorioService = relatorioService;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task EnviarRelatorioAsync()
    {
        _logger.LogInformation("[RelatorioEmail] Iniciando envio do relatÃ³rio por e-mail.");

        var (pdfBytes, _, pdfNome) = await _relatorioService.ExportarAsync(FormatoRelatorio.Pdf);
        var (excelBytes, _, excelNome) = await _relatorioService.ExportarAsync(FormatoRelatorio.Excel);

        using var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_emailSettings.Remetente, _emailSettings.Senha)
        };

        using var pdfStream = new MemoryStream(pdfBytes);
        using var excelStream = new MemoryStream(excelBytes);
        using var mail = new MailMessage
        {
            From = new MailAddress(_emailSettings.Remetente),
            Subject = "RelatÃ³rio de Convidados Confirmados",
            Body = "Segue em anexo a lista de convidados confirmados em PDF e Excel.",
        };

        mail.To.Add(_emailSettings.Destinatario);
        mail.Attachments.Add(new Attachment(pdfStream, pdfNome, "application/pdf"));
        mail.Attachments.Add(new Attachment(excelStream, excelNome,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));

        await smtpClient.SendMailAsync(mail);

        _logger.LogInformation("[RelatorioEmail] E-mail enviado com sucesso para {Destinatario}.", _emailSettings.Destinatario);
    }
}
