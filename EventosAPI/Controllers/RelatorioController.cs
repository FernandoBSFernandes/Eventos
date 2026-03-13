using Microsoft.AspNetCore.Mvc;
using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Response;
using EventosAPI.Reports;

namespace EventosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    public class RelatorioController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;
        private readonly IRelatorioEmailService _relatorioEmailService;

        public RelatorioController(IRelatorioService relatorioService, IRelatorioEmailService relatorioEmailService)
        {
            _relatorioService = relatorioService;
            _relatorioEmailService = relatorioEmailService;
        }

        /// <summary>
        /// Exporta o relatório de convidados confirmados em formato Excel (.xlsx)
        /// </summary>
        /// <returns>Arquivo Excel com a relação de participantes e seus acompanhantes</returns>
        /// <response code="200">Arquivo gerado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("excel")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
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
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioPdf()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarAsync(new RelatorioPdfExporter());
            return File(bytes, contentType, nomeArquivo);
        }

        /// <summary>
        /// Envia por e-mail o relatório de convidados confirmados em PDF e Excel para o organizador
        /// </summary>
        /// <returns>Confirmação de envio</returns>
        /// <response code="200">E-mail enviado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpPost("enviar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EnviarRelatorio()
        {
            try
            {
                await _relatorioEmailService.EnviarRelatorioConvidadosConfirmadosAsync();
                return Ok(new BaseResponse(200, "Relatório enviado por e-mail com sucesso."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse(500, $"Erro ao enviar o relatório: {ex.Message}"));
            }
        }
    }
}
