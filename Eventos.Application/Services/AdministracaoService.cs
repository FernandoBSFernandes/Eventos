using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using Eventos.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Eventos.Application.Services;

public class AdministracaoService : IAdministracaoService
{
    private readonly IEventoRepository _repo;
    private readonly ILogger<AdministracaoService> _logger;

    public AdministracaoService(IEventoRepository repo, ILogger<AdministracaoService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<BaseResponse> ZerarTabelasAsync()
    {
        try
        {
            _logger.LogInformation("[ZerarTabelas] Requisição para zerar as tabelas recebida.");

            await _repo.ZerarTabelasAsync();

            _logger.LogInformation("[ZerarTabelas] Tabelas zeradas com sucesso.");

            return new BaseResponse(200, "Tabelas zeradas com sucesso.");
        }
        catch (Exception ex)
        {
            return new BaseResponse(500, $"Ocorreu um erro ao zerar as tabelas: {ex.Message}");
        }
    }

    public async Task<BaseResponse> RemoverDuplicatasAsync()
    {
        try
        {
            _logger.LogInformation("[RemoverDuplicatas] Requisição para remover duplicatas recebida.");

            var (convidadosRemovidos, acompanhantesRemovidos) = await _repo.RemoverDuplicatasAsync();

            _logger.LogInformation(
                "[RemoverDuplicatas] Duplicatas removidas | Convidados: {Convidados} | Acompanhantes: {Acompanhantes}",
                convidadosRemovidos, acompanhantesRemovidos);

            return new BaseResponse(200,
                $"Duplicatas removidas com sucesso. Convidados removidos: {convidadosRemovidos}. Acompanhantes removidos: {acompanhantesRemovidos}.");
        }
        catch (Exception ex)
        {
            return new BaseResponse(500, $"Ocorreu um erro ao remover duplicatas: {ex.Message}");
        }
    }
}
