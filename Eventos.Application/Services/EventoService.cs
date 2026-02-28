using Eventos.Application.DTOs.Request;
using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using Eventos.Domain.Entities;
using Eventos.Domain.Repositories;

namespace Eventos.Application.Services;

public class EventoService : IEventoService
{
    private readonly IEventoRepository _repo;

    public EventoService(IEventoRepository repo)
    {
        _repo = repo;
    }

    public async Task<BaseResponse> AdicionarConvidadoAsync(AdicionarConvidadoRequest request)
    {
        try
        {
            // Validar dados do convidado
            ValidarConvidado(request);

            // Preparar o objeto para persist�ncia
            var convidado = new Convidado
            {
                Nome = request.Nome,
                PresencaConfirmada = request.PresencaConfirmada,
                Participacao = request.Participacao.ToString(),
                QuantidadeAcompanhantes = request.QuantidadeAcompanhantes,
                Acompanhantes = request.Participacao.ToString() == "Sozinho" 
                    ? new List<Acompanhante>()
                    : request.NomesAcompanhantes?
                        .Select(nome => new Acompanhante { Nome = nome })
                        .ToList() ?? new List<Acompanhante>()
            };

            // Persistir na base de dados
            await _repo.AdicionarConvidadoAsync(convidado);

            // Retornar sucesso com c�digo 201
            return new BaseResponse(201, "Convidado foi registrado com sucesso");
        }
        catch (ArgumentException ex)
        {
            // Erro de valida��o - retornar c�digo 400
            return new BaseResponse(400, ex.Message);
        }
        catch (Exception ex)
        {
            // Outro tipo de erro - retornar c�digo 500
            return new BaseResponse(500, $"Ocorreu um erro ao adicionar o convidado: {ex.Message}");
        }
    }

    private void ValidarConvidado(AdicionarConvidadoRequest request)
    {
        if (request == null)
            throw new ArgumentException("Dados do convidado s�o obrigat�rios");

        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new ArgumentException("O nome do convidado � obrigat�rio");

        if (request.Nome.Length < 3 || request.Nome.Length > 50)
            throw new ArgumentException("O nome deve ter entre 3 e 50 caracteres");

        if (request.QuantidadeAcompanhantes < 0 || request.QuantidadeAcompanhantes > 5)
            throw new ArgumentException("A quantidade de acompanhantes n�o pode ser negativa ou superior a 5");

        if (request.Participacao.ToString() == "Sozinho" && request.QuantidadeAcompanhantes > 0)
            throw new ArgumentException("Convidado que vai sozinho n�o pode ter acompanhantes");

        var quantidadeNomesAcompanhantes = request.NomesAcompanhantes?.Count ?? 0;

        if (request.QuantidadeAcompanhantes != quantidadeNomesAcompanhantes)
            throw new ArgumentException("A quantidade de acompanhantes deve ser igual � quantidade de nomes informados");

        if (request.NomesAcompanhantes != null && request.NomesAcompanhantes.Any(nome => string.IsNullOrWhiteSpace(nome)))
            throw new ArgumentException("Os nomes dos acompanhantes n�o podem estar vazios");

        if (request.NomesAcompanhantes != null && request.NomesAcompanhantes.Any(nome => nome.Length < 3 || nome.Length > 50))
            throw new ArgumentException("O nome de cada acompanhante deve ter entre 3 e 50 caracteres");
    }
}
