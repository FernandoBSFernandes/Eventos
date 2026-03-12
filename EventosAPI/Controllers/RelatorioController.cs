using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Response;
using EventosAPI.Reports;

namespace EventosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelatorioController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;

        public RelatorioController(IRelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        /// <summary>
        /// Exporta o relatório de convidados confirmados em formato Excel (.xlsx)
        /// </summary>
        /// <returns>Arquivo Excel com a relação de participantes e seus acompanhantes</returns>
        /// <response code="200">Arquivo gerado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("excel")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioExcel()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarAsync(new RelatorioExcelExporter());
            return File(bytes, contentType, nomeArquivo);
        }

        /// <summary>
        /// Exporta o relatório de convidados confirmados em formato PDF
        /// </summary>
        /// <returns>Arquivo PDF com a relação de participantes e seus acompanhantes</returns>
        /// <response code="200">Arquivo gerado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioPdf()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarAsync(new RelatorioPdfExporter());
            return File(bytes, contentType, nomeArquivo);
        }
    }
}
