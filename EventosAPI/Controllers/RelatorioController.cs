using Eventos.Application.Enums;
using Eventos.Application.Interfaces;
using Eventos.Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace EventosAPI.Controllers
{
    /// <summary>
    /// Geração de relatórios do evento.
    /// Permite exportar a lista de convidados confirmados em PDF ou Excel.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Tags("Relatórios")]
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
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterRelatorioExcel()
        {
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarAsync(FormatoRelatorio.Excel);
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
            var (bytes, contentType, nomeArquivo) = await _relatorioService.ExportarAsync(FormatoRelatorio.Pdf);
            return File(bytes, contentType, nomeArquivo);
        }

        /// <summary>
        /// Retorna a lista final de confirmados em ordem alfabética com coluna de pagamento para preenchimento manual
        /// </summary>
        /// <returns>Lista de confirmados com campo de pago (checkbox)</returns>
        /// <response code="200">Lista retornada com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("lista-final")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ListaFinalConfirmadosResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterListaFinalConfirmados()
        {
            var response = await _relatorioService.ObterListaFinalConfirmadosAsync();
            return StatusCode(response.CodigoStatus, response);
        }

    }
}
