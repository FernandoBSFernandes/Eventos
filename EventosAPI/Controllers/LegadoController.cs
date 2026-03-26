using Eventos.Application.DTOs.Response;
using Eventos.Application.DTOs.Response;
using Eventos.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace EventosAPI.Controllers;

/// <summary>
/// RelatÃ³rios gerados a partir da base de dados de origem (legado).
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Consumes("application/json")]
[Tags("Legado")]
[ExcludeFromCodeCoverage]
public class LegadoController : ControllerBase
{
    private readonly ILegadoRelatorioService _legadoRelatorioService;

    public LegadoController(ILegadoRelatorioService legadoRelatorioService)
    {
        _legadoRelatorioService = legadoRelatorioService;
    }

    /// <summary>
    /// Exporta o relatÃ³rio de convidados confirmados da base de origem em formato PDF
    /// </summary>
    /// <returns>Arquivo PDF com a relaÃ§Ã£o de participantes e seus acompanhantes da base legada</returns>
    /// <response code="200">Arquivo gerado com sucesso</response>
    /// <response code="500">Erro interno ao processar a requisiÃ§Ã£o</response>
    [HttpGet("relatorio/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ObterRelatorioPdf()
    {
        try
        {
            var (bytes, contentType, nomeArquivo) = await _legadoRelatorioService.ExportarPdfAsync();
            return File(bytes, contentType, nomeArquivo);
        }
        catch (Exception)
        {
            return StatusCode(500, new BaseResponse(500, "Ocorreu um erro interno ao gerar o relatÃ³rio da base de origem."));
        }
    }
}
