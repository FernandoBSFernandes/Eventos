namespace Eventos.Application.Interfaces;

public interface IMigracaoDadosService
{
    Task<(int convidadosMigrados, int acompanhantesMigrados)> MigrarDadosAsync();
}
