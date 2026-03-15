using Microsoft.AspNetCore.Mvc;
using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Response;
using EventosAPI.Reports;

namespace EventosAPI.Controllers
{
    /// <summary>
    /// Geração e envio de relatórios do evento.
    /// Permite exportar a lista de convidados confirmados em PDF ou Excel e enviá-la por e-mail.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Tags("Relatórios")]
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
        /// Envia o relatório de convidados confirmados por e-mail em PDF e Excel
        /// </summary>
        /// <returns>Confirmação de envio do e-mail</returns>
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
                await _relatorioEmailService.EnviarRelatorioAsync();
                return Ok(new BaseResponse(200, "Relatório enviado com sucesso."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponse(500, "Ocorreu um erro ao enviar o relatório por e-mail. Tente novamente mais tarde."));
            }
        }
    }
}
