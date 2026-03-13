namespace Eventos.Application.Interfaces;

/// <summary>
/// Serviço responsável por enviar o relatório de convidados confirmados por e-mail
/// </summary>
public interface IRelatorioEmailService
{
    /// <summary>
    /// Gera os arquivos PDF e Excel com a lista de convidados confirmados e os envia por e-mail
    /// </summary>
    Task EnviarRelatorioAsync();
}
