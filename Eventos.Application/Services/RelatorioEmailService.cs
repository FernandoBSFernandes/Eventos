using System.Net.Mail;
using ClosedXML.Excel;
using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using Eventos.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Eventos.Application.Services;

public class RelatorioEmailService : IRelatorioEmailService
{
    private readonly IConvidadoService _convidadoService;
    private readonly ISmtpClientWrapper _smtpClient;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<RelatorioEmailService> _logger;

    public RelatorioEmailService(
        IConvidadoService convidadoService,
        ISmtpClientWrapper smtpClient,
        IOptions<EmailSettings> emailSettings,
        ILogger<RelatorioEmailService> logger)
    {
        _convidadoService = convidadoService;
        _smtpClient = smtpClient;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task EnviarRelatorioConvidadosConfirmadosAsync()
    {
        _logger.LogInformation("[RelatorioEmail] Iniciando envio do relatório de convidados confirmados.");

        var todos = await _convidadoService.ListarConvidadosAsync();
        var confirmados = todos.Where(c => c.PresencaConfirmada).ToList();

        _logger.LogInformation("[RelatorioEmail] Total de confirmados: {Total}", confirmados.Count);

        var pdfBytes = GerarPdf(confirmados);
        var excelBytes = GerarExcel(confirmados);

        var pdfStream = new MemoryStream(pdfBytes);
        var excelStream = new MemoryStream(excelBytes);
        try
        {
            using var mensagem = new MailMessage
            {
                From = new MailAddress(_emailSettings.Remetente),
                Subject = "Relatório de Convidados Confirmados",
                IsBodyHtml = true,
                Body = $"""
                    <html>
                    <body>
                        <p>Olá,</p>
                        <p>Segue em anexo o relatório de convidados confirmados para o evento.</p>
                        <p><strong>Total de confirmados: {confirmados.Count}</strong></p>
                        <br/>
                        <p>Atenciosamente,<br/>Sistema de Eventos</p>
                    </body>
                    </html>
                    """
            };

            mensagem.To.Add(_emailSettings.Destinatario);
            mensagem.Attachments.Add(new Attachment(pdfStream, "convidados_confirmados.pdf", "application/pdf"));
            mensagem.Attachments.Add(new Attachment(excelStream, "convidados_confirmados.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));

            await _smtpClient.EnviarAsync(mensagem);
        }
        finally
        {
            await pdfStream.DisposeAsync();
            await excelStream.DisposeAsync();
        }

        _logger.LogInformation("[RelatorioEmail] E-mail enviado com sucesso para {Destinatario}.", _emailSettings.Destinatario);
    }

    internal static byte[] GerarPdf(List<ConvidadoItem> convidados)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Segoe UI"));

                page.Header().PaddingBottom(10).Column(col =>
                {
                    col.Item().Text("Relatório de Convidados Confirmados")
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken2).AlignCenter();
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(4);
                    });

                    table.Header(header =>
                    {
                        foreach (var titulo in new[] { "Nome", "Participação", "Qtd. Acomp.", "Nomes dos Acompanhantes" })
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                .Text(titulo).Bold().FontColor(Colors.White).AlignCenter();
                        }
                    });

                    for (int i = 0; i < convidados.Count; i++)
                    {
                        var c = convidados[i];
                        var cor = i % 2 == 0 ? Colors.White : Colors.Blue.Lighten5;

                        table.Cell().Background(cor).Padding(5).Text(c.Nome);
                        table.Cell().Background(cor).Padding(5).Text(c.Participacao).AlignCenter();
                        table.Cell().Background(cor).Padding(5).Text(c.QuantidadeAcompanhantes.ToString()).AlignCenter();
                        table.Cell().Background(cor).Padding(5).Text(
                            c.NomesAcompanhantes.Count > 0 ? string.Join(", ", c.NomesAcompanhantes) : "—"
                        );
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Gerado em: ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();
    }

    internal static byte[] GerarExcel(List<ConvidadoItem> convidados)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Convidados Confirmados");

        var titulo = sheet.Range("A1:D1").Merge();
        titulo.Value = "Relatório de Convidados Confirmados";
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 14;
        titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        titulo.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
        titulo.Style.Font.FontColor = XLColor.White;

        sheet.Cell("A2").Value = "Nome";
        sheet.Cell("B2").Value = "Participação";
        sheet.Cell("C2").Value = "Qtd. Acompanhantes";
        sheet.Cell("D2").Value = "Nomes dos Acompanhantes";

        var cabecalho = sheet.Range("A2:D2");
        cabecalho.Style.Font.Bold = true;
        cabecalho.Style.Fill.BackgroundColor = XLColor.FromHtml("#D6E4F0");
        cabecalho.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int linha = 3;
        foreach (var c in convidados)
        {
            sheet.Cell(linha, 1).Value = c.Nome;
            sheet.Cell(linha, 2).Value = c.Participacao;
            sheet.Cell(linha, 3).Value = c.QuantidadeAcompanhantes;
            sheet.Cell(linha, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell(linha, 4).Value = c.NomesAcompanhantes.Count > 0
                ? string.Join(", ", c.NomesAcompanhantes)
                : "—";

            if (linha % 2 == 0)
                sheet.Range(linha, 1, linha, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F7FC");

            linha++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
