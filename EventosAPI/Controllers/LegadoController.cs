using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using EventosAPI.Reports;
using Microsoft.AspNetCore.Mvc;

namespace EventosAPI.Controllers;

/// <summary>
/// Relatórios gerados a partir da base de dados de origem (legado).
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Consumes("application/json")]
[Tags("Legado")]
public class LegadoController : ControllerBase
{
    private readonly ILegadoRelatorioService _legadoRelatorioService;

    public LegadoController(ILegadoRelatorioService legadoRelatorioService)
    {
        _legadoRelatorioService = legadoRelatorioService;
    }

    /// <summary>
    /// Exporta o relatório de convidados confirmados da base de origem em formato PDF
    /// </summary>
    /// <returns>Arquivo PDF com a relação de participantes e seus acompanhantes da base legada</returns>
    /// <response code="200">Arquivo gerado com sucesso</response>
    /// <response code="500">Erro interno ao processar a requisição</response>
    [HttpGet("relatorio/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ObterRelatorioPdf()
    {
        try
        {
            var (bytes, contentType, nomeArquivo) = await _legadoRelatorioService.ExportarPdfAsync(new RelatorioPdfExporter());
            return File(bytes, contentType, nomeArquivo);
        }
        catch (Exception)
        {
            return StatusCode(500, new BaseResponse(500, "Ocorreu um erro interno ao gerar o relatório da base de origem."));
        }
    }
}
