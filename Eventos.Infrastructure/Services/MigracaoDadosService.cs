using Eventos.Application.Interfaces;
using Eventos.Domain.Entities;
using Eventos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Eventos.Infrastructure.Services;

public class MigracaoDadosService : IMigracaoDadosService
{
    private readonly OrigemDbContext _origem;
    private readonly EventosDbContext _destino;
    private readonly ILogger<MigracaoDadosService> _logger;

    public MigracaoDadosService(
        OrigemDbContext origem,
        EventosDbContext destino,
        ILogger<MigracaoDadosService> logger)
    {
        _origem = origem;
        _destino = destino;
        _logger = logger;
    }

    public async Task<(int convidadosMigrados, int acompanhantesMigrados)> MigrarDadosAsync()
    {
        _logger.LogInformation("[MigracaoDados] Iniciando migração de dados.");

        List<Convidado> convidadosOrigem;
        try
        {
            convidadosOrigem = await _origem.Convidado
                .AsNoTracking()
                .Include(c => c.Acompanhantes)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MigracaoDados] Falha ao ler dados da base de origem. Migração abortada.");
            throw;
        }

        _logger.LogInformation("[MigracaoDados] {Total} convidado(s) encontrado(s) na base de origem.", convidadosOrigem.Count);

        await using var transaction = await _destino.Database.BeginTransactionAsync();
        try
        {
            var nomesJaExistentes = await _destino.Convidado
                .AsNoTracking()
                .Select(c => c.Nome.ToLower())
                .ToHashSetAsync();

            var novosConvidados = new List<Convidado>();

            foreach (var convidado in convidadosOrigem)
            {
                var nomeConvidadoNormalizado = NormalizarNome(convidado.Nome);

                if (nomesJaExistentes.Contains(nomeConvidadoNormalizado.ToLower()))
                {
                    _logger.LogDebug("[MigracaoDados] Convidado '{Nome}' já existe no destino, ignorando.", nomeConvidadoNormalizado);
                    continue;
                }

                novosConvidados.Add(new Convidado
                {
                    Nome = nomeConvidadoNormalizado,
                    PresencaConfirmada = convidado.PresencaConfirmada,
                    Participacao = convidado.Participacao,
                    QuantidadeAcompanhantes = convidado.QuantidadeAcompanhantes,
                    Acompanhantes = convidado.Acompanhantes
                        .Select(a => new Acompanhante { Nome = NormalizarNome(a.Nome) })
                        .ToList()
                });
            }

            if (novosConvidados.Count == 0)
            {
                _logger.LogInformation("[MigracaoDados] Nenhum dado novo para migrar.");
                return (0, 0);
            }

            await _destino.Convidado.AddRangeAsync(novosConvidados);
            await _destino.SaveChangesAsync();
            await transaction.CommitAsync();

            var totalAcompanhantes = novosConvidados.Sum(c => c.Acompanhantes.Count);

            _logger.LogInformation(
                "[MigracaoDados] Migração concluída | Convidados: {Convidados} | Acompanhantes: {Acompanhantes}",
                novosConvidados.Count, totalAcompanhantes);

            return (novosConvidados.Count, totalAcompanhantes);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "[MigracaoDados] Falha ao gravar dados na base de destino. Rollback efetuado. Migração abortada.");
            throw;
        }
    }

    private static string NormalizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return nome;

        return Regex.Replace(nome, @" {2,}", " ").TrimEnd();
    }
}
