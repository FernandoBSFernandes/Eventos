using Eventos.Application.Enums;
using Eventos.Application.Enums;
using Eventos.Application.Enums;
using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace EventosAPI.Controllers
{
    /// <summary>
    /// GeraÃ§Ã£o e envio de relatÃ³rios do evento.
    /// Permite exportar a lista de convidados confirmados em PDF ou Excel e enviÃ¡-la por e-mail.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Tags("RelatÃ³rios")]
    public class RelatorioController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;
        private readonly IRelatorioEmailService _relatorioEmailService;

        public RelatorioController(
            IRelatorioService relatorioService,
            IRelatorioEmailService relatorioEmailService)
        {
            _relatorioService = relatorioService;
            _relatorioEmailService = relatorioEmailService;
        }

        /// <summary>
        /// Exporta o relatÃ³rio de convidados confirmados em formato Excel (.xlsx)
        /// </summary>
        /// <returns>Arquivo Excel com a relaÃ§Ã£o de participantes e seus acompanhantes</returns>
        /// <response code="200">Arquivo gerado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisiÃ§Ã£o</response>
        [HttpGet("excel")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioExcel()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarAsync(FormatoRelatorio.Excel);
            return File(bytes, contentType, nomeArquivo);
        }

        /// <summary>
        /// Exporta o relatÃ³rio de convidados confirmados em formato PDF
        /// </summary>
        /// <returns>Arquivo PDF com a relaÃ§Ã£o de participantes e seus acompanhantes</returns>
        /// <response code="200">Arquivo gerado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisiÃ§Ã£o</response>
        [HttpGet("pdf")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioPdf()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarAsync(FormatoRelatorio.Pdf);
            return File(bytes, contentType, nomeArquivo);
        }

        /// <summary>
        /// Envia o relatÃ³rio de convidados confirmados por e-mail em PDF e Excel
        /// </summary>
        /// <returns>ConfirmaÃ§Ã£o de envio do e-mail</returns>
        /// <response code="200">E-mail enviado com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisiÃ§Ã£o</response>
        [HttpPost("enviar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EnviarRelatorio()
        {
            try
            {
                await _relatorioEmailService.EnviarRelatorioAsync();
                return Ok(new BaseResponse(200, "RelatÃ³rio enviado com sucesso."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponse(500, "Ocorreu um erro ao enviar o relatÃ³rio por e-mail. Tente novamente mais tarde."));
            }
        }
    }
}
