using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using Eventos.Application.Services;
using Eventos.Application.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;

namespace Eventos.Tests.Services;

[Trait("Serviço", "RelatorioEmail")]
public class RelatorioEmailServiceTests
{
    private readonly IConvidadoService _convidadoService;
    private readonly ISmtpClientWrapper _smtpClient;
    private readonly RelatorioEmailService _service;

    public RelatorioEmailServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _convidadoService = Substitute.For<IConvidadoService>();
        _smtpClient = Substitute.For<ISmtpClientWrapper>();

        var settings = Options.Create(new EmailSettings
        {
            SmtpHost = "smtp.office365.com",
            SmtpPort = 587,
            Remetente = "test@outlook.com",
            Senha = "senha-teste",
            Destinatario = "dest@outlook.com"
        });

        _service = new RelatorioEmailService(
            _convidadoService,
            _smtpClient,
            settings,
            NullLogger<RelatorioEmailService>.Instance);
    }

    #region Sucesso

    [Fact(DisplayName = "Deve enviar e-mail sem exceção quando há convidados confirmados")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveEnviarEmail_QuandoExistemConvidadosConfirmados()
    {
        // Arrange
        var convidados = new List<ConvidadoItem>
        {
            new ConvidadoItem("João Silva", true, "Acompanhado", 1, new List<string> { "Ana Silva" }),
            new ConvidadoItem("Maria Souza", true, "Sozinho", 0, new List<string>())
        };

        _convidadoService.ListarConvidadosAsync().Returns(convidados);
        _smtpClient.EnviarAsync(Arg.Any<System.Net.Mail.MailMessage>()).Returns(Task.CompletedTask);

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.EnviarRelatorioConvidadosConfirmadosAsync());

        // Assert
        Assert.Null(excecao);
    }

    [Fact(DisplayName = "Deve enviar e-mail sem exceção quando não há convidados confirmados")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveEnviarEmail_QuandoNaoExistemConvidadosConfirmados()
    {
        // Arrange
        var convidados = new List<ConvidadoItem>
        {
            new ConvidadoItem("Pedro Alves", false, "Sozinho", 0, new List<string>())
        };

        _convidadoService.ListarConvidadosAsync().Returns(convidados);
        _smtpClient.EnviarAsync(Arg.Any<System.Net.Mail.MailMessage>()).Returns(Task.CompletedTask);

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.EnviarRelatorioConvidadosConfirmadosAsync());

        // Assert
        Assert.Null(excecao);
    }

    [Fact(DisplayName = "Deve chamar ListarConvidadosAsync exatamente uma vez")]
    [Trait("Categoria", "Sucesso")]
    public async Task DeveChamarListarConvidadosAsync_UmaVez()
    {
        // Arrange
        _convidadoService.ListarConvidadosAsync().Returns(new List<ConvidadoItem>());
        _smtpClient.EnviarAsync(Arg.Any<System.Net.Mail.MailMessage>()).Returns(Task.CompletedTask);

        // Act
        await _service.EnviarRelatorioConvidadosConfirmadosAsync();

        // Assert
        await _convidadoService.Received(1).ListarConvidadosAsync();
    }

    [Fact(DisplayName = "Deve gerar PDF com conteúdo não vazio quando há convidados confirmados")]
    [Trait("Categoria", "Sucesso")]
    public void DeveGerarPdf_ComConvidadosConfirmados()
    {
        // Arrange
        var convidados = new List<ConvidadoItem>
        {
            new ConvidadoItem("João Silva", true, "Acompanhado", 1, new List<string> { "Ana Silva" }),
            new ConvidadoItem("Maria Souza", true, "Sozinho", 0, new List<string>())
        };

        // Act
        var excecao = Record.Exception(() => RelatorioEmailService.GerarPdf(convidados));
        var bytes = RelatorioEmailService.GerarPdf(convidados);

        // Assert
        Assert.Null(excecao);
        Assert.NotEmpty(bytes);
    }

    [Fact(DisplayName = "Deve gerar Excel com conteúdo não vazio quando há convidados confirmados")]
    [Trait("Categoria", "Sucesso")]
    public void DeveGerarExcel_ComConvidadosConfirmados()
    {
        // Arrange
        var convidados = new List<ConvidadoItem>
        {
            new ConvidadoItem("João Silva", true, "Acompanhado", 1, new List<string> { "Ana Silva" }),
            new ConvidadoItem("Maria Souza", true, "Sozinho", 0, new List<string>())
        };

        // Act
        var excecao = Record.Exception(() => RelatorioEmailService.GerarExcel(convidados));
        var bytes = RelatorioEmailService.GerarExcel(convidados);

        // Assert
        Assert.Null(excecao);
        Assert.NotEmpty(bytes);
    }

    #endregion
}
